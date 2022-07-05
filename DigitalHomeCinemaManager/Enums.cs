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

using System.ComponentModel;

namespace DigitalHomeCinemaManager
{

    public enum PlaylistType
    {
        Preroll,
        Trailer,
        Commercial,
        Feature
    }

    public enum VideoFormat
    {
        Unknown,
        [Description("720")]
        HD,
        [Description("1080")]
        FHD,
        [Description("1440")]
        QHD,
        [Description("2160")]
        UHD,
        [Description("4320")]
        EightK,
        DVD
    }

    public enum AudioFormat
    {
        Unknown,
        DolbyAtmos,
        DTS,
        DolbyDigital,
        TrueHD,
        TrueHDAtmos,
        DTSX,
        DTSHDMA,
        DTSHD,
        DolbyDigitalPlus,
        DolbyDigitalPlusAtmos,
    }

    public enum HDR
    {
        Unknown,
        HDR10,
        HDR10Plus,
        DolbyVision,
        HLG,
        SLHDR1,
        SLHDR2,
        SLHDR3
    }
}
