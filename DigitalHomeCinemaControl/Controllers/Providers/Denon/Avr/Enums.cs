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

#pragma warning disable CA1707

namespace DigitalHomeCinemaControl.Controllers.Providers.Denon.Avr
{
    using System.ComponentModel;

    public enum InputSource
    {
        [Description("")]
        Unknown,
        [Description("Phono")]
        Phono,
        [Description("CD")]
        CD,
        [Description("Tuner")]
        Tuner,
        [Description("DVD")]
        Dvd,
        [Description("BluRay")]
        BluRay,
        [Description("Television")]
        TV,
        [Description("Satelite")]
        Satelite,
        [Description("Media Player")]
        MediaPlayer,
        [Description("Game")]
        Game,
        [Description("HD Radio")]
        HdRadio,
        [Description("HEOS")]
        Heos,
        [Description("Pandora")]
        Pandora,
        [Description("Sirius XM")]
        SiriusXm,
        [Description("Spotify")]
        Spotify,
        [Description("LastFM")]
        LastFm,
        [Description("Flickr")]
        Flickr,
        [Description("iRadio")]
        IRadio,
        [Description("Server")]
        Server,
        [Description("Favorites")]
        Favorites,
        [Description("Aux 1")]
        Aux1,
        [Description("Aux 2")]
        Aux2,
        [Description("Aux 3")]
        Aux3,
        [Description("Aux 4")]
        Aux4,
        [Description("Aux 5")]
        Aux5,
        [Description("Aux 6")]
        Aux6,
        [Description("Aux 7")]
        Aux7,
        [Description("Bluetooth")]
        Bluetooth,
        [Description("USB")]
        Usb
    }

    public enum DigitalInputMode
    {
        [Description("---")]
        Unknown,
        Auto,
        Pcm,
        Dts
    }

    public enum InputMode
    {
        [Description("---")]
        Unknown,
        Auto,
        [Description("HDMI")]
        Hdmi,
        Digital,
        Analog,
        [Description("External")]
        Ext1
    }

    public enum SurroundMode
    {
        [Description("---")]
        Unknown,
        [Description("Movie")]
        Movie,
        [Description("Music")]
        Music,
        [Description("Game")]
        Game,
        [Description("Direct")]
        Direct,
        [Description("Pure Direct")]
        PureDirect,
        [Description("Stereo")]
        Stereo,
        [Description("Auto")]
        Auto,
        [Description("Standard")]
        Standard,
        [Description("Dolby Digital")]
        DolbyDigital,
        [Description("Dolby Pro Logic")]
        DolbyProLogic,
        [Description("Dolby Pro Logic II")]
        DolbyProLogic2,
        [Description("Dolby Surround")]
        DolbySurround,
        [Description("Dolby Atmos")]
        DolbyAtmos,
        [Description("Dolby Digital EX")]
        DolbyDigitalEx,
        [Description("Dolby Digital + Pro Logic  II")]
        DolbyDigital_ProLogic2,
        [Description("Dolby Digital + Dolby Surround")]
        DolbyDigital_DolbySurround,
        [Description("Dolby Digital + Neo:X")]
        DolbyDigital_NeoX,
        [Description("Dolby Digital + Neural:X")]
        DolbyDigital_NeuralX,
        [Description("DTS Surround")]
        DtsSurround,
        [Description("DTS 6.1")]
        Dts_6_1,
        [Description("DTS Master")]
        DtsMaster,
        [Description("DTS + Pro Logic II")]
        Dts_ProLogic2,
        [Description("DTS + Dolby Surround")]
        Dts_DolbySurround,
        [Description("DTS + Neo:6")]
        Dts_Neo6,
        [Description("DTS + Neo:X")]
        Dts_NeoX,
        [Description("DTS + Neural:X")]
        Dts_NeuralX,
        [Description("DTS + Virtual:X")]
        Dts_VirtualX,
        [Description("Multi Channel")]
        MultiChannel,
        [Description("Multi Channel + Dolby EX")]
        MultiChannel_DolbyEx,
        [Description("Multi Channel + Pro Logic II")]
        MultiChannel_ProLogic2,
        [Description("Multi Channel + Dolby Surround")]
        MultiChannel_DolbySurround,
        [Description("Multi Channel 7.1")]
        MultiChannel_7_1,
        [Description("Multi Channel + Neo:X")]
        MultiChannel_NeoX,
        [Description("Multi Channel + Neural:X")]
        MultiChannel_NeuralX,
        [Description("Multi Channel + Virtual:X")]
        MultiChannel_VirtualX,
        [Description("Dolby Digital Plus")]
        DolbyDigitalPlus,
        [Description("Dolby Digital Plus + Dolby EX")]
        DolbyDigitalPlus_Ex,
        [Description("Dolby Digital Plus + Pro Logic II")]
        DolbyDigitalPlus_ProLogic2,
        [Description("Dolby Digital Plus + Dolby Surround")]
        DolbyDigitalPlus_DolbySurround,
        [Description("Dolby Digital Plus + Neo:X")]
        DolbyDigitalPlus_NeoX,
        [Description("Dolby Digital Plus + Neural:X")]
        DolbyDigitalPlus_NeuralX,
        [Description("Dolby TrueHD")]
        DolbyHD,
        [Description("Dolby TrueHD + Dolby EX")]
        DolbyHD_Ex,
        [Description("Dolby TrueHD + Pro Logic II")]
        DolbyHD_ProLogic2,
        [Description("Dolby TrueHD + Dolby Surround")]
        DolbyHD_DolbySurround,
        [Description("Dolby TrueHD + Neo:X")]
        DolbyHD_NeoX,
        [Description("DTS HD")]
        DtsHD,
        [Description("DTS HD Master")]
        DtsHDMaster,
        [Description("DTS HD + Pro Logic II")]
        DtsHD_ProLogic2,
        [Description("DTS HD + Dolby Surround")]
        DtsHD_DolbySurround,
        [Description("DTS HD + Neo:6")]
        DtsHD_Neo6,
        [Description("DTS HD + Neo:X")]
        DtsHD_NeoX,
        [Description("DTS HD + Neural:X")]
        DtsHD_NeuralX,
        [Description("DTS HD + Virtual:X")]
        DtsHD_VirtualX,
        [Description("DTS-X")]
        DtsX,
        [Description("DTS-X + Virtual:X")]
        DtsX_VirtualX,
        [Description("DTS EX Matrix + Neural:X")]
        DtsExMatrix_NeuralX,
        [Description("DTS EX Discreet + Neural:X")]
        DtsExDiscreet_NeuralX,
        [Description("DTS 96/24")]
        Dts96,
        [Description("DTS 96/24 Matrix")]
        Dts96Matrix,
        [Description("DTS-X Master")]
        DtsXMaster,
        [Description("DTS Express")]
        DtsExpress,
        [Description("DTS ES 8ch Discreet")]
        DtsEs8chDiscreet,
        [Description("Mpeg2 AAC")]
        Mpeg2AAC,
        [Description("AAC + Dolby EX")]
        AAC_DolbyEx,
        [Description("AAC + Pro Logic II")]
        AAC_ProLogic2,
        [Description("AAC + Dolby Surround")]
        AAC_DolbySurround,
        [Description("AAC + Neo:X")]
        AAC_NeoX,
        [Description("AAC + Neural:X")]
        AAC_NeuralX,
        [Description("AAC + Virtual:X")]
        AAC_VirtualX,
        [Description("Dolby Pro Logic + DSX")]
        ProLogic_DSX,
        [Description("Dolby Pro Logic II + DSX")]
        ProLogic2_DSX,
        [Description("DTS Neo:6 + DSX")]
        Neo6_DSX,
        [Description("Audyssey + DSX")]
        Audyssey_DSX,
        [Description("DTS Neural:X")]
        NeuralX,
        [Description("DTS Virtual:X")]
        VirtualX,
        [Description("Auro 3D")]
        Auro3d,
        [Description("Auro 2D Surround")]
        Auro2dSurround,
        [Description("Multi Channel Stereo")]
        MultiChannelStereo,
        [Description("Widescreen")]
        WideScreen,
        [Description("Super Stadium")]
        SuperStadium,
        [Description("Rock Arena")]
        RockArena,
        [Description("Jazz Club")]
        JazzClub,
        [Description("Classic Concert")]
        ClassicConcert,
        [Description("Mono Movie")]
        MonoMovie,
        [Description("Matrix")]
        Matrix,
        [Description("Video Game")]
        VideoGame,
        [Description("Virtual")]
        Virtual,
        [Description("Left")]
        Left,
        [Description("Right")]
        Right,
        [Description("All Zone Stereo")]
        AllZoneStereo
    }

