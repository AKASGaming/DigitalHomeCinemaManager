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

namespace DigitalHomeCinemaControl.Controllers.Providers.Denon.Avr
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    public class AvrClient : INotifyPropertyChanged, IDisposable
    {

        #region Members

        private const int DEFAULT_PORT = 23;
        private const int KEEPALIVE_INTERVAL = 5000;
        private const string INTERNAL_THREAD_NAME = "AVR_WORKER";

        private TcpClient client;
        private NetworkStream networkStream;
        private StreamReader reader;
        private StreamWriter writer;
        private Thread readThread;
        private bool initialized;
        private bool disposed = false;
        private System.Timers.Timer timer;

        private PowerStatus power = PowerStatus.Unknown;
        private InputSource input = InputSource.Unknown;
        private DigitalInputMode digitalInput = DigitalInputMode.Unknown;
        private InputMode inputMode = InputMode.Unknown;
        private SurroundMode surround = SurroundMode.Unknown;
        private MultEqMode multEq = MultEqMode.Unknown;
        private DynamicVolumeMode dynamicVolume = DynamicVolumeMode.Unknown;
        private DynamicCompressionMode dynamicCompression = DynamicCompressionMode.Unknown;
        private DialogueEnhancerMode dialogueEnhancer = DialogueEnhancerMode.Unknown;
        private AudioRestorerMode audioRestorer = AudioRestorerMode.Unknown;
        private SpeakerConfiguration speakerConfig = SpeakerConfiguration.Unknown;
        private QuickSelect quickSelect = QuickSelect.Unknown;
        private int masterVolume = -1;
        private int maxMasterVolume = -1;
        private int delay = -1;
        private bool? toneControl = null;
        private bool? cinemaEq = null;
        private bool? loudnessManagement = null;
        private bool? dynEq = null;
        private int referenceLevelOffset = -1;
        private Dictionary<string, string> inputNames = new Dictionary<string, string>();
        private Dictionary<string, string> quickNames = new Dictionary<string, string>();
        private bool inputNamesInit = false;
        private bool quickNamesInit = false;
        private Dictionary<Channel, bool> channelStatus = new Dictionary<Channel, bool>();

        #endregion

        #region Constructor

        public AvrClient()
        {
            this.Port = DEFAULT_PORT;

            this.client = new TcpClient() {
                NoDelay = true,
            };

            this.Closed = false;
            this.IsConnected = false;
        }

        #endregion

        #region Methods

        public void Connect()
        {
            if (string.IsNullOrEmpty(this.Host)) { throw new InvalidOperationException(Properties.Resources.MSG_INVALID_HOST); }
            if (this.Closed) { throw new ObjectDisposedException("SdcpClient", Properties.Resources.MSG_OBJECT_DISPOSED); }

            this.client.Connect(this.Host, this.Port);
            try {
                this.networkStream = this.client.GetStream();
                this.reader = new StreamReader(this.networkStream);
                this.writer = new StreamWriter(this.networkStream);
            } catch {
                this.Closed = true;
                return;
            }

            this.IsConnected = true;

            this.readThread = new Thread(ClientThread) {
                Name = INTERNAL_THREAD_NAME,
            };
            this.readThread.Start();

            this.timer = new System.Timers.Timer() {
                Interval = KEEPALIVE_INTERVAL,
                AutoReset = true,
            };
            this.timer.Elapsed += TimerElapsed;
            this.timer.Start();
        }

        public void Close()
        {
            this.IsConnected = false;

            if (this.timer != null && this.timer.Enabled) {
                this.timer.Stop();
            }

            try {
                this.readThread?.Abort();
                this.readThread?.Join();
            } catch { }

            try {
                Dispose(true);
            } catch {
            } finally {
                this.Closed = true;
            }
        }

        private void ClientThread()
        {
            char[] buffer = new char[1];
            StringBuilder sb;

            GetPowerStatus();
            UpdateState();

            while (this.IsConnected) {
                if (!this.networkStream.CanRead || !this.networkStream.CanWrite) {
                    OnDisconnected();
                    return;
                }

                sb = new StringBuilder();
                do {
                    try {
                        int bytesRead = this.reader.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0 && buffer[0] != '\r') {
                            sb.Append(buffer);
                        }
                    } catch { 
                        if (!this.IsConnected) {
                            return;
                        }
                    }
                } while (buffer[0] != '\r');

                OnDataReceived(sb.ToString());
            }
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.IsConnected) {
                this.writer.WriteLine("OPINFASP ?");
                this.writer.Flush();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateState()
        {
            this.writer.WriteLine("MS?");
            this.writer.WriteLine("MV?");
            this.writer.WriteLine("SI?");
            this.writer.WriteLine("SD?");
            this.writer.WriteLine("PSTONE CTRL ?");
            this.writer.WriteLine("PSCINEMA EQ. ?");
            this.writer.WriteLine("PSLOM ?");
            this.writer.WriteLine("PSSP: ?");
            this.writer.WriteLine("PSMULTEQ: ?");
            this.writer.WriteLine("PSDYNEQ ?");
            this.writer.WriteLine("PSREFLEV ?");
            this.writer.WriteLine("PSDYNVOL ?");
            this.writer.WriteLine("PSLFC ?");
            this.writer.WriteLine("PSDRC ?");
            this.writer.WriteLine("PSDEH ?");
            this.writer.WriteLine("PSDELAY ?");
            this.writer.WriteLine("PSRSTR ?");
            this.writer.WriteLine("OPINFASP ?");
            this.writer.Flush();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetPowerStatus()
        {
            this.writer.WriteLine("PW?");
            this.writer.Flush();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Initialize()
        {
            this.writer.WriteLine("SSFUN ?");
            this.writer.WriteLine("SSQSNZMA ?");
            this.writer.Flush();
        }

        private void OnDataReceived(string data)
        {
            if (string.IsNullOrEmpty(data)) { return; }

            // TODO: We can optimize this with Dictionaries and delegates

            string command = data.Substring(0, 2);
            string parameter = data.Substring(2).Trim();

            switch (command) {
                case "PW": // power
                    if (!this.initialized) {
                        this.initialized = true;
                        Initialize();
                    }
                    if (parameter == "ON") {
                        this.Power = PowerStatus.On;
                    } else if (parameter == "STANDBY") {
                        this.Power = PowerStatus.Standby;
                        this.Surround = SurroundMode.Unknown;
                    }
                    break;
                case "MS": // mode surround
                    SetSurroundMode(parameter);
                    break;
                case "SS": // undocumented commands
                    SetExtendedValues(parameter);
                    break;
                case "OP": // operational values - undocumented
                    SetOperationalValues(parameter);
                    break;
                case "MV": // master volume
                    int i;
                    if (parameter.Contains(" ")) {
                        string[] s = parameter.Split(' ');
                        if (s.Length > 1 && s[0] == "MAX") {
                            if (int.TryParse(s[1], out i)) {
                                this.MaxMasterVolume = i;
                            }
                        }
                    } else if (int.TryParse(parameter, out i)) {
                        this.MasterVolume = i;
                    }
                    break;
                case "SI": // select input source
                    SetSourceInput(parameter);
                    break;
                case "PS": // parameter setting
                    SetParameterValues(parameter);
                    break;
                case "SD": // input mode
                    switch (parameter) {
                        case "AUTO": this.InputMode = InputMode.Auto; break;
                        case "HDMI": this.InputMode = InputMode.Hdmi; break;
                        case "DIGITAL": this.InputMode = InputMode.Digital; break;
                        case "ANALOG": this.InputMode = InputMode.Analog; break;
                        case "EXT.IN-1": this.InputMode = InputMode.Ext1; break;
                    }
                    break;
                case "DC": // digital input mode
                    switch (parameter) {
                        case "AUTO": this.DigitalInput = DigitalInputMode.Auto; break;
                        case "PCM": this.DigitalInput = DigitalInputMode.Pcm; break;
                        case "DTS": this.DigitalInput = DigitalInputMode.Dts; break;
                    }
                    break;
                case "SV": // video select mode
                case "PV": // video parameter
                case "CV": // channel volume
                default:
                    break;

            }
        }

        private void SetSourceInput(string data)
        {
            if (string.IsNullOrEmpty(data)) { return; }

            switch (data) {
                case "PHONO": this.Input = InputSource.Phono; break;
                case "CD": this.Input = InputSource.CD; break;
                case "TUNER": this.Input = InputSource.Tuner; break;
                case "DVD": this.Input = InputSource.Dvd; break;
                case "BD": this.Input = InputSource.BluRay; break;
                case "TV": this.Input = InputSource.TV; break;
                case "SAT/CBL": this.Input = InputSource.Satelite; break;
                case "MPLAY": this.Input = InputSource.MediaPlayer; break;
                case "GAME": this.Input = InputSource.Game; break;
                case "HDRADIO": this.Input = InputSource.HdRadio; break;
                case "NET": this.Input = InputSource.Heos; break;
                case "PANDORA": this.Input = InputSource.Pandora; break;
                case "SIRIUSXM": this.Input = InputSource.SiriusXm; break;
                case "SPOTIFY": this.Input = InputSource.Spotify; break;
                case "LASTFM": this.Input = InputSource.LastFm; break;
                case "FLICKR": this.Input = InputSource.Flickr; break;
                case "IRADIO": this.Input = InputSource.IRadio; break;
                case "SERVER": this.Input = InputSource.Server; break;
                case "FAVORITES": this.Input = InputSource.Favorites; break;
                case "AUX1": this.Input = InputSource.Aux1; break;
                case "AUX2": this.Input = InputSource.Aux2; break;
                case "AUX3": this.Input = InputSource.Aux3; break;
                case "AUX4": this.Input = InputSource.Aux4; break;
                case "AUX5": this.Input = InputSource.Aux5; break;
                case "AUX6": this.Input = InputSource.Aux6; break;
                case "AUX7": this.Input = InputSource.Aux7; break;
                case "BT": this.Input = InputSource.Bluetooth; break;
                case "USB":
                case "USB/IPOD": this.Input = InputSource.Usb; break;
            }
        }

        private void SetSurroundMode(string data)
        {
            if (string.IsNullOrEmpty(data)) { return; }

            if (this.Power != PowerStatus.On) {
                this.Surround = SurroundMode.Unknown;
                return;
            }

            switch (data) {
                case "MOVIE": this.Surround = SurroundMode.Movie; break;
                case "MUSIC": this.Surround = SurroundMode.Music; break;
                case "GAME": this.Surround = SurroundMode.Game; break;
                case "DIRECT": this.Surround = SurroundMode.Direct; break;
                case "PURE DIRECT": this.Surround = SurroundMode.PureDirect; break;
                case "STEREO": this.Surround = SurroundMode.Stereo; break;
                case "AUTO": this.Surround = SurroundMode.Auto; break;
                case "DOLBY DIGITAL": this.Surround = SurroundMode.DolbyDigital; break;
                case "DOLBY PRO LOGIC": this.Surround = SurroundMode.DolbyProLogic; break;
                case "DOLBY PL2 C":
                case "DOLBY PL2 M":
                case "DOLBY PL2 G":
                case "DOLBY PL2X C":
                case "DOLBY PL2X M":
                case "DOLBY PL2X G":
                case "DOLBY PL2Z H": this.Surround = SurroundMode.DolbyProLogic2; break;
                case "DOLBY SURROUND": this.Surround = SurroundMode.DolbySurround; break;
                case "DOLBY ATMOS": this.Surround = SurroundMode.DolbyAtmos; break;
                case "DOLBY D EX": this.Surround = SurroundMode.DolbyDigitalEx; break;
                case "DOLBY D+PL2X C":
                case "DOLBY D+PL2X M":
                case "DOLBY D+PL2Z H": this.Surround = SurroundMode.DolbyDigital_ProLogic2; break;
                case "DOLBY D+DS": this.Surround = SurroundMode.DolbyDigital_DolbySurround; break;
                case "DOLBY D+NEO:X C":
                case "DOLBY D+NEO:X M":
                case "DOLBY D+NEO:X G": this.Surround = SurroundMode.DolbyDigital_NeoX; break;
                case "DOLBY D+NEURAL:X": this.Surround = SurroundMode.DolbyDigital_NeuralX; break;
                case "MSDOLBY D+": this.Surround = SurroundMode.DolbyDigitalPlus; break;
                case "DOLBY D+ +EX": this.Surround = SurroundMode.DolbyDigitalPlus_Ex; break;
                case "DOLBY D+ +PL2X C":
                case "DOLBY D+ +PL2X M":
                case "DOLBY D+ +PL2Z H": this.Surround = SurroundMode.DolbyDigitalPlus_ProLogic2; break;
                case "DOLBY D+ +DS": this.Surround = SurroundMode.DolbyDigitalPlus_DolbySurround; break;
                case "DOLBY D+ +NEO:X C":
                case "DOLBY D+ +NEO:X M":
                case "DOLBY D+ +NEO:X G": this.Surround = SurroundMode.DolbyDigitalPlus_NeoX; break;
                case "DOLBY D+ +NEURAL:X": this.Surround = SurroundMode.DolbyDigitalPlus_NeuralX; break;
                case "DOLBY HD": this.Surround = SurroundMode.DolbyHD; break;
                case "DOLBY HD+EX": this.Surround = SurroundMode.DolbyHD_Ex; break;
                case "DOLBY HD+PL2X C":
                case "DOLBY HD+PL2X M":
                case "DOLBY HD+PL2Z H": this.Surround = SurroundMode.DolbyHD_ProLogic2; break;
                case "DOLBY HD+DS": this.Surround = SurroundMode.DolbyHD_DolbySurround; break;
                case "DOLBY HD+NEO:X C":
                case "DOLBY HD+NEO:X M":
                case "DOLBY HD+NEO:X G": this.Surround = SurroundMode.DolbyHD_NeoX; break;
                case "DOLBY HD+NEURAL:X": this.Surround = SurroundMode.DolbyDigital_NeuralX; break;
                case "DTS SURROUND": this.Surround = SurroundMode.DtsSurround; break;
                case "DTS NEO:6 C":
                case "DTS NEO:6 M": this.Surround = SurroundMode.DtsHD_Neo6; break;
                case "DTS NEO:X C":
                case "DTS NEO:X M":
                case "DTS NEO:X G": this.Surround = SurroundMode.DtsHD_NeoX; break;
                case "NEURAL:X": this.Surround = SurroundMode.NeuralX; break;
                case "VIRTUAL:X": this.Surround = SurroundMode.VirtualX; break;
                case "DTS ES DSCRT6.1":
                case "DTS ES MTRX6.1": this.Surround = SurroundMode.Dts_6_1; break;
                case "DTS+PL2X C":
                case "DTS+PL2X M":
                case "DTS+PL2Z H": this.Surround = SurroundMode.Dts_ProLogic2; break;
                case "DTS+DS": this.Surround = SurroundMode.Dts_DolbySurround; break;
                case "DTS+NEO:6": this.Surround = SurroundMode.Dts_Neo6; break;
                case "DTS+NEO:X C":
                case "DTS+NEO:X M":
                case "DTS+NEO:X G": this.Surround = SurroundMode.Dts_NeoX; break;
                case "DTS+NEURAL:X": this.Surround = SurroundMode.Dts_NeuralX; break;
                case "DTS+VIRTUAL:X": this.Surround = SurroundMode.Dts_VirtualX; break;
                case "DTS ES MTRX+NEURAL:X": this.Surround = SurroundMode.DtsExMatrix_NeuralX; break;
                case "DTS ES DSCRT+NEURAL:X": this.Surround = SurroundMode.DtsExDiscreet_NeuralX; break;
                case "DTS96/24": this.Surround = SurroundMode.Dts96; break;
                case "DTS96 ES MTRX": this.Surround = SurroundMode.Dts96Matrix; break;
                case "DTS HD": this.Surround = SurroundMode.DtsHD; break;
                case "DTS HD MSTR": this.Surround = SurroundMode.DtsHDMaster; break;
                case "DTS HD+PL2X C":
                case "DTS HD+PL2X M":
                case "DTS HD+PL2Z H": this.Surround = SurroundMode.DtsHD_ProLogic2; break;
                case "DTS HD+DS": this.Surround = SurroundMode.DtsHD_DolbySurround; break;
                case "DTS HD+NEO:6": this.Surround = SurroundMode.DtsHD_Neo6; break;
                case "DTS HD+NEO:X C":
                case "DTS HD+NEO:X M":
                case "DTS HD+NEO:X G": this.Surround = SurroundMode.DtsHD_NeoX; break;
                case "DTS HD+NEURAL:X": this.Surround = SurroundMode.DtsHD_NeuralX; break;
                case "DTS HD+VIRTUAL:X": this.Surround = SurroundMode.DtsHD_VirtualX; break;
                case "DTS:X": this.Surround = SurroundMode.DtsX; break;
                case "DTS:X MSTR": this.Surround = SurroundMode.DtsXMaster; break;
                case "DTS:X+VIRTUAL:X": this.Surround = SurroundMode.DtsX_VirtualX; break;
                case "DTS EXPRESS": this.Surround = SurroundMode.DtsExpress; break;
                case "DTS ES 8CH DSCRT": this.Surround = SurroundMode.DtsEs8chDiscreet; break;
                case "MULTI CH IN": this.Surround = SurroundMode.MultiChannel; break;
                case "M CH IN+DOLBY EX": this.Surround = SurroundMode.MultiChannel_DolbyEx; break;
                case "M CH IN+PL2X C":
                case "M CH IN+PL2X M":
                case "M CH IN+PL2Z H": this.Surround = SurroundMode.MultiChannel_ProLogic2; break;
                case "M CH IN+DS": this.Surround = SurroundMode.MultiChannel_DolbySurround; break;
                case "M CH IN+NEO:X C":
                case "M CH IN+NEO:X M":
                case "M CH IN+NEO:X G": this.Surround = SurroundMode.MultiChannel_NeoX; break;
                case "M CH IN+NEURAL:X": this.Surround = SurroundMode.MultiChannel_NeuralX; break;
                case "M CH IN+VIRTUAL:X": this.Surround = SurroundMode.MultiChannel_VirtualX; break;
                case "MULTI CH IN 7.1": this.Surround = SurroundMode.MultiChannel_7_1; break;
                case "MPEG2 AAC": this.Surround = SurroundMode.Mpeg2AAC; break;
                case "AAC+DOLBY EX": this.Surround = SurroundMode.AAC_DolbyEx; break;
                case "AAC+PL2X C":
                case "AAC+PL2X M":
                case "AAC+PL2Z H": this.Surround = SurroundMode.AAC_ProLogic2; break;
                case "AAC+DS": this.Surround = SurroundMode.AAC_DolbySurround; break;
                case "AAC+NEO:X C":
                case "AAC+NEO:X M":
                case "AAC+NEO:X G": this.Surround = SurroundMode.AAC_NeoX; break;
                case "AAC+NEURAL:X": this.Surround = SurroundMode.AAC_NeuralX; break;
                case "AAC+VIRTUAL:X": this.Surround = SurroundMode.AAC_VirtualX; break;
                case "PL DSX": this.Surround = SurroundMode.ProLogic_DSX; break;
                case "PL2 C DSX":
                case "PL2 M DSX":
                case "PL2 G DSX": this.Surround = SurroundMode.ProLogic2_DSX; break;
                case "NEO:6 C DSX":
                case "NEO:6 M DSX": this.Surround = SurroundMode.Neo6_DSX; break;
                case "AUDYSSEY DSX": this.Surround = SurroundMode.Audyssey_DSX; break;
                case "AURO3D": this.Surround = SurroundMode.Auro3d; break;
                case "AURO2DSURR": this.Surround = SurroundMode.Auro2dSurround; break;
                case "MCH STEREO": this.Surround = SurroundMode.MultiChannelStereo; break;
                case "VIRTUAL": this.Surround = SurroundMode.Virtual; break;
                case "LEFT": this.Surround = SurroundMode.Left; break;
                case "RIGHT": this.Surround = SurroundMode.Right; break;
                case "WIDE SCREEN": this.Surround = SurroundMode.WideScreen; break;
                case "SUPER STADIUM": this.Surround = SurroundMode.SuperStadium; break;
                case "ROCK ARENA": this.Surround = SurroundMode.RockArena; break;
                case "JAZZ CLUB": this.Surround = SurroundMode.JazzClub; break;
                case "CLASSIC CONCERT": this.Surround = SurroundMode.ClassicConcert; break;
                case "MONO MOVIE": this.Surround = SurroundMode.MonoMovie; break;
                case "MATRIX": this.Surround = SurroundMode.Matrix; break;
                case "VIDEO GAME": this.Surround = SurroundMode.VideoGame; break;
                case "STANDARD": this.Surround = SurroundMode.Standard; break;
                case "ALL ZONE STEREO": this.Surround = SurroundMode.AllZoneStereo; break;
            }
        }

        private void SetExtendedValues(string data)
        {
            if (string.IsNullOrEmpty(data)) { return; }

            if (data.Contains("FUN")) {
                // source friendly names
                string s = data.Replace("FUN", "");
                if (s == " END") {
                    this.inputNamesInit = true;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(this.InputNames)));
                } else {
                    if (this.inputNamesInit == true) {
                        this.inputNames.Clear();
                        this.inputNamesInit = false;
                    }
                    string[] fun = s.Split(new char[] { ' ' }, 2);
                    if (fun.Length > 1) {
                        this.inputNames.Add(fun[0], fun[1]);
                    } else {
                        this.inputNames.Add(fun[0], fun[0]);
                    }
;
                }
            } else if (data.Contains("QSNZMA")) {
                // quick select names
                string s = data.Replace("QSNZMA", "");
                if (s == " END") {
                    this.quickNamesInit = true;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(this.QuickNames)));
                } else {
                    if (this.quickNamesInit == true) {
                        this.quickNames.Clear();
                        this.quickNamesInit = false;
                    }
                    string[] qsn = s.Split(new char[] { ' ' }, 2);
                    if (qsn.Length > 1) {
                        this.quickNames.Add(qsn[0], qsn[1]);
                    } else {
                        this.quickNames.Add(qsn[0], qsn[0]);
                    }
                }
            } else if (data.Contains(" ")) {
                string[] s = data.Split(' ');

                switch (s[0]) {
                    case "INFSIGRES": // video resolution    
                    case "AUDSTS":
                    case "AST":
                    case "ALSSET":
                    case "ALSDSP":
                    case "ALSVAL":
                    case "VCTZMAPON":
                    case "HOSIPS":
                    case "HOSIPM":
                    case "SMG":
                    case "INFAISSIG":
                    case "INFAISFSV":
                        break;
                }
            }
        }

        private void SetParameterValues(string data)
        {
            if (string.IsNullOrEmpty(data)) { return; }

            int i;

            if (data.Contains(" ")) {
                string[] s = data.Split(new char[] { ' ' }, 2);

                switch (s[0]) {
                    case "RSTR":
                        switch (s[1]) {
                            case "OFF": this.AudioRestorer = AudioRestorerMode.Off; break;
                            case "LOW": this.AudioRestorer = AudioRestorerMode.Low; break;
                            case "MED": this.AudioRestorer = AudioRestorerMode.Medium; break;
                            case "HI": this.AudioRestorer = AudioRestorerMode.High; break;
                        }
                        break;
                    case "DEH":
                        switch (s[1]) {
                            case "OFF": this.DialogueEnhancer = DialogueEnhancerMode.Off; break;
                            case "LOW": this.DialogueEnhancer = DialogueEnhancerMode.Low; break;
                            case "MED": this.DialogueEnhancer = DialogueEnhancerMode.Medium; break;
                            case "HIGH": this.DialogueEnhancer = DialogueEnhancerMode.High; break;
                        }
                        break;
                    case "DRC":
                        switch (s[1]) {
                            case "OFF": this.DynamicCompression = DynamicCompressionMode.Off; break;
                            case "AUTO": this.DynamicCompression = DynamicCompressionMode.Auto; break;
                            case "LOW": this.DynamicCompression = DynamicCompressionMode.Low; break;
                            case "MID": this.DynamicCompression = DynamicCompressionMode.Medium; break;
                            case "HI": this.DynamicCompression = DynamicCompressionMode.High; break;
                        }
                        break;
                    case "DYNEQ":
                        switch (s[1]) {
                            case "ON": this.DynEq = true; break;
                            case "OFF": this.DynEq = false; break;
                        }
                        break;
                    case "DYNVOL":
                        switch (s[1]) {
                            case "OFF": this.DynamicVolume = DynamicVolumeMode.Off; break;
                            case "HEV": this.DynamicVolume = DynamicVolumeMode.Heavy; break;
                            case "MED": this.DynamicVolume = DynamicVolumeMode.Medium; break;
                            case "LIT": this.DynamicVolume = DynamicVolumeMode.Light; break;
                        }
                        break;
                    case "CINEMA":
                        switch (s[1]) {
                            case "EQ.OFF": this.CinemaEq = false; break;
                            case "EQ.ON": this.CinemaEq = true; break;
                        }
                        break;
                    case "LOM":
                        switch (s[1]) {
                            case "OFF": this.LoudnessManagement = false; break;
                            case "ON": this.LoudnessManagement = true; break;
                        }
                        break;
                    case "REFLEV":
                        if (int.TryParse(s[1], out i)) {
                            this.ReferenceLevelOffset = i;
                        }
                        break;
                    case "DELAY":
                        if (int.TryParse(s[1], out i)) {
                            this.Delay = i;
                        }
                        break;
                    case "TONE":
                        switch (s[1]) {
                            case "CTRL OFF": this.ToneControl = false; break;
                            case "CTRL ON": this.ToneControl = true; break;
                        }
                        break;
                    case "LFE":
                    case "BAS":
                    case "TRE":
                    case "LFC":
                    case "CNTAMT":
                    case "HEQ":
                    case "CEN":
                    case "DIM":
                        break;
                }
            } else {
                switch (data) {
                    case "MULTEQ:AUDYSSEY": this.MultEq = MultEqMode.Audyssey; break;
                    case "MULTEQ:BYP.LR": this.MultEq = MultEqMode.Bypass; break;
                    case "MULTEQ:FLAT": this.MultEq = MultEqMode.Flat; break;
                    case "MULTEQ:OFF": this.MultEq = MultEqMode.Off; break;
                    case "MULTEQ:MANUAL": this.MultEq = MultEqMode.Manual; break;
                    case "SP:FL": this.SpeakerConfig = SpeakerConfiguration.Floor; break;
                    case "SP:HF": this.SpeakerConfig = SpeakerConfiguration.Floor_Height; break;
                    case "SP:FR": this.SpeakerConfig = SpeakerConfiguration.Front; break;
                    case "SP:FW": this.SpeakerConfig = SpeakerConfiguration.FrontWide; break;
                    case "SP:SB": this.SpeakerConfig = SpeakerConfiguration.SurroundBack; break;
                    case "SP:HW": this.SpeakerConfig = SpeakerConfiguration.FrontHeight_FrontWide; break;
                    case "SP:BH": this.SpeakerConfig = SpeakerConfiguration.SurroundBack_FrontHeight; break;
                    case "SP:BW": this.SpeakerConfig = SpeakerConfiguration.SurroundBack_FrontWide; break;
                }
            }
        }

        private void SetOperationalValues(string data)
        {
            if (string.IsNullOrEmpty(data)) { return; }

            if (data.Contains(" ")) {
                string[] s = data.Split(new char[] { ' ' }, 2);
                switch (s[0]) {
                    case "INFASP": // output channel bitmap
                        this.ChannelStatus.Clear();
                        for (int i = 0; i < 32; i++) {
                            string b = s[1].Substring(i, 1);
                            bool val = (b == "2") ? true : false;
                            Channel channel;

                            if (Enum.IsDefined(typeof(Channel), i)) {
                                channel = (Channel)i;
                                this.ChannelStatus.Add(channel, (this.Power == PowerStatus.On) ? val : false);
                            }
                        }
                        PropertyChanged(this, new PropertyChangedEventArgs(nameof(this.ChannelStatus)));
                        break;
                    case "INFINS": // input channel bitmap
                        break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SendValue(object value, [CallerMemberName] string name = null)
        {
            if (string.IsNullOrEmpty(name) || (value == null)) { return; }

            // prevent internal updates from triggering setting new value
            if (Thread.CurrentThread.Name.Equals(INTERNAL_THREAD_NAME, StringComparison.Ordinal)) { return; }

            string command = string.Empty;

            switch (name) {
                case "QuickSelect":
                    command = string.Format(CultureInfo.InvariantCulture, "MSQUICK{0}", ((int)value));
                    break;
                case "MasterVolume":
                    command = string.Format(CultureInfo.InvariantCulture, "MV{0}", value);
                    break;
                case "Delay":
                    command = string.Format(CultureInfo.InvariantCulture, "PSDELAY {0}", value);
                    break;
            }
             
            if (!string.IsNullOrEmpty(command)) {
                this.writer.WriteLine(command);
                this.writer.Flush();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            // prevent external updates from raising event
            if (!Thread.CurrentThread.Name.Equals(INTERNAL_THREAD_NAME, StringComparison.Ordinal)) { return; }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnDisconnected()
        {
            this.IsConnected = false;

            Disconnected?.Invoke(this, new EventArgs());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed) {
                if (disposing) {

                        this.reader?.Dispose();
                        this.writer?.Dispose();
                        this.networkStream?.Dispose();
                        this.timer?.Dispose();
                        this.client?.Close();
                }

                this.disposed = true;
                this.IsConnected = false;
                this.Closed = true;
                this.client = null;
                this.reader = null;
                this.writer = null;
                this.networkStream = null;
                this.readThread = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Events

        public event EventHandler Disconnected;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public string Host { get; set; }
        
        public int Port { get; set; }

        public bool IsConnected { get; private set; }

        public bool Closed { get; private set; }

        public PowerStatus Power
        {
            get { return this.power; }
            set {
                if (value != this.power) {
                    this.power = value;
                    OnPropertyChanged();
                    UpdateState();
                }
            }
        }

        public InputSource Input
        {
            get { return this.input; }
            set {
                if (value != this.input) {
                    this.input = value;
                    OnPropertyChanged();
                }
            }

        }

        public DigitalInputMode DigitalInput
        {
            get { return this.digitalInput; }
            set {
                if (value != this.digitalInput) {
                    this.digitalInput = value;
                    OnPropertyChanged();
                }
            }
        }

        public InputMode InputMode
        {
            get { return this.inputMode; }
            set {
                if (value != this.inputMode) {
                    this.inputMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public SurroundMode Surround
        {
            get { return this.surround; }
            set {
                if (value != this.surround) {
                    this.surround = value;
                    OnPropertyChanged();
                }
            }
        }

        public MultEqMode MultEq
        {
            get { return this.multEq; }
            set {
                if (value != this.multEq) {
                    this.multEq = value;
                    OnPropertyChanged();
                }
            }
        }

        public DynamicVolumeMode DynamicVolume
        {
            get { return this.dynamicVolume; }
            set {
                if (value != this.dynamicVolume) {
                    this.dynamicVolume = value;
                    OnPropertyChanged();
                }
            }
        }

        public DynamicCompressionMode DynamicCompression
        {
            get { return this.dynamicCompression; }
            set {
                if (value != this.dynamicCompression) {
                    this.dynamicCompression = value;
                    OnPropertyChanged();
                }
            }
        }

        public DialogueEnhancerMode DialogueEnhancer
        {
            get { return this.dialogueEnhancer; }
            set {
                if (value != this.dialogueEnhancer) {
                    this.dialogueEnhancer = value;
                    OnPropertyChanged();
                }
            }
        }

        public AudioRestorerMode AudioRestorer
        {
            get { return this.audioRestorer; }
            set {
                if (value != this.audioRestorer) {
                    this.audioRestorer = value;
                    OnPropertyChanged();
                }
            }
        }

        public SpeakerConfiguration SpeakerConfig
        {
            get { return this.speakerConfig; }
            set {
                if (value != this.speakerConfig) {
                    this.speakerConfig = value;
                    OnPropertyChanged();
                }
            }
        }

        public QuickSelect QuickSelect
        {
            get { return this.quickSelect; }
            set {
                if (value != this.quickSelect) {
                    this.quickSelect = value;
                    SendValue(value);
                    OnPropertyChanged();
                }
            }
        }

        public int MasterVolume
        {
            get { return this.masterVolume; }
            set {
                if (this.power != PowerStatus.On) {
                    value = 0;
                }
                if (value != this.masterVolume) {
                    this.masterVolume = value;
                    SendValue(value);
                    OnPropertyChanged();
                }
            }
        }

        public int MaxMasterVolume
        {
            get { return this.maxMasterVolume; }
            set {
                if (value != this.maxMasterVolume) {
                    this.maxMasterVolume = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Delay
        {
            get { return this.delay; }
            set {
                if (value != this.delay) {
                    this.delay = value;
                    SendValue(value);
                    OnPropertyChanged();
                }
            }
        }

        public bool? ToneControl
        {
            get { return this.toneControl; }
            set {
                if (value != this.toneControl) {
                    this.toneControl = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool? CinemaEq
        {
            get { return this.cinemaEq; }
            set {
                if (value != this.cinemaEq) {
                    this.cinemaEq = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool? LoudnessManagement
        {
            get { return this.loudnessManagement; }
            set {
                if (value != this.loudnessManagement) {
                    this.loudnessManagement = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool? DynEq
        {
            get { return this.dynEq; }
            set {
                if (value != this.dynEq) {
                    this.dynEq = value;
                    OnPropertyChanged();
                }
            }
        }

        public int ReferenceLevelOffset
        {
            get { return this.referenceLevelOffset; }
            set {
                if (value != this.referenceLevelOffset) {
                    this.referenceLevelOffset = value;
                    OnPropertyChanged();
                }
            }
        }

        public Dictionary<Channel, bool> ChannelStatus
        {
            get { return this.channelStatus; }
        }

        public Dictionary<string, string> InputNames
        {
            get { return this.inputNames; }
        }

        public Dictionary<string, string> QuickNames
        {
            get { return this.quickNames; }
        }

        #endregion

    }

}
