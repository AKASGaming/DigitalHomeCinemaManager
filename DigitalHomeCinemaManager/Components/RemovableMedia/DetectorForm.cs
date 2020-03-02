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

namespace DigitalHomeCinemaManager.Components.RemovableMedia
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    /// <summary>
    /// Hidden Form which we use to receive Windows messages about flash drives
    /// https://www.codeproject.com/articles/18062/detecting-usb-drive-removal-in-a-c-program
    /// </summary>
    internal class DetectorForm : Form
    {
        private Label label1;
        private RemovableDriveManager detector = null;

        /// <summary>
        /// Set up the hidden form. 
        /// </summary>
        /// <param name="detector">DriveDetector object which will receive notification about USB drives, see WndProc</param>
        public DetectorForm(RemovableDriveManager detector)
        {
            this.detector = detector;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.ShowInTaskbar = false;
            this.ShowIcon = false;
            this.FormBorderStyle = FormBorderStyle.None;

            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            this.Visible = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Create really small form, invisible anyway.
            this.Size = new System.Drawing.Size(5, 5);
        }

        /// <summary>
        /// This function receives all the windows messages for this window (form).
        /// We call the DriveDetector from here so that is can pick up the messages about
        /// drives arrived and removed.
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            this.detector?.WndProc(ref m);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(314, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = string.Empty;
            // 
            // DetectorForm
            // 
            this.ClientSize = new System.Drawing.Size(360, 80);
            this.Controls.Add(this.label1);
            this.Name = "DetectorForm";
            ResumeLayout(false);
            PerformLayout();

        }

    } 

}
