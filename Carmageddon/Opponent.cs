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
            TireFunctionDescription frontLateralTireFn = new TireFunctionDescription();
            frontLateralTireFn.ExtremumSlip = 0.26f;
            frontLateralTireFn.ExtremumValue = 2.3f;
            frontLateralTireFn.AsymptoteSlip = 2.222f;
            frontLateralTireFn.AsymptoteValue = 0.001f;

            TireFunctionDescription rearLateralTireFn = new TireFunctionDescription();
            rearLateralTireFn.ExtremumSlip = 0.35f;
            rearLateralTireFn.ExtremumValue = 2.3f;
            rearLateralTireFn.AsymptoteSlip = 2.4f;
            rearLateralTireFn.AsymptoteValue = 0.001f;

            foreach (VehicleWheel wheel in Vehicle.Chassis.Wheels)
            {
                wheel.Shape.LateralTireForceFunction = wheel.IsRear ? rearLateralTireFn : frontLateralTireFn;
            }

            Vector3 massPos = Vehicle.Config.CenterOfMass;
            massPos.Y = Vehicle.Config.WheelActors[0].Position.Y - Vehicle.Config.NonDrivenWheelRadius + 0.31f;
            Vehicle.Chassis.Actor.SetCenterOfMassOffsetLocalPosition(massPos);
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
