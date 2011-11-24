using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JigLibX.Vehicles;
using JigLibX.Collision;
using PlatformEngine;
using NFSEngine;

namespace Carmageddon.Physics
{
    class CarObject : PhysicObject
    {

        private Car car;
        private Model wheel;

        public CarObject(Game game,
                        Model model,
                        Model wheels,
            bool FWDrive,               // Does the car have front wheel drive?
            bool RWDrive,               // Does the car have rear wheel drive?
            float maxSteerAngle,        // Max angle that the wheels can turn (wheel lock angle)    
            float steerRate,            // Supposed to be the max rate wheels can turn but doesn't do anything 
            float wheelFSideFriction,   // Lateral tire friction - For an oversteering vehicle... 
            float wheelRSideFriction,   // ...make the front value larger than the rear 
            float wheelFwdFriction,     // Longitutinal tire friction - try to keep these values...
            float wheelRwdFriction,     // ...lower than 2.0f for reliability (larger values may be fine too)
            float handbrakeRSideFriction,//Amount of lateral friction when handbrake is active
            float handbrakeRwdFriction, // Ditto but longitudinal friction (make for real handbrake)
            float startSlideFactor,     // The amount of drift/slide the car has - When the lateral...
            float thresh1SlideFactor,   // ...wheel velocity is greater than slip thresholds the wheel gets...
            float thresh2SlideFactor,   // ...the next slide factor. Factors are how much drift you'll get... 
            float slideThreshold1,      // ...and Thresholds are at what point they will get it. These...
            float slideThreshold2,      // ...change the smallVel variable in Wheel.cs
            float slideSpeed,           // Adjusts speed lost while drifting - make the same as...
            // ...thresh2SlideFactor to lose least speed during drift
            float slipFactor,           // Standard slip factor for lateral and longitudinal friction
            float wheelTravel,          // Total travel range of the suspension
            float wheelRadius,          // Length of the Rays that test wheel collision
            float wheelZOffset,         // Ride height adjustment - Mounting point of wheels up+/down-  
            float wheelRestingFrac,     // Spring rate - Stiffer at 0.1f, softer at 0.9f
            float wheelDampingFrac,     // Shock dampening - More at 0.9f, less at 0.1f
            int wheelNumRays,           // Number of rays testing wheel collision
            float rollResistance,       // Rolling resistance - higher is more resistance
            float topSpeed,             // Uhh, top speed of vehicle (units arbitrary)                      
            float driveTorque,          // Torque of vehicle (units arbitrary)
            float gravity              // Gravity affect on the car
            )
            : base(game, model)
        {
            car = new Car(FWDrive, RWDrive, maxSteerAngle, steerRate,
            wheelFSideFriction, wheelRSideFriction, wheelFwdFriction,
            wheelRwdFriction, handbrakeRSideFriction, handbrakeRwdFriction,
            startSlideFactor, thresh1SlideFactor, thresh2SlideFactor,
            slideThreshold1, slideThreshold2, slideSpeed, slipFactor, wheelTravel,
            wheelRadius, wheelZOffset, wheelRestingFrac, wheelDampingFrac,
            wheelNumRays, rollResistance, topSpeed, driveTorque, gravity, new Vector3(2.3f, 1.6f, 6f) * new Vector3(2.5f));

            this.body = car.Chassis.Body;
            this.collision = car.Chassis.Skin;
            this.wheel = wheels;

            SetCarMass(50.0f);
        }

        private void DrawWheel(Wheel wh, bool rotated)
        {
            float steer = wh.SteerAngle;

            Matrix rot;
            if (rotated) rot = Matrix.CreateRotationY(MathHelper.ToRadians(180.0f));
            else rot = Matrix.Identity;

            Matrix world = rot * Matrix.CreateRotationZ(MathHelper.ToRadians(-wh.AxisAngle)) * // rotate the wheels
                Matrix.CreateRotationY(MathHelper.ToRadians(steer)) *
                Matrix.CreateTranslation(wh.Pos + wh.Displacement * wh.LocalAxisUp) * car.Chassis.Body.Orientation * // oritentation of wheels
                Matrix.CreateTranslation(car.Chassis.Body.Position); // translation

            Engine.Instance.GraphicsUtils.AddSolidShape(ShapeType.Cube, world, Color.Yellow, null);
        }
               


        public override void Draw(GameTime gameTime)
        {
            DrawWheel(car.Wheels[0], true);
            DrawWheel(car.Wheels[1], true);
            DrawWheel(car.Wheels[2], false);
            DrawWheel(car.Wheels[3], false);

            base.Draw(gameTime);

            GameConsole.WriteLine("Speed: " + Vector3.Dot(car.Chassis.Body.Velocity, Vector3.Forward), 2);
        }

        public Car Car
        {
            get { return this.car; }
        }

        private void SetCarMass(float mass)
        {
            body.Mass = mass;
            Vector3 min, max;
            car.Chassis.GetDims(out min, out max);
            Vector3 sides = max - min;

            float Ixx = (1.0f / 12.0f) * mass * (sides.Y * sides.Y + sides.Z * sides.Z);
            float Iyy = (1.0f / 12.0f) * mass * (sides.X * sides.X + sides.Z * sides.Z);
            float Izz = (1.0f / 12.0f) * mass * (sides.X * sides.X + sides.Y * sides.Y);

            Matrix inertia = Matrix.Identity;
            inertia.M11 = Ixx; inertia.M22 = Iyy; inertia.M33 = Izz;
            car.Chassis.Body.BodyInertia = inertia;
            car.SetupDefaultWheels();
        }

        public override void ApplyEffects(BasicEffect effect)
        {
            
            //
        }
    }
}
