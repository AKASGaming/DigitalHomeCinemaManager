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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using DigitalHomeCinemaControl.Collections;
    using DigitalHomeCinemaControl.Controllers.Base;
    using DigitalHomeCinemaControl.Controllers.Providers.Denon.Avr;
    using DigitalHomeCinemaControl.Controllers.Routing;

    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "<Pending>")]
    public class AvrController : ProcessorController, IRoutingDestination
    {

        #region Members

        private const int DEFAULT_PORT = 23;
        private const int VOLUME_SCALE = 80;

        private AvrClient avr;
        private ChannelBinding channelBinding;
        private IDictionary<string, Type> actions;

        #endregion

        #region Constructor

        public AvrController()
            : base()
        {
            // The ChannelBinding is a specialied implementation of the IBindingItem interface
            // which allows for the PropertyChanged event to be externally reset.
            // Keep a reference to the binding locally and add it to the DataSource collection.
            this.channelBinding = new ChannelBinding("Channels", this.Dispatcher) {
                ChannelStatus = new ChannelStatus()
            };

            this.Port = DEFAULT_PORT;
            this.HideUnusedOutputs = false;

            this.Delay = 0;
            this.MasterVolume = 0m - VOLUME_SCALE;

            // Since our UIElement is a container for multiple other controls, group our data
            // logically into a MultiplexedBindingList rather than the standard DispatchedBindingList
            var dataSource = new MultiplexedBindingList<IBindingItem> {
                { "Output", this.channelBinding },
                { "Audyssey", new BindingItem<string>("MultEQ") },
                { "Audyssey", new BindingItem<string>("Dyn EQ") },
                { "Audyssey", new BindingItem<string>("Dyn Vol") },
                { "Status", new BindingItem<string>("Input Mode") },
                { "Status", new BindingItem<string>("Dynamic Compression") },
                { "Status", new BindingItem<string>("Dialogue Enhancer") },
                { "Status", new BindingItem<string>("Audio Restorer") },
                { "Status", new BindingItem<string>("Tone Control") },
                { "Status", new BindingItem<string>("Loudness Management") },
                new BindingItem<string>("Surround Mode"),
                new BindingItem<string>("Input Source")
            };

            this.DataSource = dataSource;

            // Only a small number of the AvrClient properties currently support external changes.
            // More can be added if needed.
            this.actions = new Dictionary<string, Type> {
                { "QuickSelect", typeof(QuickSelect) },
                { "MasterVolume", typeof(decimal) },
                { "Delay", typeof(int) },
            };
        }

        #endregion

        #region Methods

        public override void Connect()
        {
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
                if (this.avr != null) {
                    this.avr.Close();
                }
            } catch { 
            } finally {
                this.avr.Dispose();
                OnDisconnected();
            }
            
        }

        private void ClientPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case "Power":
                    this.ControllerStatus = this.avr.Power.ToControllerStatus();
                    break;
                case "Delay":
                    this.Delay = this.avr.Delay;
                    break;
                case "MasterVolume":
                    this.MasterVolume = RelativeToAbsoluteVolume(VOLUME_SCALE, this.avr.MasterVolume);
                    break;
                case "Surround":
                    UpdateDataSource<string>("Surround Mode", this.avr.Surround.GetDescription());
                    break;
                case "ToneControl":
                    string toneControl = (this.avr.ToneControl == null) ? "---" : ((bool)this.avr.ToneControl == true) ? "On" : "Off";
                    UpdateDataSource<string>("Tone Control", toneControl);
                    break;
                case "LoudnessManagement":
                    string loudness = (this.avr.LoudnessManagement == null) ? "---" : ((bool)this.avr.LoudnessManagement == true) ? "On" : "Off";
                    UpdateDataSource<string>("Loudness Management", loudness);
                    break;
                case "InputMode":
                    UpdateDataSource<string>("Input Mode", this.avr.InputMode.GetDescription());
                    break;
                case "DynamicCompression":
                    UpdateDataSource<string>("Dynamic Compression", this.avr.DynamicCompression.GetDescription());
                    break;
                case "DialogueEnhancer":
                    UpdateDataSource<string>("Dialogue Enhancer", this.avr.DialogueEnhancer.GetDescription());
                    break;
                case "AudioRestorer":
                    UpdateDataSource<string>("Audio Restorer", this.avr.AudioRestorer.GetDescription());
                    break;
                case "MultEq":
                    UpdateDataSource<string>("MultEQ", this.avr.MultEq.GetDescription());
                    break;
                case "DynEq":
                    string dyneq = (this.avr.DynEq == null) ? "---" : ((bool)this.avr.DynEq == true) ? "On" : "Off";
                    UpdateDataSource<string>("Dyn EQ", dyneq);
                    break;
                case "DynamicVolume":
                    UpdateDataSource<string>("Dyn Vol", this.avr.DynamicVolume.GetDescription());
                    break;
                case "Input":
                    string inputName = this.avr.Input.ToString();
                    if (this.avr.InputNames.ContainsKey(inputName.ToUpperInvariant())) {
                        UpdateDataSource<string>("Input Source", this.avr.InputNames[inputName.ToUpperInvariant()]);
                    } else {
                        UpdateDataSource<string>("Input Source", this.avr.Input.GetDescription());
                    }
                    break;   
                case "ChannelStatus":
                    SetActiveSpeakers();
                    break;  
                case "SpeakerConfig":
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

            this.channelBinding.Reset("AvailableChannels");
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

            this.channelBinding.Reset("ActiveChannels");
        }

        private void ClientDisconnected(object sender, EventArgs e)
        {
            OnError(Properties.Resources.FMT_DISCONNECTED);
            this.ControllerStatus = ControllerStatus.Disconnected;

            // TODO: reconnect?
        }

        protected override void OnSettingChanged(string name)
        {
            if (!string.IsNullOrEmpty(name) && (name == "HideUnusedOutputs")) {
                Debug.Assert(this.channelBinding != null);

                this.channelBinding.ChannelStatus.HideUnusedChannels = this.HideUnusedOutputs;
                this.channelBinding.Reset("HideUnusedChannels");
            }
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        public string RouteAction(string action, object args)
        {
            if (string.IsNullOrEmpty(action)) { return "AVR ERROR: Invalid Action!"; }
            if (!this.avr.IsConnected) { return "AVR ERROR: Not connected!"; }

            switch (action) {
                case "QuickSelect":
                    if (Enum.TryParse(args.ToString(), out QuickSelect qs)) {
                        if (qs != QuickSelect.Unknown) {
                            this.avr.QuickSelect = qs;
                        }
                    }
                    break;
                case "MasterVolume":
                    int mv = AbsoluteToRelativeVolume(VOLUME_SCALE, (decimal)args);
                    this.avr.MasterVolume = mv;
                    break;
                case "Delay":
                    this.avr.Delay = (int)args;
                    break;
            }

            return "AVR OK.";
        }

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
