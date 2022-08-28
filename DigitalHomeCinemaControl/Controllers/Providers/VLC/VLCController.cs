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

namespace DigitalHomeCinemaControl.Controllers.Providers.VLC
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
    using System.Xml;
    using DigitalHomeCinemaControl.Controllers.Base;
    using DigitalHomeCinemaControl.Controllers.Routing;

    /// <summary>
    /// VLC controller.
    /// </summary>
    public sealed class VLCController : SourceController, ISourceController, IRoutingSource, IRoutingDestination, IDisposable
    {

        #region Members

        private const string PROCESS_NAME    = "vlc";
        private const string DEFAULT_HOST    = "http://localhost";
        private const int    DEFAULT_PORT    = 8080;
        private const int    HTTP_PASSWORD   = 13312;
        private const int    STATUS_INTERVAL = 1000;
        private const string VARIABLES = "/requests/status.xml";
        private const string PLAYER_PARAMS   = "--http-host http://localhost --http-port 8080 --intf dummy --dummy-quiet --fullscreen --no-crashdump";
        private const string DISPLAY_PARAM   = " /monitor ";
        private const int    MAX_ERROR_COUNT = 10;

        private string Volume;

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

        public VLCController()
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
            string url = DEFAULT_HOST + ":" + DEFAULT_PORT + VARIABLES;

            WebRequest request = WebRequest.Create(new Uri(url));
            HttpWebResponse response;

            try {
                response = (HttpWebResponse)request.GetResponse();
            } catch (WebException ex) {
                OnError(string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_VLC_CONNECT_ERROR, ex.Status));
                this.errorCount++;
                if (this.errorCount > MAX_ERROR_COUNT) {
                    OnError(Properties.Resources.MSG_VLC_MAX_ERRORS);
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

            var doc = new XmlDocument();
            doc.LoadXml(responseString);

            string currentFile = doc.GetElementById("root").InnerText;

            Volume = doc.GetElementById("volume").InnerText;

            if (Enum.TryParse<PlaybackState>(doc.GetElementById("state").InnerText, out PlaybackState s)) {
                if ((s >= PlaybackState.Playing) && (!string.IsNullOrEmpty(this.feature)) && this.feature.Contains(currentFile)) {
                    s = PlaybackState.PlayingFeature;
                }

                if (s != this.State) {
                    this.State = s;
                    OnDataReceived(s);
                }
            }

            UpdateDataSource<string>(CURRENTFILE, currentFile);
            UpdateDataSource<string>(FILESIZE, doc.GetElementById("size").InnerText);
            UpdateDataSource<string>(POSITION, doc.GetElementById("time").InnerText);
            UpdateDataSource<string>(DURATION, doc.GetElementById("length").InnerText);

            if (int.TryParse(doc.GetElementById("time").InnerText, out int p)) {
                UpdateDataSource<int>(CURRENTPOSITION, p);
            }
            if (int.TryParse(doc.GetElementById("length").InnerText, out int l)) {
                UpdateDataSource<int>(LENGTH, l);
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
                OnError(Properties.Resources.MSG_VLC_START_ERROR);
            }
        }

        public override void Play()
        {
            SendCommand("?command=pl_play");
        }

        public override void Pause()
        {
            SendCommand("?command=pl_pause");
        }

        public override void Stop()
        {
            SendCommand("?command=pl_stop");
        }

        public override void Rewind()
        {
            SendCommand("?command=seek&val=-10s");
        }

        public override void FastForward()
        {
            SendCommand("?command=seek&val=+10s");
        }

        public override void Previous()
        {
            SendCommand("?command=pl_previous");
        }

        public override void Next()
        {
            SendCommand("?command=pl_next");
        }

        public override void Subtitles()
        {

        }

        public override void Mute()
        {

            var oldVol = Volume;

            bool onOff = false; 

            if (onOff == false)
            {
                SendCommand("?command=volume&val=" + Volume);
            }
            else if (onOff == true)
            {
                SendCommand("?command=volume&val=0");
            }


        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SendCommand(string command)
        {
            string playerUrl = DEFAULT_HOST + ":" + DEFAULT_PORT;

            var values = new Dictionary<string, string> {
                { "vlc_command", command.ToString(CultureInfo.InvariantCulture) }
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
                return string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_VLC_ERROR,
                    Properties.Resources.MSG_INVALID_ACTION);
            }
            if (args == null) {
                return string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_VLC_ERROR,
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
                    return "VLC Unknown Action!";
            }

            return string.Format(CultureInfo.InvariantCulture, "VLC {0}", Properties.Resources.MSG_OK);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed) {
                if (disposing) {
                    this.statsTimer?.Dispose();
                }

                this.disposed = true;
                this.statsTimer = null;

                Process[] vlcProcs = Process.GetProcessesByName(PROCESS_NAME);
                foreach (var p in vlcProcs) {
                    p.Kill();
                }
            }
        }

        ~VLCController()
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
            get { return "VLC"; }
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
