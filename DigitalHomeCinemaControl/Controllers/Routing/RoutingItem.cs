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

namespace DigitalHomeCinemaControl.Controllers.Routing
{
    using System;

    /// <summary>
    /// Object used by IRoutingSource devices to send data to the routing engine for processing.
    /// </summary>
    public struct RoutingItem : IEquatable<RoutingItem>, IEquatable<MatchAction>
    {

        #region Contructor

        /// <summary>
        /// Creates a new instance of the RoutingItem class.
        /// </summary>
        /// <param name="source">The name of the source device for this item.</param>
        /// <param name="dataType">The Type of data being sent.</param>
        /// <param name="data">The data associated with this item.</param>
        public RoutingItem(string source, Type dataType, object data)
        {
            this.Source = source;
            this.DataType = dataType;
            this.Data = data;
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(RoutingItem other)
        {
            if (this.Source.Equals(other.Source, StringComparison.Ordinal) &&
                this.DataType.Equals(other.DataType) &&
                this.Data.Equals(other.Data)) {

                return true;
            }

            return false;
        }

        public bool Equals(MatchAction other)
        {
            if (other == null) { return false; }

            if (this.Source.Equals(other.MatchSource, StringComparison.Ordinal) &&
                this.Data.Equals(other.Match) && other.Enabled) {

                return true;
            }

            return false;
        }

        public static bool operator ==(RoutingItem left, RoutingItem right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RoutingItem left, RoutingItem right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            unchecked {
                // Choose large primes to avoid hashing collisions
                const int hashingBase = (int)2166136261;
                const int hashingMultiplier = 16777619;

                int hash = hashingBase;

#pragma warning disable IDE0041 // Null check can be simplified
                hash = (hash * hashingMultiplier) ^ (!Object.ReferenceEquals(null, this.Source) ? this.Source.GetHashCode() : 0);
                hash = (hash * hashingMultiplier) ^ (!Object.ReferenceEquals(null, this.DataType) ? this.DataType.GetHashCode() : 0);
                hash = (hash * hashingMultiplier) ^ (!Object.ReferenceEquals(null, this.Data) ? this.Data.GetHashCode() : 0);
#pragma warning restore IDE0041

                return hash;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the IRoutingSource that sent this item.
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Gets the Type for the data that was sent.
        /// </summary>
        public Type DataType { get; private set; }

        /// <summary>
        /// Gets the data that was sent by the source.
        /// </summary>
        public object Data { get; private set; }

        #endregion

    }

}
