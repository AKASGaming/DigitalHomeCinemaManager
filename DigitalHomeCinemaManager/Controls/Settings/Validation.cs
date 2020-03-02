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

namespace DigitalHomeCinemaManager.Controls.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text.RegularExpressions;

    internal static class Validation
    {

        public static bool ContainsKey(this ObservableCollection<KeyValuePair<string, string>> collection, string key)
        {
            if ((collection == null) || (collection.Count == 0)) { return false; }
            if (string.IsNullOrEmpty(key)) { return false; }

            foreach (var kvp in collection) {
                if (kvp.Key.Equals(key, StringComparison.Ordinal)) { return true; }
            }

            return false;
        }

        public static bool IsUrl(this string text)
        {
            if (string.IsNullOrEmpty(text)) { return false; }

            bool result = Uri.TryCreate(text, UriKind.Absolute, out Uri uri) &&
                ((uri.Scheme == Uri.UriSchemeHttp) || (uri.Scheme == Uri.UriSchemeHttps));

            return result;
        }

        public static bool IsIpAddress(this string text)
        {
            if (string.IsNullOrEmpty(text)) { return false; }

            bool result = Regex.IsMatch(text, "^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");

            return result;
        }

        public static bool IsInteger(this System.Windows.Input.Key key)
        {
            bool result = ((key >= System.Windows.Input.Key.D0 && key <= System.Windows.Input.Key.D9) ||
                (key >= System.Windows.Input.Key.NumPad0 && key <= System.Windows.Input.Key.NumPad9) ||
                key == System.Windows.Input.Key.Back || key == System.Windows.Input.Key.Delete);

            return result;
        }

        public static bool IsNumeric(this System.Windows.Input.Key key)
        {
            bool result = ((key >= System.Windows.Input.Key.D0 && key <= System.Windows.Input.Key.D9) ||
                (key >= System.Windows.Input.Key.NumPad0 && key <= System.Windows.Input.Key.NumPad9) ||
                key == System.Windows.Input.Key.Back || key == System.Windows.Input.Key.Delete ||
                key == System.Windows.Input.Key.Decimal || key == System.Windows.Input.Key.OemPeriod);

            return result;
        }

    }

}

