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

namespace DigitalHomeCinemaControl.Controllers.Base
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using DigitalHomeCinemaControl.Components.Lighting;

    public abstract class LightingController : DeviceController, ILightingController
    {

        public LightingController()
            : base()
        {
            
        }

        public string RouteAction(string action, object args)
        {
            throw new NotImplementedException();
        }

        public void SetScene(string scence, bool state)
        {
            throw new NotImplementedException();
        }

        public int Index { get; set; }

        public Dictionary<string, LightingScene> Scenes { get; private set; }

        public string Name { get; protected set; }

        public IDictionary<string, Type> Actions { get; private set; }

        
    }

}
