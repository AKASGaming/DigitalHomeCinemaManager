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
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using System.Xml;
using DigitalHomeCinemaControl.Controllers.Base;
using DigitalHomeCinemaControl.Controllers.Routing;

namespace DigitalHomeCinemaControl.Controllers.Providers.VLC
{
    /// <summary>
    /// VLC controller.
    /// </summary>
    public sealed class VLCController : SourceController, ISourceController, IRoutingSource, IRoutingDestination, IDisposable
    {

        #region Members

        private const string PROCESS_NAME    = "vlc";
        private const string DEFAULT_HOST    = "http://localhost";
        private const int    STATUS_INTERVAL = 1000;
        private const string VARIABLES       = "/requests/status.xml";
        private const string PLAYER_PARAMS   = " --http-host=localhost --fullscreen --no-crashdump -q --no-repeat --no-random --no-loop --one-instance --no-osd --qt-minimal-view --no-qt-name-in-title";
        private const string PORT_PARAM      = " --http-port=";
        private const string PASSWORD_PARAM  = " --http-password=";
        private const string DISPLAY_PARAM   = " --directx-device=";
        private const int    MAX_ERROR_COUNT = 10;

        private string Volume;

        private const string PLAY = "Play";
        private const string PAUSE = "Pause";
        private const string STOP = "Stop";

        private System.Timers.Timer statsTimer;
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
            string url = DEFAULT_HOST + ":" + Port + VARIABLES;
            //Console.WriteLine(url);

            string username = "";
            string password = VLCPassword;
            string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));

            WebRequest request = WebRequest.Create(new Uri(url));
            request.Headers.Add("Authorization", "Basic " + svcCredentials);
            HttpWebResponse response;

            try {
                response = (HttpWebResponse)request.GetResponse();
                //Console.WriteLine(response);
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

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(responseString);


            var fileName = doc.SelectSingleNode("/root/information/category/info[2]"); // information > category "meta" > filename
            var fileName2 = (fileName != null) ? fileName.InnerText : string.Empty;
            var volume = doc.GetElementsByTagName("volume")[0].InnerText;
            var size = doc.GetElementsByTagName("size");
            var size2 = (size[0] != null) ? size[0].InnerText : string.Empty;

            var time = doc.GetElementsByTagName("time")[0].InnerText;
            TimeSpan timeFormatted = TimeSpan.FromSeconds(int.Parse(time));
            var time2 = timeFormatted.ToString("hh\\:mm\\:ss");
            var length = doc.GetElementsByTagName("length")[0].InnerText;
            TimeSpan lengthFormatted = TimeSpan.FromSeconds(int.Parse(length));
            var length2 = lengthFormatted.ToString("hh\\:mm\\:ss");
            var state = doc.GetElementsByTagName("state")[0].InnerText;

            Volume = volume.ToString();

            if (Enum.TryParse<PlaybackState>(state, true, out PlaybackState s))
            {
                if ((s == PlaybackState.Playing) && (!string.IsNullOrEmpty(this.feature)) && this.feature.Contains(fileName2.ToString()))
                {
                    s = PlaybackState.PlayingFeature;
                }
                if ((s == PlaybackState.Playing) && (!string.IsNullOrEmpty(this.feature)) && this.feature.Contains(fileName2.ToString()))
                {
                    s = PlaybackState.PlayingFeature;
                }

                if (s != this.State)
                {
                    this.State = s;
                    OnDataReceived(s);
                }
            }

            UpdateDataSource<string>(CURRENTFILE, fileName2.ToString());
            UpdateDataSource<string>(FILESIZE, size2);
            UpdateDataSource<string>(POSITION, time2);
            UpdateDataSource<string>(DURATION, length2);

            if (int.TryParse(time, out int p))
            {
                UpdateDataSource<int>(CURRENTPOSITION, p);
            }
            if (int.TryParse(length, out int l))
            {
                UpdateDataSource<int>(LENGTH, l);
            }
        }

        public override void OpenPlaylist(string playlist, string feature = "")
        {
            this.feature = feature;
            if (!File.Exists(this.Path)) { return; }

            ProcessStartInfo vlcStart = new ProcessStartInfo {
                Arguments = playlist + PLAYER_PARAMS +
                    ((this.FullscreenDisplay >= 0) ? DISPLAY_PARAM + '"' + this.FullscreenDisplayID.Remove(0, 4) + '"' : string.Empty) + 
                    PORT_PARAM + Port + " " + PASSWORD_PARAM + '"' + this.VLCPassword + '"',
                FileName = this.Path
            };

            try {
                using (var vlc = new Process() { StartInfo = vlcStart }) {
                    vlc.Start();
                }
                Console.WriteLine("VLC Launched using command: " + vlcStart.Arguments.ToString());
                Connect();
                Stop();
                System.Threading.Thread.Sleep(500);
                Play();
                System.Threading.Thread.Sleep(500);
                Pause();
            } catch (Exception e) {
                OnError(Properties.Resources.MSG_VLC_START_ERROR);
                Console.WriteLine(e.ToString());
            }
        }

        public override void Play()
        {
            SendCommand("?command=pl_play");
        }

        public override void Pause()
        {
            if(this.State == PlaybackState.Paused)
            {
                return;
            }
            if (this.State == PlaybackState.Stopped)
            {
                return;
            }
            if (this.State == PlaybackState.Unknown)
            {
                return;
            }
            SendCommand("?command=pl_pause");
        }

        public override void Stop()
        {
            this.State = PlaybackState.Stopped;
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
                SendCommand("?command=volume&val=" + oldVol);
            }
            else if (onOff == true)
            {
                SendCommand("?command=volume&val=0");
            }


        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SendCommand(string command)
        {
            string playerUrl = DEFAULT_HOST + ":" + Port + VARIABLES;

            var values = new Dictionary<string, string> {
                { "vlc_command", command.ToString(CultureInfo.InvariantCulture) }
            };

            
            try {
                string url = DEFAULT_HOST + ":" + Port + VARIABLES + command;
                //Console.WriteLine(url);

                string username = "";
                string password = VLCPassword;
                string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));

                WebRequest request = WebRequest.Create(new Uri(url));
                request.Headers.Add("Authorization", "Basic " + svcCredentials);
                HttpWebResponse response;


                response = (HttpWebResponse)request.GetResponse();

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
