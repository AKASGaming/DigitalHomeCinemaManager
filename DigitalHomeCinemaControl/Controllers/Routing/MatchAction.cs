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
    /// The MatchAction class specifies a rule for matching by the routing engine
    /// and directing an action to the IRoutingDestination
    /// </summary>
    [Serializable]
    public sealed class MatchAction
    {

        #region Members

        private object args;
        private object match;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets the name of the source that triggers this action.
        /// </summary>
        public string MatchSource { get; set; }

        /// <summary>
        /// Gets or Sets a value that is used by the routing engine to validate
        /// a match occurred.
        /// </summary>
        public object Match
        {
            get { return this.match; }
            set {
                this.match = value;
                if (value != null) {
                    this.MatchType = value.GetType().AssemblyQualifiedName;
                } else {
                    this.MatchType = null;
                }
            }
        }

        /// <summary>
        /// Gets or Set the Type of the Match property.
        /// Used for Xml serializaion.
        /// </summary>
        public string MatchType { get; set; }
        
        /// <summary>
        /// Gets or Sets the Name of the destination to route this action to.
        /// </summary>
        public string ActionDestination { get; set; }

        /// <summary>
        /// Gets or Sets the object that describes the action for the destination.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets or Sets the object to pass to the destination for the specified action.
        /// </summary>
        public object Args
        {
            get { return this.args; }
            set {
                this.args = value;
                if (value != null) {
                    this.ArgsType = value.GetType().AssemblyQualifiedName;
                } else {
                    this.ArgsType = null;
                }
            }

        }

        /// <summary>
        /// Gets or Sets the Type of the Args property.
        /// Used for Xml serialization.
        /// </summary>
        public string ArgsType { get; set; }

        /// <summary>
        /// Gets or Sets a value that determines if this MatchAction is ignored by the routing engine.
        /// </summary>
        public bool Enabled { get; set; }

        #endregion

    }

}
