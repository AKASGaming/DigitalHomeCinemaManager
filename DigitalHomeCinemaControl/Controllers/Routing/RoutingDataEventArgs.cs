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

    public class RoutingDataEventArgs : EventArgs
    {

        public RoutingDataEventArgs(RoutingItem item)
            : base()
        {
            this.Item = item;
        }

        public RoutingItem Item { get; private set; }

    }

}