    public enum MultEqMode
    {
        [Description("---")]
        Unknown,
        [Description("Reference")]
        Audyssey,
        Bypass,
        Flat,
        Manual,
        Off
    }

    public enum DynamicVolumeMode
    {
        [Description("---")]
        Unknown,
        Heavy,
        Medium,
        Light,
        Off
    }

    public enum DynamicCompressionMode
    {
        [Description("---")]
        Unknown,
        Auto,
        Low,
        Medium,
        High,
        Off
    }

    public enum DialogueEnhancerMode
    {
        [Description("---")]
        Unknown,
        Off,
        Low,
        Medium,
        High
    }

    public enum AudioRestorerMode
    {
        [Description("---")]
        Unknown,
        Off,
        Low,
        Medium,
        High
    }

    public enum SpeakerConfiguration
    {
        Unknown,
        FrontHeight,
        FrontWide,
        SurroundBack,
        FrontHeight_FrontWide,
        SurroundBack_FrontHeight,
        SurroundBack_FrontWide,
        Floor,
        Floor_Height,
        Front
    }

    public enum Channel
    {
        Left = 0,
        Right = 1,
        Center = 2,
        Subwoofer = 3,
        SurroundLeft = 4,
        SurroundRight = 5,
        SurroundBackLeft = 6,
        SurroundBackRight = 7,
        TopFrontLeft = 23,
        TopFrontRight = 24,
        TopBackLeft = 27,
        TopBackRight = 28,
        FrontWideLeft,
        FrontWideRight,
        TopMiddleLeft,
        TopMiddleRight,
        VoiceOfGod
    }

    public enum PowerStatus
    {
        Unknown,
        On,
        Standby
    }

    public enum QuickSelect
    {
        [Description("")]
        Unknown = 0,
        [Description("Quick Select 1")]
        Quick1 = 1,
        [Description("Quick Select 2")]
        Quick2 = 2,
        [Description("Quick Select 3")]
        Quick3 = 3,
        [Description("Quick Select 4")]
        Quick4 = 4,
    }

}

#pragma warning restore CA1707
