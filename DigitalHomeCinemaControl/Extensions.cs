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
    using System.ComponentModel;
    using System.Globalization;
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
            if (string.IsNullOrEmpty(asciiString)) { return Array.Empty<byte>(); }

            int len = asciiString.Length;
            byte[] data = new byte[len];

            for (int i = len - 1; i >= 0; i--) {
                data[i] = Convert.ToByte((int)asciiString[i]);
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
            StringBuilder result = new StringBuilder(bytes.Length);
            for (int i = 0; i < bytes.Length; i++) {
                if (i > 0) {
                    result.Append(':');
                }
                result.Append(bytes[i].ToString("x", CultureInfo.InvariantCulture));
            }
            return result.ToString();
        }

        /// <summary>
        /// Gets the Description attribute of the specified enum.
        /// </summary>
        /// <param name="genericEnum"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum genericEnum)
        {
            if (genericEnum == null) { return string.Empty; }

            Type genericEnumType = genericEnum.GetType();
            MemberInfo[] memberInfo = genericEnumType.GetMember(genericEnum.ToString());
            if ((memberInfo != null && memberInfo.Length > 0)) {
                object[] attribs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if ((attribs != null && attribs.Length > 0)) {
                    return ((DescriptionAttribute)attribs[0]).Description;
                }
            }
            return genericEnum.ToString();
        }

        /// <summary>
        /// Gets the custom value for the specified enum or it's Description attribute if no custom name exists.
        /// </summary>
        /// <param name="genericEnum"></param>
        /// <param name="customValues"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum genericEnum, NameValueCollection customValues)
        {
            if (genericEnum == null) { return string.Empty; }
            if ((customValues == null) || (customValues.Count == 0)) {
                return GetDescription(genericEnum);
            }

            string value = customValues.Get(genericEnum.ToString());
            if (value != null) {
                return value;
            }
            return GetDescription(genericEnum);
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
