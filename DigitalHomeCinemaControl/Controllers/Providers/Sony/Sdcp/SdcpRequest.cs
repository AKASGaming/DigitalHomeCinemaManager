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
    using System;

    public class SdcpRequest
    {

        #region Members

        private const byte version = 0x02;
        private const byte category = 0x0A;

        private byte[] community = new byte[4];
        private byte[] command = new byte[4];
        private byte[] data;

        #endregion

        #region Contructor

        public SdcpRequest()
            : this("SONY")
        {
        }

        public SdcpRequest(string community)
        {
            if (community == null) { throw new ArgumentException("value cannot be NULL", "community"); }
            if (community.Length != 4) { throw new ArgumentException("value length must be equal to 4 characters", "community"); }

            this.community = community.ToByteArray();
        }

        public SdcpRequest(string community, RequestType request)
            : this(community)
        {
            this.Request = request;
        }

        public SdcpRequest(string community, RequestType request, byte[] itemNumber)
            : this(community, request)
        {
            this.ItemNumber = itemNumber;
        }

        public SdcpRequest(string community, RequestType request, byte[] itemNumber, byte[] data)
            : this(community, request, itemNumber)
        {
            this.Data = data;
        }

        public SdcpRequest(RequestType request, byte[] itemNumber)
            : this()
        {
            this.Request = request;
            this.ItemNumber = itemNumber;
        }

        public SdcpRequest(RequestType request, byte[] itemNumber, byte[] data)
            : this()
        {
            this.Request = request;
            this.ItemNumber = itemNumber;
            this.Data = data;
        }

        public SdcpRequest(string community, RequestType request, CommandItem item)
            : this(community)
        {
            this.Request = request;
            this.Item = item;
        }

        public SdcpRequest(string community, RequestType request, CommandItem item, object data)
            : this(community)
        {
            this.Request = request;
            this.Item = item;
            SetData(data);
        }

        public SdcpRequest(RequestType request, CommandItem item, object data)
            : this()
        {
            this.Request = request;
            this.Item = item;
            SetData(data);

        }

        #endregion

        #region Methods

        public void SetData(object data)
        {
            if (data.GetType().IsEnum == false) { throw new ArgumentException(""); }

            int i = (int)data;
            byte[] value = new byte[2] { (byte)(i >> 8), (byte)(i & 0xff) };
            this.Data = value;
        }

        public void SetData(int data)
        {
            byte[] value = new byte[2] { (byte)(data >> 8), (byte)(data & 0xff) };
            this.Data = value;
        }

        #endregion

        #region Properties

        public byte[] Community
        {
            get { return this.community; }
            set {
                if (value != null) {
                    this.community = value;
                }
            }
        }

        public byte[] Command
        {
            get { return this.command; }
            set {
                if (value != null) {
                    this.command = value;
                }
            }
        }

        public RequestType Request
        {
            get { return (RequestType)this.command[0]; }
            set { this.command[0] = (byte)value; }
        }

        public CommandItem Item
        {
            get {
                int i = (256 * this.command[1]) + this.command[2];
                return (CommandItem)i;
            }
            set {
                this.command[1] = (byte)((int)(value) >> 8);
                this.command[2] = (byte)((int)(value) & 0xff);
            }
        }

        public byte[] ItemNumber
        {
            get {
                byte[] result = new byte[2] { this.command[1], this.command[2] };
                return result;
            }
            set {
                if (value == null || value.Length != 2) { return; }
                this.command[1] = value[0];
                this.command[2] = value[1];
            }
        }

        public byte[] Data
        {
            get { return this.data; }
            set {
                if (value == null) { return; }
                this.data = value;
                this.command[3] = Convert.ToByte(value.Length.ToString(), 16);
            }
        }

        public byte[] ToByteArray()
        {
            int length = Convert.ToInt32(this.command[3]) + 10;
            byte[] result = new byte[length];
            result[0] = version;
            result[1] = category;
            result[2] = this.community[0];
            result[3] = this.community[1];
            result[4] = this.community[2];
            result[5] = this.community[3];
            result[6] = this.command[0];
            result[7] = this.command[1];
            result[8] = this.command[2];
            result[9] = this.command[3];

            if (this.data != null && this.data.Length > 0) {
                for (int i = 0; i < this.data.Length; i++) {
                    result[10 + i] = this.data[i];
                }
            }

            return result;
        }

        #endregion

    }

}
