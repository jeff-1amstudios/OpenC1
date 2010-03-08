using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Carmageddon
{
    class Opponent
    {
        public VehicleModel Vehicle;

        public Opponent(string carFile, Vector3 position, float direction)
        {
            Vehicle = new VehicleModel(GameVariables.BasePath + @"data\cars\" + carFile, false);
            Vehicle.SetupChassis(position, direction);
            //Vehicle.Chassis.Body.Sleep();
        }
    }
}
