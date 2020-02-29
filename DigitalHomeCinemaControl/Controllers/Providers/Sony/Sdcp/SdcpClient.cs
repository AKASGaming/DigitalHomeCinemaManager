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
    using System.Net.Sockets;
    using System.Threading;

    public class SdcpClient : IDisposable
    {

        #region Members

        private const int READ_BUFFER_SIZE = 1024;

        private readonly object lockObject = new object();
        private TcpClient client;
        private NetworkStream stream;
        private AutoResetEvent waitHandle;
        private bool disposed = false;
        private string host;
        private int port = 53484;
        private int commandDelay = 1500;

        #endregion

        #region Constructor

        public SdcpClient()
        {
            this.client = new TcpClient() {
                NoDelay = true,
                ReceiveBufferSize = READ_BUFFER_SIZE,
            };
            this.waitHandle = new AutoResetEvent(false);
            this.Closed = false;
        }

        public SdcpClient(string host)
            : this()
        {
            this.host = host;
        }

        public SdcpClient(string host, int port)
            : this()
        {
            this.host = host;
            this.port = port;
        }

        #endregion

        #region Methods

        public void Connect()
        {
            if (string.IsNullOrEmpty(this.host)) { throw new InvalidOperationException("Invalid Host"); }
            if (this.Closed) { throw new ObjectDisposedException("SdcpClient", "Client has been Disposed."); }

            this.waitHandle.Reset();
            this.client.Connect(this.host, this.port);
            try {
                this.stream = this.client.GetStream();
            } catch {
                this.Closed = true;
            }
        }

        public void Connect(string host)
        {
            this.host = host;
            Connect();
        }

        public void Connect(string host, int port)
        {
            this.host = host;
            this.port = port;
            Connect();
        }

        public SdcpResponse Send(SdcpRequest request)
        {
            if (request == null) { throw new ArgumentException("object cannot be null", "request"); }
            if (!this.client.Connected) { throw new InvalidOperationException("Not Connected"); }
            if (this.Closed) { throw new InvalidOperationException("Connection closed"); }

            if ((this.stream == null) || !this.stream.CanWrite || !this.stream.CanRead) {
                this.Closed = true;
                throw new InvalidOperationException("Stream not readable or writable"); 
            }

            byte[] buffer = request.ToByteArray();
            byte[] readBuffer = new byte[READ_BUFFER_SIZE];

            // only one command is allowed to be sent at a time,
            // prevent any other threads from making simultaneous calls to send
            lock (this.lockObject) {
                this.stream.Write(buffer, 0, buffer.Length);

                int bytesRead = 0;

                // We need to wait until the projector has processed the command
                if (!this.waitHandle.WaitOne(this.commandDelay)) {
                    // wait timed out without beig set, so read the data from the port
                    do {
                        // note: technically this can overflow, but projector should never send more than ~150 bytes
                        bytesRead = this.stream.Read(readBuffer, bytesRead, readBuffer.Length - bytesRead);
                    } while (this.stream.DataAvailable);
                } else {
                    // wait handle was signaled, we're closing
                    return new SdcpResponse(SdcpResult.ERROR, SdcpError.OtherCommError);
                }
            }

            SdcpResponse response;
            SdcpResult result = (SdcpResult)readBuffer[6];

            if (result == SdcpResult.ERROR) {
                int len = readBuffer[9];
                if (len > 0) {
                    int error = (256 * readBuffer[10]) + readBuffer[11];
                    response = new SdcpResponse(result, (SdcpError)error);
                } else {
                    response = new SdcpResponse(result, SdcpError.UnknownResponse);
                }
            } else {
                response = new SdcpResponse(result);

                if (request.Request == RequestType.Get) {
                    int len = readBuffer[9];
                    byte[] data = new byte[len];

                    for (int i = 0; i < len; i++) {
                        data[i] = readBuffer[10 + i];
                    }

                    response.Data = data;
                }
            }

            return response;
        }

        public void Close()
        {
            lock (this.lockObject) {
                if (this.waitHandle != null) {
                    this.waitHandle.Set();
                }
            }

            try {
                if (this.client != null) {
                    this.client.Close();
                }
                if (this.stream != null) {
                    this.stream.Close();
                }
            } catch { 
            } finally {
                this.Closed = true;
                Dispose(true);
            }

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed) {
                if (disposing) {
                    try {
                        if (this.waitHandle != null) {
                            this.waitHandle.Dispose();
                        }
                        if (this.client != null) {
                            this.client.Close();
                        }
                        if (this.stream != null) {
                            this.stream.Dispose();
                        }
                    } finally {
                        this.waitHandle = null;
                        this.client = null;
                        this.stream = null;
                    }
                }

                this.Closed = true;
                this.disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Properties

        public string Host
        {
            get { return this.host; }
            set {
                if (string.IsNullOrEmpty(value)) { return; }
                this.host = value;
            }
        }

        public int Port
        {
            get { return this.port; }
            set { this.port = value; }
        }

        public int CommandDelay
        {
            get { return this.commandDelay; }
            set {
                if (value < 30) { throw new ArgumentOutOfRangeException("CommandDelay", "Value may not be less than 30"); }
                if (value > 3200) { throw new ArgumentOutOfRangeException("CommandDelay", "Value may not be larger than 3200"); }

                this.commandDelay = value;
            }
        }

        public bool Closed { get; private set; }

        public bool Connected
        {
            get {
                if (this.client == null) { return false; }
                return this.client.Connected;
            }
        }

        #endregion

    }

}
