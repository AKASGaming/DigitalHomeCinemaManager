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
    using DigitalHomeCinemaControl.Collections;
    using DigitalHomeCinemaControl.Controllers.Base;
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

#pragma warning disable CA1062 // Validate arguments of public methods
        protected override void DataSourceListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged) {
                IBindingItem item = this.DataSource[e.NewIndex];
                switch (item.Name) {
                    case SourceController.CURRENTFILE:
                        this.txtNowPlaying.Text = (string)item.Value;
                        break;
                    case SourceController.FILESIZE:
                        this.txtFileSize.Text = (string)item.Value;
                        break;
                    case SourceController.POSITION:
                        this.txtPostion.Text = (string)item.Value;
                        break;
                    case SourceController.DURATION:
                        this.txtDuration.Text = (string)item.Value;
                        break;
                    case SourceController.CURRENTPOSITION:
                        this.fileProgress.Value = (int)item.Value;
                        break;
                    case SourceController.LENGTH:
                        this.fileProgress.Maximum = (int)item.Value;
                        break;
                }
            }
        }
#pragma warning restore CA1062

        #endregion

    }

}
