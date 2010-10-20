using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using OpenC1.Physics;
using StillDesign.PhysX;

namespace OpenC1
{
    class Opponent
    {
        public Vehicle Vehicle;
        public CpuDriver Driver;
        BoundingSphere _boundingSphere;
        public bool IsDead { get { return Driver.IsDead; } }

        public Opponent(string carFile, Vector3 position, float direction)
            : this(carFile, position, direction, null)
        {
        }

        public Opponent(string carFile, Vector3 position, float direction, CpuDriver driver)
        {
            if (driver == null) driver = new CpuDriver();
            Driver = driver;
            Vehicle = new Vehicle(GameVars.BasePath + @"data\cars\" + carFile, Driver);
            if (driver is CopDriver)
            {
                Vehicle.Chassis.Actor.GlobalPosition = position;
                Vehicle.Chassis.Actor.GlobalOrientation *= Matrix.CreateRotationY(MathHelper.ToRadians(90));
            }
            else
            {
                Vehicle.PlaceOnGrid(position, direction);
            }
            SetupVehicle();
        }

        public void SetupVehicle()
        {
            TireFunctionDescription frontLateralTireFn = new TireFunctionDescription();
            frontLateralTireFn.ExtremumSlip = 0.26f;
            frontLateralTireFn.ExtremumValue = 2.3f;
            frontLateralTireFn.AsymptoteSlip = 2.21f;
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

            Vector3 massPos = Vehicle.Chassis.Actor.CenterOfMassLocalPosition;
            massPos.Y -= 0.3f;
            //Vehicle.Chassis.Actor.SetCenterOfMassOffsetLocalPosition(massPos);
        }

        public BoundingSphere GetBoundingSphere()
        {
            if (_boundingSphere.Radius == 0)
            {
                Bounds3 bounds = Vehicle.Chassis.Actor.Shapes[0].WorldSpaceBounds;
                _boundingSphere = new BoundingSphere(Vector3.Zero, bounds.Size.Length());
            }
            _boundingSphere.Center = Vehicle.Position;
            return _boundingSphere;
        }

        public void Kill()
        {
            Driver.IsDead = true;
        }
    }
}
