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

/*
* 
* Original source from:  https://www.codeproject.com/articles/18062/detecting-usb-drive-removal-in-a-c-program
* Refactored and converted to WPF on 3/2020 by Bill Mandra
* 
*/

namespace DigitalHomeCinemaManager.Components.RemovableMedia
{
    using System;
    using System.Windows;

    /// <summary>
    /// Hidden Window which we use to receive Windows messages about flash drives
    /// </summary>
    internal class DetectorWindow : Window
    {

        #region Constructor

        public DetectorWindow()
        {
            this.WindowStyle = WindowStyle.None;
            this.ShowInTaskbar = false;
            this.Height = 5;
            this.Width = 5;
        }

        #endregion

        #region Methods

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            this.Visibility = Visibility.Hidden;
        }

        #endregion

    }

}
