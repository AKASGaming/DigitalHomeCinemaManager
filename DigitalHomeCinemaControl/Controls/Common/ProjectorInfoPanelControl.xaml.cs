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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for SmallInfoPanelControl.xaml
    /// </summary>
    public partial class ProjectorInfoPanelControl : DeviceControl
    {
        public ProjectorInfoPanelControl()
        {
            InitializeComponent();

            
        }

        protected override void DataSourceListChanged(object sender, ListChangedEventArgs e)
        {
            if (this.SmallList.ItemsSource == null) {
                this.SmallList.ItemsSource = this.DataSource;
            }
        }
    }
}
