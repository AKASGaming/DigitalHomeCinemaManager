﻿/*
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

namespace DigitalHomeCinemaControl.Controls.HDFury
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using DigitalHomeCinemaControl.Controllers;
    using DigitalHomeCinemaControl.Controllers.Providers.HDFury;

    /// <summary>
    /// Interaction logic for InputControl.xaml
    /// </summary>
    public partial class InputControl : DeviceControl, IRequireController
    {

        #region Members

        private static BitmapSource EMPTY_IMAGE = BitmapImage.Create(2, 2, 96, 96, PixelFormats.Indexed1, new BitmapPalette(new List<Color> { Colors.Transparent }), new byte[] { 0, 0, 0, 0 }, 1);

        #endregion

        #region Constructor

        public InputControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

#pragma warning disable CA1062 // Validate arguments of public methods
        protected override void DataSourceListChanged(object sender, ListChangedEventArgs e)
        {
            var changedItem = this.DataSource[e.NewIndex];
            switch (changedItem.Name) {
                case DivaController.TX0SINK: this.TX0.Text = changedItem.Value.ToString(); break;
                case DivaController.TX0OUT: this.OutputTx0.Text = changedItem.Value.ToString(); break;
                case DivaController.INPUT:
                    var input = (Rx)changedItem.Value;
                    this.Source.Text = input.GetDescription() + ":";
                    SetCurrentInput(input);
                    break;
            }
        }
#pragma warning restore CA1062

        private void SetCurrentInput(Rx input)
        {
            switch (input) {
                case Rx.Input1:
                    SetImageSource(this.Input0, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_1_white_24dp.png");
                    SetImageSource(this.Input1, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_2_black_24dp.png");
                    SetImageSource(this.Input2, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_3_black_24dp.png");
                    SetImageSource(this.Input3, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_4_black_24dp.png");
                    break;
                case Rx.Input2:
                    SetImageSource(this.Input0, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_1_black_24dp.png");
                    SetImageSource(this.Input1, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_2_white_24dp.png");
                    SetImageSource(this.Input2, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_3_black_24dp.png");
                    SetImageSource(this.Input3, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_4_black_24dp.png");
                    break;
                case Rx.Input3:
                    SetImageSource(this.Input0, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_1_black_24dp.png");
                    SetImageSource(this.Input1, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_2_black_24dp.png");
                    SetImageSource(this.Input2, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_3_white_24dp.png");
                    SetImageSource(this.Input3, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_4_black_24dp.png");
                    break;
                case Rx.Input4:
                    SetImageSource(this.Input0, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_1_black_24dp.png");
                    SetImageSource(this.Input1, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_2_black_24dp.png");
                    SetImageSource(this.Input2, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_3_black_24dp.png");
                    SetImageSource(this.Input3, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_4_white_24dp.png");
                    break;
                default:
                    SetImageSource(this.Input0, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_1_black_24dp.png");
                    SetImageSource(this.Input1, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_2_black_24dp.png");
                    SetImageSource(this.Input2, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_3_black_24dp.png");
                    SetImageSource(this.Input3, "pack://application:,,/DigitalHomeCinemaControl;component/Resources/Icons/outline_filter_4_black_24dp.png");
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetImageSource(Image image, string source)
        {
            ImageSource imageSource;

            if (!string.IsNullOrEmpty(source)) {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(source);
                bi.EndInit();
                imageSource = bi;
            } else {
                imageSource = EMPTY_IMAGE;
            }

            image.Source = imageSource;
        }

        private void ButtonSourceSelectClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var newInput = (Rx)(int)((Button)sender).Tag;
            var hdFury = (DivaController)this.Controller;

            if (!hdFury.IsConnected) { return; }

            if (!hdFury.SetInput(newInput)) {
                MessageBox.Show(Properties.Resources.MSG_HDFURY_FAIL_INPUT_SELECT, Properties.Resources.MSG_ERROR, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        #endregion

        #region Properties

        public IController Controller { get; set; }

        #endregion

    }

}
