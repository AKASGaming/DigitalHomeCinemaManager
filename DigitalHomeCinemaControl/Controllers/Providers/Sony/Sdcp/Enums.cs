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

namespace DigitalHomeCinemaControl.Controllers.Providers.Sony.Sdcp
{
    using System.ComponentModel;

#pragma warning disable CA1707 // Remove underscores from member names
#pragma warning disable CA1028 // Enum storage should be Int32
    public enum RequestType : byte
    {
        Set = 0x0,
        Get = 0x1
    }

    public enum SdcpResult : byte
    {
        OK = 0x1,
        ERROR = 0x0
    }
#pragma warning restore CA1028

    public enum SdcpError
    {
        ItemError = 0x0101,
        InvalidItemRequest = 0x0102,
        InvalidLength = 0x0103,
        InvalidData = 0x0104,
        ShortData = 0x0111,
        NotApplicableItem = 0x0180,
        CommunityError = 0x0201,
        RequestError = 0x1001,
        InvalidCategory = 0x1002,
        InvalidRequest = 0x1003,
        ShortHeader = 0x1011,
        ShortCommunity = 0x1012,
        ShortCommand = 0x1013,
        NetworkErrorTimeout = 0x2001,
        CommErrorTimeout = 0xF001,
        ChecksumError = 0xF010,
        FramingError = 0xF020,
        ParityError = 0xF030,
        OverrunError = 0xF040,
        OtherCommError = 0xF050,
        UnknownResponse = 0xF0F0,
        NvramError = 0xF110,
        WriteError = 0xF120
    }

    public enum CommandItem
    {
        StatusPower = 0x0102,
        LampTimer = 0x0113,
        Power = 0x0130,
        CalibrationPreset = 0x0002,
        Contrast = 0x0010,
        Brightness = 0x0011,
        Color = 0x0012,
        Hue = 0x0013,
        Sharpness = 0x0014,
        ColorTemp = 0x0017,
        LampControl = 0x001A,
        ContrastEnhancer = 0x001C,
        AdvancedIris = 0x001D,
        FilmMode = 0x001F,
        GammaCorrection = 0x0022,
        NoiseReduction = 0x0025,
        ColorSpace = 0x003B,
        MotionFlow = 0x0059,
        xvColor = 0x005A,
        RealityCreation = 0x0067,
        Resolution = 0x0068,
        SmoothGradation = 0x006D,
        RealityCreationDatabase = 0x0075,
        HDR = 0x007C,
        ColorCorrection = 0x0086,
        InputLagReduction = 0x0099,
        AspectRatio = 0x0020,
        Input = 0x0001,
        SettingsLock = 0x0073,
        DisplaySelect3D = 0x0060,
        Format3D = 0x0061,
        DepthAdjust3D = 0x0062,
        Simulated3DEffect = 0x0063,
        Brightness3D = 0x0072,
        ModelName = 0x8001
    }

    public enum Power
    {
        Off = 0x00,
        On = 0x01
    }

    public enum StatusPower
    {
        Standby = 0x0000,
        Startup = 0x0001,
        StartupLamp = 0x0002,
        [Description("On")]
        PowerOn = 0x0003,
        Cooling1 = 0x0004,
        Cooling2 = 0x0005,
        Unknown
    }

    public enum CalibrationPreset
    {
        [Description("Cinema Film 1")]
        CinemaFilm1 = 0x0000,
        [Description("Cinema Film 2")]
        CinemaFilm2 = 0x0001,
        Reference = 0x0002,
        [Description("TV")]
        Tv = 0x0003,
        Photo = 0x0004,
        Game = 0x0005,
        [Description("Bright Cinema")]
        BrightCinema = 0x0006,
        [Description("Bright TV")]
        BrightTv = 0x0007,
        User = 0x0008,
        [Description("N/A")]
        Unknown = -2
    }

    public enum ColorTemp
    {
        D93 = 0x0000,
        D75 = 0x0001,
        D65 = 0x0002,
        Custom1 = 0x0003,
        Custom2 = 0x0004,
        Custom3 = 0x0005,
        Custom4 = 0x0006,
        Custom5 = 0x0008,
        D55 = 0x0009,
        [Description("N/A")]
        Unknown = -2
    }

    public enum LampControl
    {
        Low = 0x0000,
        High = 0x0001,
        [Description("---")]
        Unknown = -2
    }

