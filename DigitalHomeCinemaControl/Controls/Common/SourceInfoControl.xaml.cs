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
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using DigitalHomeCinemaControl.Collections;
    using DigitalHomeCinemaControl.Controls;

    /// <summary>
    /// Interaction logic for SourceInfoControl.xaml
    /// </summary>
    public partial class SourceInfoControl : DeviceControl
    {

        #region Constructor

        public SourceInfoControl()
        {
            InitializeComponent();
            this.fileProgress.Minimum = 0;
            
        }

        #endregion

        #region Methods

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        protected override void DataSourceListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged) {
                IBindingItem item = this.DataSource[e.NewIndex];
                switch (item.Name) {
                    case "CurrentFile":
                        this.txtNowPlaying.Text = (string)item.Value;
                        break;
                    case "FileSize":
                        this.txtFileSize.Text = (string)item.Value;
                        break;
                    case "Position":
                        this.txtPostion.Text = (string)item.Value;
                        break;
                    case "Duration":
                        this.txtDuration.Text = (string)item.Value;
                        break;
                    case "CurrentPosition":
                        this.fileProgress.Value = (int)item.Value;
                        break;
                    case "Length":
                        this.fileProgress.Maximum = (int)item.Value;
                        break;
                }
            }
        }

        #endregion

    }

}
