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
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Timers;
    using DigitalHomeCinemaControl.Controllers.Base;
    using DigitalHomeCinemaControl.Controllers.Routing;

    /// <summary>
    /// Media Player Classic - Home Cinema controller.
    /// </summary>
    public class MpcController : SourceController, ISourceController, IRoutingSource
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

        #endregion

        #region Constructor

        public MpcController()
            : base()
        {
            this.statsTimer = new System.Timers.Timer {
                Interval = MpcController.STATUS_INTERVAL
            };
            this.statsTimer.Elapsed += StatsTimerElapsed;
            this.statsTimer.AutoReset = true;
            this.State = PlaybackState.Unknown;
        }

        #endregion

        #region Methods

        private void StatsTimerElapsed(object sender, ElapsedEventArgs e)
        {
            string url = DEFAULT_HOST + ":" + DEFAULT_PORT + MpcController.VARIABLES;

            WebRequest request = WebRequest.Create(url);
            HttpWebResponse response;

            try {
                response = (HttpWebResponse)request.GetResponse();
            } catch (WebException ex) {
                OnError("Failed to connect to MPC: " + ex.Status.ToString());
                this.errorCount++;
                if (this.errorCount > MAX_ERROR_COUNT) {
                    OnError("MPC: Max errors exceeded. Exiting.");
                    this.statsTimer.Stop();
                }
                return;
            }

            string responseString;

            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8)) {
                responseString = reader.ReadToEnd();
            }
            response.Close();

            ParseHtmlResponse(responseString);
        }

        private void ParseHtmlResponse(string responseString)
        {
            if (string.IsNullOrEmpty(responseString)) { return; }

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
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

        public override void Connect()
        {
            if (this.Dispatcher == null) {
                throw new InvalidOperationException();
            }

            this.statsTimer.Start();
            OnConnected();
        }

        public override void Disconnect()
        {
            Process[] mpcProcs = Process.GetProcessesByName(MpcController.PROCESS_NAME);

            try {
                this.statsTimer.Stop();

                foreach (var p in mpcProcs) {
                    p.Kill();
                }
            } catch { } finally {
                OnDisconnected();
            }
        }

        public override void OpenPlaylist(string playlist, string feature = "")
        {
            this.feature = feature;
            if (!File.Exists(this.Path)) { return; }

            ProcessStartInfo mpcStart = new ProcessStartInfo {
                Arguments = playlist + PLAYER_PARAMS +
                    ((this.FullscreenDisplay >= 0) ?
                        DISPLAY_PARAM + this.FullscreenDisplay.ToString() : string.Empty),
                FileName = this.Path
            };
            Process mpc = new Process {
                StartInfo = mpcStart
            };

            try {
                mpc.Start();
            } catch {
                OnError("Failed to start MPC process!");
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
                { "wm_command", command.ToString() }
            };
            var content = new FormUrlEncodedContent(values);
            try {
                var response = client.PostAsync(playerUrl, content);
            } catch { }
        }

        protected void OnDataReceived(PlaybackState data)
        {
            if (this.Dispatcher == null) {
                RouteData?.Invoke(this, new RoutingItem(this.Name, typeof(PlaybackState), data));
            } else {
                this.Dispatcher.BeginInvoke((Action)(() => {
                    RouteData?.Invoke(this, new RoutingItem(this.Name, typeof(PlaybackState), data));
                }));
            }
        }

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
