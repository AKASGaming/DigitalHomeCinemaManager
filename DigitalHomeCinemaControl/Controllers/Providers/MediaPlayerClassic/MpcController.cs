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

namespace DigitalHomeCinemaControl.Controllers.Providers.MediaPlayerClassic
{
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


    /// <summary>
    /// Media Player Classic - Home Cinema controller.
    /// </summary>
    public sealed class MpcController : SourceController, ISourceController, IRoutingSource, IDisposable
    {

        #region Members

        private const string PROCESS_NAME    = "mpc-hc64";
        private const string DEFAULT_HOST    = "http://localhost";
        private const int    DEFAULT_PORT    = 13579;
        private const string VARIABLES       = "/variables.html";
        private const string COMMANDS        = "/command.html";
        private const int    STATUS_INTERVAL = 1000;
        private const string PLAYER_PARAMS   = " /open /fullscreen /nocrashreporter";
        private const string DISPLAY_PARAM   = " /monitor ";
        private const int    MAX_ERROR_COUNT = 10;

        private Timer statsTimer;
        private static readonly HttpClient client = new HttpClient();
        private string feature = "";
        private int errorCount = 0;
        private bool disposed = false;

        #endregion

        #region Constructor

        public MpcController()
            : base()
        {
            this.State = PlaybackState.Unknown;
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
            Process[] mpcProcs = Process.GetProcessesByName(MpcController.PROCESS_NAME);

            if ((this.statsTimer != null) && this.statsTimer.Enabled) {
                this.statsTimer.Stop();
            }

            try {
                foreach (var p in mpcProcs) {
                    p.Kill();
                }

                Dispose(true);
            } catch { 
            } finally {
                OnDisconnected();
            }
        }

        private void StatsTimerElapsed(object sender, ElapsedEventArgs e)
        {
            string url = DEFAULT_HOST + ":" + DEFAULT_PORT + VARIABLES;

            WebRequest request = WebRequest.Create(new Uri(url));
            HttpWebResponse response;

            try {
                response = (HttpWebResponse)request.GetResponse();
            } catch (WebException ex) {
                OnError(string.Format(CultureInfo.InvariantCulture, "Failed to connect to MPC: {0}", ex.Status));
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
                if ((s == PlaybackState.Playing) && (!string.IsNullOrEmpty(this.feature)) && this.feature.Contains(currentFile)) {
                    s = PlaybackState.PlayingFeature;
                }
                this.State = s;
                OnDataReceived(s);
            }

            UpdateDataSource<string>("CurrentFile", currentFile);
            UpdateDataSource<string>("FileSize", doc.GetElementbyId("size").InnerText);
            UpdateDataSource<string>("Position", doc.GetElementbyId("positionstring").InnerText);
            UpdateDataSource<string>("Duration", doc.GetElementbyId("durationstring").InnerText);

            if (int.TryParse(doc.GetElementbyId("position").InnerText, out int p)) {
                UpdateDataSource<int>("CurrentPosition", p);
            }
            if (int.TryParse(doc.GetElementbyId("duration").InnerText, out int l)) {
                UpdateDataSource<int>("Length", l);
            }
        }

        public override void OpenPlaylist(string playlist, string feature = "")
        {
            this.feature = feature;
            if (!File.Exists(this.Path)) { return; }

            ProcessStartInfo mpcStart = new ProcessStartInfo {
                Arguments = playlist + PLAYER_PARAMS +
                    ((this.FullscreenDisplay >= 0) ?
                        DISPLAY_PARAM + this.FullscreenDisplay.ToString(CultureInfo.InvariantCulture) : string.Empty),
                FileName = this.Path
            };

            try {
                using (var mpc = new Process() { StartInfo = mpcStart }) {
                    mpc.Start();
                } 
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
            string playerUrl = DEFAULT_HOST + ":" + DEFAULT_PORT + COMMANDS;

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
            RouteData?.Invoke(this, new RoutingItem(this.Name, typeof(PlaybackState), data));
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed) {
                if (disposing) {
                    this.statsTimer?.Dispose();
                }

                this.disposed = true;
                this.statsTimer = null;
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

        public event EventHandler<RoutingItem> RouteData;

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

        #endregion

    }

}
