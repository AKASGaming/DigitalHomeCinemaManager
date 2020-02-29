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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;

    internal static class Extensions
    {

        internal static ObservableCollection<KeyValuePair<string, string>> ToObservableCollection(this StringCollection collection)
        {
            var result = new ObservableCollection<KeyValuePair<string, string>>();
            if (collection.Count == 0) { return result; }

            foreach (string s in collection) {
                if (!s.Contains(":")) { continue; }

                string[] kv = new string[2];
                kv[0] = s.Substring(0, s.IndexOf(":"));
                kv[1] = s.Substring(s.IndexOf(":") + 1);
                
                var kvp = new KeyValuePair<string, string>(kv[0], kv[1]);
                result.Add(kvp);
            }

            return result;
        }

        internal static StringCollection ToStringCollection(this ObservableCollection<KeyValuePair<string, string>> collection)
        {
            var result = new StringCollection();
            if (collection.Count == 0) { return result; }

            foreach (var kvp in collection) {
                result.Add(kvp.Key + ":" + kvp.Value);
            }

            return result;
        }

        internal static NameValueCollection ToNameValueCollection(this StringCollection collection)
        {
            var result = new NameValueCollection(collection.Count);
            if (collection.Count == 0) { return result; }

            foreach (string s in collection) {
                if (!s.Contains(":")) { continue; }

                string[] kv = new string[2];
                kv[0] = s.Substring(0, s.IndexOf(":"));
                kv[1] = s.Substring(s.IndexOf(":") + 1);
                result.Add(kv[0], kv[1]);
            }

            return result;
        }

    }

}
