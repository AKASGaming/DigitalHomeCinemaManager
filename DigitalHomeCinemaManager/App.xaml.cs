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

namespace DigitalHomeCinemaManager
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using DigitalHomeCinemaManager.Components;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IDisposable
    {

        #region Members

        private DigitalCinemaManager cinemaManager;
        private bool disposed = false;

        #endregion

        #region Methods

        private void Main(object sender, StartupEventArgs e) 
        {
            this.cinemaManager = new DigitalCinemaManager();
            this.cinemaManager.Closing += CinemaManagerClosing;
            this.cinemaManager.Run();
        }

        private void CinemaManagerClosing(object sender, EventArgs e)
        {
            Thread.Sleep(500);
            Dispose();
            Environment.Exit(0);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed) {
                if (disposing) {
                    this.cinemaManager?.Dispose();
                }

                this.disposed = true;
                this.cinemaManager = null;
            }
        }

        ~App()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

}

