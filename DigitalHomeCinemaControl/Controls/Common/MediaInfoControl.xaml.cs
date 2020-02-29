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

namespace DigitalHomeCinemaControl.Controls.Common
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using DigitalHomeCinemaControl.Collections;

    /// <summary>
    /// Interaction logic for MediaInfoControl.xaml
    /// </summary>
    public partial class MediaInfoControl : DeviceControl
    {

        #region Members

        private const int READ_BUFFER_LENGTH = 100;

        #endregion

        #region Constructor

        public MediaInfoControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        protected override void DataSourceListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged) {
                IBindingItem item = this.DataSource[e.NewIndex];
                switch (item.Name) {
                    case "PosterPath":
                        if (string.IsNullOrEmpty((string)item.Value)) {
                            this.imgPoster.Source = new BitmapImage();
                        } else {
                            BeginSetFeaturePoster((string)item.Value);
                        }
                        break;
                    case "Description":
                        this.txtOverview.Text = (string)item.Value;
                        break;
                }
            }
        }

        private void BeginSetFeaturePoster(string url)
        {
            try {
                var request = WebRequest.Create(new Uri(url, UriKind.Absolute));
                request.Timeout = -1;
                request.BeginGetResponse(new AsyncCallback(EndSetFeaturePoster), request);
            } catch {
                this.imgPoster.Source = new BitmapImage();
            }
        }

        private void EndSetFeaturePoster(IAsyncResult result)
        {
            var response = (result.AsyncState as WebRequest).EndGetResponse(result) as WebResponse;
            
            this.Dispatcher.Invoke((Action)(() => {
                try {
                    var responseStream = response.GetResponseStream();
                    using (var reader = new BinaryReader(responseStream)) {
                        var memoryStream = new MemoryStream();

                        byte[] bytebuffer = new byte[READ_BUFFER_LENGTH];
                        int bytesRead = reader.Read(bytebuffer, 0, READ_BUFFER_LENGTH);

                        while (bytesRead > 0) {
                            memoryStream.Write(bytebuffer, 0, bytesRead);
                            bytesRead = reader.Read(bytebuffer, 0, READ_BUFFER_LENGTH);
                        }

                        var image = new BitmapImage();
                        image.BeginInit();
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        image.StreamSource = memoryStream;
                        image.EndInit();

                        this.imgPoster.Source = image;
                    }
                } catch {
                    this.imgPoster.Source = new BitmapImage();
                } finally {
                    if (response != null) { response.Dispose(); }
                }
            }));

        }
    
        #endregion

    }

}
