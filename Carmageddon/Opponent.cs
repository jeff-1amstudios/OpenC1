using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Carmageddon
{
    class Opponent
    {
        public Vehicle Vehicle;
        public IDriver Driver;

        public Opponent(string carFile, Vector3 position, float direction)
        {
            Driver = new CpuDriver();
            Vehicle = new Vehicle(GameVariables.BasePath + @"data\cars\" + carFile, Driver);
            Vehicle.SetupPhysics(position, direction);
            Vehicle.Chassis.Actor.LinearDamping = 0.02f;
            Vector3 com = Vehicle.Chassis.Actor.CenterOfMassLocalPosition;
            com.Y -= 0.06f;
            Vehicle.Chassis.Actor.SetCenterOfMassOffsetLocalPosition(com); //help out ai to keep the car stable

            //give cpu driver a bit more grip
            Vehicle.Chassis.SetLateralFrictionMultiplier(1.1111f);
        }
    }
}
