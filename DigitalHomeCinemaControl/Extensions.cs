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

namespace DigitalHomeCinemaControl
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using DigitalHomeCinemaControl.Controllers.Routing;

    /// <summary>
    /// Provides various helper methods in the form of extensions.
    /// </summary>
    public static  class Extensions
    {

        /// <summary>
        /// Converts an ASCII string to a byte array.
        /// </summary>
        /// <param name="asciiString">The string to convert.</param>
        /// <returns>A byte array of characters in the string.</returns>
        public static byte[] ToByteArray(this string asciiString)
        {
            if (string.IsNullOrEmpty(asciiString)) { return new byte[0]; }

            byte[] data = new byte[asciiString.Length];
            for (int i = 0; i < asciiString.Length; i++) {
                if (char.TryParse(asciiString.Substring(i, 1), out char c)) {
                    data[i] = Convert.ToByte((int)c);
                }
            }

            return data;
        }

        /// <summary>
        /// Converts an ASCII string to a hex representation.
        /// </summary>
        /// <param name="asciiString">The string to convert.</param>
        /// <returns>A colon ':' delimited string of hex values.</returns>
        public static string AsciiToHexString(this string asciiString)
        {
            if (string.IsNullOrEmpty(asciiString)) { return string.Empty; }

            byte[] bytes = asciiString.ToByteArray();
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++) {
                if (i > 0) {
                    result.Append(":");
                }
                result.Append(bytes[i].ToString("x"));
            }
            return result.ToString();
        }

        /// <summary>
        /// Gets the Description attribute of the specified enum.
        /// </summary>
        /// <param name="GenericEnum"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum GenericEnum)
        {
            Type genericEnumType = GenericEnum.GetType();
            MemberInfo[] memberInfo = genericEnumType.GetMember(GenericEnum.ToString());
            if ((memberInfo != null && memberInfo.Length > 0)) {
                var _Attribs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if ((_Attribs != null && _Attribs.Count() > 0)) {
                    return ((System.ComponentModel.DescriptionAttribute)_Attribs.ElementAt(0)).Description;
                }
            }
            return GenericEnum.ToString();
        }

        /// <summary>
        /// Gets the custom value for the specified enum or it's Description attribute if no custom name exists.
        /// </summary>
        /// <param name="GenericEnum"></param>
        /// <param name="customValues"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum GenericEnum, NameValueCollection customValues)
        {
            if ((customValues == null) || (customValues.Count == 0)) {
                return GetDescription(GenericEnum);
            }

            string value = customValues.Get(GenericEnum.ToString());
            if (value != null) {
                return value;
            }
            return GetDescription(GenericEnum);
        }

        /// <summary>
        /// Gets the match Type for the specified action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Type GetMatchType(this MatchAction action)
        {
            if (action == null) { return null; }
            if (string.IsNullOrEmpty(action.MatchType)) { return null; }

            return Type.GetType(action.MatchType);
        }

        /// <summary>
        /// Gets the args Type for the specified action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Type GetArgsType(this MatchAction action)
        {
            if (action == null) { return null; }
            if (string.IsNullOrEmpty(action.ArgsType)) { return null; }

            return Type.GetType(action.ArgsType);
        }

    }

}
