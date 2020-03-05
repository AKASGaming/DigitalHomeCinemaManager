﻿/*
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

namespace DigitalHomeCinemaControl.Controllers
{
    using System;
    using DigitalHomeCinemaControl.Controllers.Providers.Scheduler;
    using DigitalHomeCinemaControl.Controllers.Routing;

    public interface IScheduleController : IController, IRoutingSource, IRoutingDestination, IDisposable
    {

        bool SetSchedule(ScheduleItem start);

        void ClearSchedule();

        bool Enabled { get; }

        ScheduleItem Schedule { get; }

    }

}