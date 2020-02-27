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

namespace DigitalHomeCinemaManager.Windows
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using DigitalHomeCinemaControl;
    using DigitalHomeCinemaManager.Components;
    using DigitalHomeCinemaManager.Controls.Settings;

    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        class CategoryItem
        {
            public string Category { get; set; }
            public string Name { get; set; }
            public UIElement UIElement { get; set; }

        }

        private bool hasChanges = false;

        public SettingsWindow(DeviceManager deviceManager)
        {
            InitializeComponent();

            var genSettings = new GeneralSettings();
            genSettings.ItemChanged += SettingsItemChanged;
            
            var general = new CategoryItem() {
                Name = "General",
                UIElement = genSettings,
            };

            var categories = new List<CategoryItem>() {
                general,
            };

            foreach (var t in Enum.GetValues(typeof(DeviceType))) {
                var name = t.ToString();
                CategoryItem category = new CategoryItem() { Category = "Devices", Name = name, };
                switch (name) {
                    case "Display":
                        var dui = new DisplaySettings();
                        dui.ItemChanged += SettingsItemChanged;
                        category.UIElement = dui;
                        break;
                    case "InputSwitch":
                        var iui = new InputSwitchSettings();
                        iui.ItemChanged += SettingsItemChanged;
                        category.UIElement = iui;
                        break;
                    case "MediaInfo":
                        var mui = new MediaInfoSettings();
                        mui.ItemChanged += SettingsItemChanged;
                        category.UIElement = mui;
                        break;
                    case "Processor":
                        var pui = new ProcessorSettings();
                        pui.ItemChanged += SettingsItemChanged;
                        category.UIElement = pui;
                        break;
                    case "Serial":
                        var sui = new SerialSettings();
                        sui.ItemChanged += SettingsItemChanged;
                        category.UIElement = sui;
                        break;
                    case "Source":
                        var srui = new SourceSettings();
                        srui.ItemChanged += SettingsItemChanged;
                        category.UIElement = srui;
                        break;
                }
                categories.Add(category);
            }
            
            ICollectionView view = CollectionViewSource.GetDefaultView(categories);
            view.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            this.Category.ItemsSource = view;

            this.Category.SelectedItem = general;
            this.ButtonSave.IsEnabled = false;
        }

        private void SettingsItemChanged(object sender, EventArgs e)
        {
            this.ButtonSave.IsEnabled = true;
            this.hasChanges = true;
        }

        private void CategorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Category.SelectedValue == null) { return; }

            var category = (CategoryItem)this.Category.SelectedValue;

            if (category.UIElement != null) {
                this.SettingPanel.Child = category.UIElement;
            }
        }

        private void ButtonSaveClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        
    }

}
