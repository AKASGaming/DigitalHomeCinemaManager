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

    public sealed class SdcpResponse
    {

        #region Members

        private SdcpResult result;
        private SdcpError error;
        private byte[] data;

        #endregion

        #region Constructor

        public SdcpResponse(SdcpResult result)
        {
            this.result = result;
        }

        public SdcpResponse(SdcpResult result, SdcpError error)
            : this(result)
        {
            this.error = error;
        }

        #endregion

        #region Properties

        public SdcpResult Result
        {
            get { return this.result; }
        }

        public SdcpError Error
        {
            get { return this.error; }
        }

#pragma warning disable CA1819 // Properties should not return arrays

        public byte[] Data
        {
            get { return this.data; }
            set { this.data = value; }
        }

#pragma warning restore CA1819

        public int DataValue
        {
            get {
                if (this.data == null) { return -1; }
                if (this.data.Length == 1) { return this.data[0]; }
                if (this.data.Length == 2) { return (256 * this.data[0]) + this.data[1]; }
                return -2;
            }
        }

        #endregion

    }

}