    public enum ContrastEnhancer
    {
        Off = 0x0000,
        Low = 0x0001,
        High = 0x0002,
        Middle = 0x0003
    }

    public enum AdvancedIris
    {
        Off = 0x0000,
        Full = 0x0002,
        Limited = 0x0003
    }

    public enum FilmMode
    {
        Off = 0x0000,
        Auto = 0x0002
    }

    public enum GammaCorrection
    {
        Off = 0x0000,
        [Description("1.8")]
        Gamma1_8 = 0x0001,
        [Description("2.0")]
        Gamma2_0 = 0x0002,
        [Description("2.1")]
        Gamma2_1 = 0x0003,
        [Description("2.2")]
        Gamma2_2 = 0x0004,
        [Description("2.4")]
        Gamma2_4 = 0x0005,
        [Description("2.6")]
        Gamma2_6 = 0x0006,
        Gamma7 = 0x0007,
        Gamma8 = 0x0008,
        Gamma9 = 0x0009,
        Gamma10 = 0x000A,
        [Description("---")]
        Unknown,
        [Description("N/A")]
        NotApplicable
    }

    public enum NoiseRedution
    {
        Off = 0x0000,
        Low = 0x0001,
        Middle = 0x0002,
        High = 0x0003,
        Auto = 0x0004
    }

    public enum ColorSpace
    {
        BT709 = 0x0000,
        ColorSpace1 = 0x0003,
        ColorSpace2 = 0x0004,
        ColorSpace3 = 0x0005,
        Custom = 0x0006,
        BT2020 = 0x0008,
        [Description("---")]
        Unknown
    }

    public enum MotionFlow
    {
        Off = 0x0000,
        [Description("Smooth High")]
        SmoothHigh = 0x0001,
        [Description("Smooth Low")]
        SmoothLow = 0x0002,
        Impulse = 0x0003,
        Combination = 0x0004,
        [Description("True Cinema")]
        TrueCinema = 0x0005,
        [Description("---")]
        Unknown = -2,
        [Description("N/A")]
        NotApplicable
    }

#pragma warning disable IDE1006 // Naming Styles
    public enum xvColor
    {
        Off = 0x0000,
        On = 0x0001
    }
#pragma warning restore IDE1006 // Naming Styles

    public enum RealityCreation
    {
        Off = 0x0000,
        On = 0x0001,
        [Description("---")]
        Unknown
    }

    public enum SmoothGradation
    {
        Off = 0x0000,
        Low = 0x0001,
        Middle = 0x0002,
        High = 0x0003
    }

    public enum RealityCreationDatabase
    {
        [Description("Mastered in 4K")]
        MasteredIn4K = 0x00,
        Normal = 0x01,
        [Description("")]
        Unknown
    }

    public enum HDR
    {
        Off = 0x00,
        On = 0x01,
        Auto = 0x02
    }

    public enum ColorCorrection
    {
        Off = 0x0000,
        On = 0x0001
    }

    public enum InputLagReduction
    {
        Off = 0x0000,
        On = 0x0001
    }

    public enum AspectRatio
    {
        Normal = 0x0000,
        [Description("Vertical Stretch")]
        V_Stretch = 0x000B,
        [Description("Zoom 1.85:1")]
        Zoom185_1 = 0x000C,
        [Description("Zoom 2.35:1")]
        Zoom235_1 = 0x000D,
        Stretch = 0x000E,
        Squeeze = 0x000F,
        [Description("---")]
        Unknown = -2
    }

#pragma warning disable CA1724 // Name conflicts with namespace System.Windows.Input
    public enum Input
    {
        HDMI1 = 0x0002,
        HDMI2 = 0x0003
    }
#pragma warning restore CA1724

    public enum SettingsLock
    {
        Off = 0x0000,
        LevelA = 0x0001,
        LevelB = 0x0002
    }

    public enum DisplaySelect3D
    {
        Auto = 0x0000,
        _3D = 0x0001,
        _2D = 0x0002
    }

    public enum Format3D
    {
        Simulated3D = 0x0000,
        SideBySide = 0x0001,
        OverUnder = 0x0002
    }

    public enum Simulated3dEffect
    {
        High = 0x0000,
        Middle = 0x0001,
        Low = 0x0002
    }

    public enum Brightness3D
    {
        High = 0x0000,
        Low = 0x0001
    }

#pragma warning restore CA1707
}
