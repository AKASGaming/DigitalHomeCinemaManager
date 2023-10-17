/*
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for
 * more details.
 *
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using DigitalHomeCinemaControl.Controllers.Base;
using DigitalHomeCinemaControl.Controllers.Routing;

namespace DigitalHomeCinemaControl.Controllers.Providers.MediaPlayerClassic
{
    /// <summary>
    /// Media Player Classic - Home Cinema controller.
    /// </summary>
    public sealed class MpcController : SourceController, ISourceController, IRoutingSource, IRoutingDestination, IDisposable
    {

        #region Members

        private const string PROCESS_NAME    = "mpc-hc64";
        private const string DEFAULT_HOST    = "http://localhost";
        private const string VARIABLES       = "/variables.html";
        private const string COMMANDS        = "/command.html";
        private const int    STATUS_INTERVAL = 1000;
        private const string PLAYER_PARAMS   = " /open /fullscreen /nocrashreporter";
        private const string DISPLAY_PARAM   = " /monitor ";
        private const int    MAX_ERROR_COUNT = 10;

        private const string PLAY = "Play";
        private const string PAUSE = "Pause";
        private const string STOP = "Stop";

        private Timer statsTimer;
        private IDictionary<string, Type> actions;
        private static readonly HttpClient client = new HttpClient();
        private string feature = string.Empty;
        private volatile int errorCount;
        private volatile bool disposed;

        #endregion

        #region Constructor

        public MpcController()
            : base()
        {
            this.State = PlaybackState.Unknown;

            this.actions = new Dictionary<string, Type> {
                { PLAY, null },
                { PAUSE, null },
                { STOP, null },
            };
        }

        #endregion

        #region Methods

        public override void Connect()
        {
            if (client == null) {
                throw new InvalidOperationException();
            }
            this.disposed = false;

            this.statsTimer = new System.Timers.Timer {
                Interval = STATUS_INTERVAL,
                AutoReset = true
            };
            this.statsTimer.Elapsed += StatsTimerElapsed;
            this.statsTimer.Start();

            OnConnected();
        }

        public override void Disconnect()
        {
            if ((this.statsTimer != null) && this.statsTimer.Enabled) {
                this.statsTimer.Stop();
            }
           
            try {
                Dispose(true);
            } catch { 
            } finally {
                OnDisconnected();
            }
        }

        private void StatsTimerElapsed(object sender, ElapsedEventArgs e)
        {
            string url = DEFAULT_HOST + ":" + Port + VARIABLES;

            WebRequest request = WebRequest.Create(new Uri(url));
            HttpWebResponse response;

            try {
                response = (HttpWebResponse)request.GetResponse();
            } catch (WebException ex) {
                OnError(string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_MPC_CONNECT_ERROR, ex.Status));
                this.errorCount++;
                if (this.errorCount > MAX_ERROR_COUNT) {
                    OnError(Properties.Resources.MSG_MPC_MAX_ERRORS);
                    this.statsTimer.Stop();
                }
                return;
            }

            string responseString;

            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8)) {
                responseString = reader.ReadToEnd();
            }
            response.Close();

            ParseHtmlResponse(responseString);
        }

        private void ParseHtmlResponse(string responseString)
        {
            if (string.IsNullOrEmpty(responseString)) { return; }

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(responseString);

            string currentFile = doc.GetElementbyId("file").InnerText;

            if (Enum.TryParse<PlaybackState>(doc.GetElementbyId("statestring").InnerText, out PlaybackState s)) {
                if ((s >= PlaybackState.Playing) && (!string.IsNullOrEmpty(this.feature)) && this.feature.Contains(currentFile)) {
                    s = PlaybackState.PlayingFeature;
                }

                if (s != this.State) {
                    this.State = s;
                    OnDataReceived(s);
                }
            }

            UpdateDataSource<string>(CURRENTFILE, currentFile);
            UpdateDataSource<string>(FILESIZE, doc.GetElementbyId("size").InnerText);
            UpdateDataSource<string>(POSITION, doc.GetElementbyId("positionstring").InnerText);
            UpdateDataSource<string>(DURATION, doc.GetElementbyId("durationstring").InnerText);

            if (int.TryParse(doc.GetElementbyId("position").InnerText, out int p)) {
                UpdateDataSource<int>(CURRENTPOSITION, p);
            }
            if (int.TryParse(doc.GetElementbyId("duration").InnerText, out int l)) {
                UpdateDataSource<int>(LENGTH, l);
            }
        }

        public override void OpenPlaylist(string playlist, string feature = "")
        {
            this.feature = feature;
            if (!File.Exists(this.Path)) { return; }

            ProcessStartInfo mpcStart = new ProcessStartInfo {
                Arguments = playlist + PLAYER_PARAMS +
                    ((this.FullscreenDisplay >= 0) ? DISPLAY_PARAM + (this.FullscreenDisplay + 1): string.Empty),
                FileName = this.Path
            };

            try {
                using (var mpc = new Process() { StartInfo = mpcStart }) {
                    mpc.Start();
                }
                Console.WriteLine("MPC Launched using command: " + mpcStart.Arguments.ToString());
                Connect();
            } catch {
                OnError(Properties.Resources.MSG_MPC_START_ERROR);
            }
        }

        public override void Play()
        {
            SendCommand(887);
        }

        public override void Pause()
        {
            SendCommand(888);
        }

        public override void Stop()
        {
            SendCommand(890);
        }

        public override void Rewind()
        {
            SendCommand(903);
        }

        public override void FastForward()
        {
            SendCommand(904);
        }

        public override void Previous()
        {
            SendCommand(921);
        }

        public override void Next()
        {
            SendCommand(922);
        }

        public override void Subtitles()
        {
            SendCommand(956);
        }

        public override void Mute()
        {
            SendCommand(909);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SendCommand(int command)
        {
            string playerUrl = DEFAULT_HOST + ":" + Port + COMMANDS;

            var values = new Dictionary<string, string> {
                { "wm_command", command.ToString(CultureInfo.InvariantCulture) }
            };

            
            try {
#pragma warning disable CA2000 // Dispose objects before losing scope
                var content = new FormUrlEncodedContent(values);

                _ = client.PostAsync(new Uri(playerUrl), content).ContinueWith((requestTask) => {
                    content.Dispose();
                }, TaskScheduler.Current);

#pragma warning restore CA2000 // Dispose objects before losing scope
            } catch { }
        }

        private void OnDataReceived(PlaybackState data)
        {
            var item = new RoutingItem(this.Name, typeof(PlaybackState), data);
            RouteData?.Invoke(this, new RoutingDataEventArgs(item));
        }

        public string RouteAction(string action, object args)
        {
            if (string.IsNullOrEmpty(action)) {
                return string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_MPC_ERROR,
                    Properties.Resources.MSG_INVALID_ACTION);
            }
            if (args == null) {
                return string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_MPC_ERROR,
                    Properties.Resources.MSG_INVALID_ARGS);
            }

            switch (action) {
                case PLAY:
                    Play();
                    break;
                case PAUSE:
                    Pause();
                    break;
                case STOP:
                    Stop();
                    break;
                default:
                    return "MPC Unknown Action!";
            }

            return string.Format(CultureInfo.InvariantCulture, "MPC {0}", Properties.Resources.MSG_OK);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed) {
                if (disposing) {
                    this.statsTimer?.Dispose();
                }

                this.disposed = true;
                this.statsTimer = null;

                Process[] mpcProcs = Process.GetProcessesByName(PROCESS_NAME);
                foreach (var p in mpcProcs) {
                    p.Kill();
                }
            }
        }

        ~MpcController()
        {
            Dispose(false);
            client?.Dispose();
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
#pragma warning restore CA1063

        #endregion

        #region Events

        public event EventHandler<RoutingDataEventArgs> RouteData;

        #endregion

        #region Properties

        public string Name
        {
            get { return "MPC-HC"; }
        }

        public Type MatchType
        {
            get { return typeof(PlaybackState); }
        }

        public IDictionary<string, Type> Actions
        {
            get { return this.actions; }
        }

        #endregion

    }

}
