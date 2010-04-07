using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Carmageddon.Physics;
using StillDesign.PhysX;

namespace Carmageddon
{
    class Opponent
    {
        public Vehicle Vehicle;
        public CpuDriver Driver;
        BoundingSphere _boundingSphere;

        public Opponent(string carFile, Vector3 position, float direction)
        {
            Driver = new CpuDriver();
            Vehicle = new Vehicle(GameVariables.BasePath + @"data\cars\" + carFile, Driver);
            Vehicle.PlaceOnGrid(position, direction);

            SetupVehicle();
        }

        public void SetupVehicle()
        {
            Vehicle.Chassis.Actor.LinearDamping = 0.02f;
            Vector3 com = Vehicle.Chassis.Actor.CenterOfMassLocalPosition;
            //com.Y -= 0.3f;
            //Vehicle.Chassis.Actor.SetCenterOfMassOffsetLocalPosition(com); //help out ai to keep the car stable


            TireFunctionDescription frontLateralTireFn = new TireFunctionDescription();
            frontLateralTireFn.ExtremumSlip = 0.26f;
            frontLateralTireFn.ExtremumValue = 1.8f;
            frontLateralTireFn.AsymptoteSlip = 2;
            frontLateralTireFn.AsymptoteValue = 0.001f;

            TireFunctionDescription rearLateralTireFn = new TireFunctionDescription();
            rearLateralTireFn.ExtremumSlip = 0.35f;
            rearLateralTireFn.ExtremumValue = 2.7f;
            rearLateralTireFn.AsymptoteSlip = 2f;
            rearLateralTireFn.AsymptoteValue = 0.001f;

            foreach (VehicleWheel wheel in Vehicle.Chassis.Wheels)
            {
                wheel.Shape.LateralTireForceFunction = wheel.IsRear ? rearLateralTireFn : frontLateralTireFn;
            }


            //give cpu driver a bit more grip
            Vehicle.Chassis.SetLateralFrictionMultiplier(1.111f);
        }

        public BoundingSphere GetBoundingSphere()
        {
            if (_boundingSphere == null)
            {
                _boundingSphere = new BoundingSphere(Vector3.Zero, Vehicle.Config.BoundingBox.GetSize().Length());
            }
            _boundingSphere.Center = Vehicle.Position;
            return _boundingSphere;
        }
    }
}
