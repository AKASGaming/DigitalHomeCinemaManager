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

namespace DigitalHomeCinemaControl.Controllers.Providers.Denon
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using DigitalHomeCinemaControl.Collections;
    using DigitalHomeCinemaControl.Controllers.Base;
    using DigitalHomeCinemaControl.Controllers.Providers.Denon.Avr;
    using DigitalHomeCinemaControl.Controllers.Routing;

    public sealed class AvrController : ProcessorController, IRoutingDestination, IDisposable
    {

        #region Members

        private const int DEFAULT_PORT = 23;
        private const int VOLUME_SCALE = 80;

        public const string CHANNELS = "Channels";
        public const string OUTPUT = "Output";
        public const string AUDYSSEY = "Audyssey";
        public const string STATUS = "Status";
        public const string MULTEQ = "MultEQ";
        public const string DYNEQ = "Dyn EQ";
        public const string DYNVOL = "Dyn Vol";
        public const string INPUTMODE = "Input Mode";
        public const string DYNCOMP = "Dynamic Compression";
        public const string DIAENH = "Dialogue Enhancer";
        public const string AUDRSTR = "Audio Restorer";
        public const string TONECNT = "Tone Control";
        public const string LOUDMGMT = "Loudness Management";
        public const string SURROUNDMODE = "Surround Mode";
        public const string INPUTSOURCE = "Input Source";
        public const string QUICKSELECT = "Quick Select";

        private const string QUICK = "QuickSelect";
        private const string MV = "MasterVolume";
        private const string DEL = "Delay";

        private AvrClient avr;
        private ChannelBinding channelBinding;
        private IDictionary<string, Type> actions;
        private bool disposed = false; 

        #endregion

        #region Constructor

        public AvrController()
            : base()
        {
            // The ChannelBinding is a specialied implementation of the IBindingItem interface
            // which allows for the PropertyChanged event to be externally reset.
            // Keep a reference to the binding locally and add it to the DataSource collection.
            this.channelBinding = new ChannelBinding(CHANNELS, this.Dispatcher) {
                ChannelStatus = new ChannelStatus()
            };

            this.Port = DEFAULT_PORT;
            this.HideUnusedOutputs = false;

            this.Delay = 0;
            this.MasterVolume = 0m - VOLUME_SCALE;

            // Since our UIElement is a container for multiple other controls, group our data
            // logically into a MultiplexedBindingList rather than the standard DispatchedBindingList
            var dataSource = new MultiplexedBindingList<IBindingItem> {
                { OUTPUT, this.channelBinding },
                { AUDYSSEY, new BindingItem<string>(MULTEQ) },
                { AUDYSSEY, new BindingItem<string>(DYNEQ) },
                { AUDYSSEY, new BindingItem<string>(DYNVOL) },
                { STATUS, new BindingItem<string>(INPUTMODE) },
                { STATUS, new BindingItem<string>(DYNCOMP) },
                { STATUS, new BindingItem<string>(DIAENH) },
                { STATUS, new BindingItem<string>(AUDRSTR) },
                { STATUS, new BindingItem<string>(TONECNT) },
                { STATUS, new BindingItem<string>(LOUDMGMT) },
                new BindingItem<string>(SURROUNDMODE),
                new BindingItem<string>(INPUTSOURCE),
                new BindingItem<string>(QUICKSELECT)
            };

            this.DataSource = dataSource;

            // Only a small number of the AvrClient properties currently support external changes.
            // More can be added if needed.
            this.actions = new Dictionary<string, Type> {
                { QUICK, typeof(QuickSelect) },
                { MV, typeof(decimal) },
                { DEL, typeof(int) },
            };
        }

        #endregion

        #region Methods

        public override void Connect()
        {
            this.disposed = false;

            this.avr = new AvrClient() {
                Host = this.Host,
                Port = this.Port,
            };
            this.avr.Disconnected += ClientDisconnected;
            this.avr.PropertyChanged += ClientPropertyChanged;

            try {
                this.avr.Connect();
                OnConnected();
            } catch {
                this.ControllerStatus = ControllerStatus.Error;
                OnError(string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_NETWORK_TIMEOUT, "processor"));
            }
        }

        public override void Disconnect()
        {
            try {
                Dispose(true);
            } catch {
            } finally {
                OnDisconnected();
            }
        }

        private void ClientPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case nameof(this.avr.Power):
                    this.ControllerStatus = this.avr.Power.ToControllerStatus();
                    break;
                case nameof(this.avr.Delay):
                    this.Delay = this.avr.Delay;
                    break;
                case nameof(this.avr.MasterVolume):
                    this.MasterVolume = RelativeToAbsoluteVolume(VOLUME_SCALE, this.avr.MasterVolume);
                    break;
                case nameof(this.avr.Surround):
                    UpdateDataSource<string>(SURROUNDMODE, this.avr.Surround.GetDescription());
                    break;
                case nameof(this.avr.InputMode):
                    UpdateDataSource<string>(INPUTMODE, this.avr.InputMode.GetDescription());
                    break;
                case nameof(this.avr.DynamicCompression):
                    UpdateDataSource<string>(DYNCOMP, this.avr.DynamicCompression.GetDescription());
                    break;
                case nameof(this.avr.DialogueEnhancer):
                    UpdateDataSource<string>(DIAENH, this.avr.DialogueEnhancer.GetDescription());
                    break;
                case nameof(this.avr.AudioRestorer):
                    UpdateDataSource<string>(AUDRSTR, this.avr.AudioRestorer.GetDescription());
                    break;
                case nameof(this.avr.MultEq):
                    UpdateDataSource<string>(MULTEQ, this.avr.MultEq.GetDescription());
                    break;
                case nameof(this.avr.DynamicVolume):
                    UpdateDataSource<string>(DYNVOL, this.avr.DynamicVolume.GetDescription());
                    break;
                case nameof(this.avr.ToneControl):
                    string toneControl = (this.avr.ToneControl == null) ? "---" : 
                                         ((bool)this.avr.ToneControl == true) ? "On" : "Off";
                    UpdateDataSource<string>(TONECNT, toneControl);
                    break;
                case nameof(this.avr.LoudnessManagement):
                    string loudness = (this.avr.LoudnessManagement == null) ? "---" : 
                                      ((bool)this.avr.LoudnessManagement == true) ? "On" : "Off";
                    UpdateDataSource<string>(LOUDMGMT, loudness);
                    break;
                case nameof(this.avr.DynEq):
                    string dyneq = (this.avr.DynEq == null) ? "---" : 
                                   ((bool)this.avr.DynEq == true) ? "On" : "Off";
                    UpdateDataSource<string>(DYNEQ, dyneq);
                    break;
                case nameof(this.avr.Input):
                    string inputName = this.avr.Input.ToString();
                    if (this.avr.InputNames.ContainsKey(inputName.ToUpperInvariant())) {
                        UpdateDataSource<string>(INPUTSOURCE, this.avr.InputNames[inputName.ToUpperInvariant()]);
                    } else {
                        UpdateDataSource<string>(INPUTSOURCE, this.avr.Input.GetDescription());
                    }
                    break;
                case nameof(this.avr.QuickSelect):
                    string quickName = this.avr.QuickSelect.ToString();
                    if (this.avr.QuickNames.ContainsKey(quickName.ToUpperInvariant())) {
                        UpdateDataSource<string>(QUICKSELECT, this.avr.QuickNames[quickName.ToUpperInvariant()]);
                    } else {
                        UpdateDataSource<string>(QUICKSELECT, this.avr.QuickSelect.GetDescription());
                    }
                    break;
                case nameof(this.avr.ChannelStatus):
                    SetActiveSpeakers();
                    break;  
                case nameof(this.avr.SpeakerConfig):
                    SetAvailableSpeakers();
                    break;
            }
        }

        private void SetAvailableSpeakers()
        {
            Debug.Assert(this.channelBinding != null);

            var channelStatus = this.channelBinding.ChannelStatus;
            var channels = this.channelBinding.ChannelStatus.AvailableChannels;
            
            switch (this.avr.SpeakerConfig) {
                case SpeakerConfiguration.Floor:
                    channelStatus.ResetAvailableChannels(true);
                    channels[AudioChannel.TopBackLeft] = false;
                    channels[AudioChannel.TopBackRight] = false;
                    channels[AudioChannel.TopFrontLeft] = false;
                    channels[AudioChannel.TopFrontRight] = false;
                    channels[AudioChannel.TopMiddleLeft] = false;
                    channels[AudioChannel.TopMiddleRight] = false;
                    channels[AudioChannel.VoiceOfGod] = false;
                    break;
                case SpeakerConfiguration.Floor_Height:
                    channelStatus.ResetAvailableChannels(true);
                    channels[AudioChannel.VoiceOfGod] = false;
                    break;
                case SpeakerConfiguration.Front:
                    channelStatus.ResetAvailableChannels(false);
                    channels[AudioChannel.Center] = true;
                    channels[AudioChannel.Left] = true;
                    channels[AudioChannel.Right] = true;
                    channels[AudioChannel.Subwoofer] = true;
                    break;
                case SpeakerConfiguration.FrontHeight:
                    channelStatus.ResetAvailableChannels(false);
                    channels[AudioChannel.Center] = true;
                    channels[AudioChannel.Left] = true;
                    channels[AudioChannel.Right] = true;
                    channels[AudioChannel.Subwoofer] = true;
                    channels[AudioChannel.TopFrontLeft] = true;
                    channels[AudioChannel.TopFrontRight] = true;
                    channels[AudioChannel.SurroundLeft] = true;
                    channels[AudioChannel.SurroundRight] = true;
                    break;
                case SpeakerConfiguration.FrontHeight_FrontWide:
                    channelStatus.ResetAvailableChannels(false);
                    channels[AudioChannel.Center] = true;
                    channels[AudioChannel.Left] = true;
                    channels[AudioChannel.Right] = true;
                    channels[AudioChannel.Subwoofer] = true;
                    channels[AudioChannel.TopFrontLeft] = true;
                    channels[AudioChannel.TopFrontRight] = true;
                    channels[AudioChannel.SurroundLeft] = true;
                    channels[AudioChannel.SurroundRight] = true;
                    channels[AudioChannel.FrontWideLeft] = true;
                    channels[AudioChannel.FrontWideRight] = true;
                    break;
                case SpeakerConfiguration.FrontWide:
                    channelStatus.ResetAvailableChannels(false);
                    channels[AudioChannel.Center] = true;
                    channels[AudioChannel.Left] = true;
                    channels[AudioChannel.Right] = true;
                    channels[AudioChannel.Subwoofer] = true;
                    channels[AudioChannel.SurroundLeft] = true;
                    channels[AudioChannel.SurroundRight] = true;
                    channels[AudioChannel.FrontWideLeft] = true;
                    channels[AudioChannel.FrontWideRight] = true;
                    break;
                case SpeakerConfiguration.SurroundBack:
                    channelStatus.ResetAvailableChannels(false);
                    channels[AudioChannel.Center] = true;
                    channels[AudioChannel.Left] = true;
                    channels[AudioChannel.Right] = true;
                    channels[AudioChannel.Subwoofer] = true;
                    channels[AudioChannel.SurroundLeft] = true;
                    channels[AudioChannel.SurroundRight] = true;
                    channels[AudioChannel.SurroundBackLeft] = true;
                    channels[AudioChannel.SurroundBackRight] = true;
                    break;
                case SpeakerConfiguration.SurroundBack_FrontHeight:
                    channelStatus.ResetAvailableChannels(false);
                    channels[AudioChannel.Center] = true;
                    channels[AudioChannel.Left] = true;
                    channels[AudioChannel.Right] = true;
                    channels[AudioChannel.Subwoofer] = true;
                    channels[AudioChannel.TopFrontLeft] = true;
                    channels[AudioChannel.TopFrontRight] = true;
                    channels[AudioChannel.SurroundLeft] = true;
                    channels[AudioChannel.SurroundRight] = true;
                    channels[AudioChannel.SurroundBackLeft] = true;
                    channels[AudioChannel.SurroundBackRight] = true;
                    break;
                case SpeakerConfiguration.SurroundBack_FrontWide:
                    channelStatus.ResetAvailableChannels(false);
                    channels[AudioChannel.Center] = true;
                    channels[AudioChannel.Left] = true;
                    channels[AudioChannel.Right] = true;
                    channels[AudioChannel.Subwoofer] = true;
                    channels[AudioChannel.FrontWideLeft] = true;
                    channels[AudioChannel.FrontWideRight] = true;
                    channels[AudioChannel.SurroundLeft] = true;
                    channels[AudioChannel.SurroundRight] = true;
                    channels[AudioChannel.SurroundBackLeft] = true;
                    channels[AudioChannel.SurroundBackRight] = true;
                    break;
                default:
                    channelStatus.ResetAvailableChannels();
                    break;
            }

            this.channelBinding.Reset(nameof(this.channelBinding.ChannelStatus.AvailableChannels));
        }

        private void SetActiveSpeakers()
        {
            Debug.Assert(this.channelBinding != null);

            var channelStatus = this.channelBinding.ChannelStatus;
            var channels = this.channelBinding.ChannelStatus.ActiveChannels;

            channelStatus.ResetActiveChannels();

            // A mapping is needed to convert from Denon.Avr.Channel to DigitalHomeCinemaControl.AudioChannel
            // Different vendors may specify channels differently from each other and we want to have the
            // flexibility needed to support them in the future.
            Channel[] keys = new Channel[this.avr.ChannelStatus.Keys.Count];
            this.avr.ChannelStatus.Keys.CopyTo(keys, 0);
            int len = keys.Length;

            for (int i = len - 1; i >= 0; i--) {
                var key = keys[i].ToAudioChannel();
                if (channels.ContainsKey(key)) {
                    channels[key] = this.avr.ChannelStatus[keys[i]];
                }
            }

            this.channelBinding.Reset(nameof(this.channelBinding.ChannelStatus.ActiveChannels));
        }

        private void ClientDisconnected(object sender, EventArgs e)
        {
            OnError(Properties.Resources.FMT_DISCONNECTED);
            this.ControllerStatus = ControllerStatus.Disconnected;

            try {
                this.avr?.Close();
            } catch { }

            // TODO: reconnect?
        }

        protected override void OnSettingChanged(string name)
        {
            if (!string.IsNullOrEmpty(name) && (name == nameof(this.HideUnusedOutputs))) {
                Debug.Assert(this.channelBinding != null);

                this.channelBinding.ChannelStatus.HideUnusedChannels = this.HideUnusedOutputs;
                this.channelBinding.Reset(nameof(this.HideUnusedOutputs));
            }
        }

        public string RouteAction(string action, object args)
        {
            if (string.IsNullOrEmpty(action)) {
                return string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_AVR_ERROR, 
                    Properties.Resources.MSG_INVALID_ACTION);
            }
            if (!this.avr.IsConnected) { 
                return string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_AVR_ERROR, 
                    Properties.Resources.MSG_NOT_CONNECTED);
            }
            if (args == null) {
                return string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_AVR_ERROR, 
                    Properties.Resources.MSG_INVALID_ARGS);
            }

            switch (action) {
                case QUICK:
                    if (Enum.TryParse(args.ToString(), out QuickSelect qs)) {
                        if (qs != QuickSelect.Unknown) {
                            this.avr.QuickSelect = qs;
                        }
                    }
                    break;
                case MV:
                    int mv = AbsoluteToRelativeVolume(VOLUME_SCALE, (decimal)args);
                    this.avr.MasterVolume = mv;
                    break;
                case DEL:
                    this.avr.Delay = (int)args;
                    break;
            }

            return string.Format(CultureInfo.InvariantCulture, "AVR {0}", Properties.Resources.MSG_OK);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed) {
                if (disposing) {
                    this.avr?.Close();
                }

                this.avr = null;

                this.disposed = true;
            }
        }

        ~AvrController()
        {
            Dispose(false);
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
#pragma warning restore CA1063

        #endregion

        #region Properties

        public string Name
        {
            get { return "AVR"; }
        }

        public bool HideUnusedOutputs
        {
            get { return GetSetting<bool>(); }
            set { Setting<bool>(value); }
        }

        public IDictionary<string, Type> Actions
        {
            get { return this.actions; }
        }

        #endregion

    }

}
