using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework;
using StillDesign.PhysX;
using Carmageddon.Physics;
using NFSEngine;

namespace Carmageddon
{
    class NonCar
    {
        public CActor CActor;
        public NoncarFile Config;
        public bool IsAttached;
        public float LastTouchTime;
        public bool Hit;
        public Vector3 Rotation;
        private Matrix _origOrientation;
        bool _initialized;

        public void OnHit()
        {
            if (!IsAttached) return;  //let physx handle it

            Matrix orientation = Matrix.CreateRotationX(Rotation.Z) * Matrix.CreateRotationZ(Rotation.X);
            float angle = MathHelper.ToDegrees(Helpers.UnsignedAngleBetweenTwoV3(Vector3.Up, orientation.Up));
            
            if (angle >= Config.BendAngleBeforeSnapping)
            {
                // reduce the impact until the object is within normal bounds, then make dynamic
                while (true)
                {
                    Rotation *= 0.8f;
                    orientation = Matrix.CreateRotationX(Rotation.Z) * Matrix.CreateRotationZ(Rotation.X);
                    angle = MathHelper.ToDegrees(Helpers.UnsignedAngleBetweenTwoV3(Vector3.Up, orientation.Up));
                    if (angle <= Config.BendAngleBeforeSnapping)
                    {
                        break;
                    }
                }
                CActor.PhysXActor.GlobalOrientation = _origOrientation * orientation;
                IsAttached = false;
                CActor.PhysXActor.ClearBodyFlag(BodyFlag.Kinematic);
            }
            else
            {
                CActor.PhysXActor.GlobalOrientation = _origOrientation * orientation;
            }

            Hit = false;
        }

        public void AttachToGround()
        {
            // if this is the first time we weld to ground, initialize
            if (!_initialized)
            {
                _origOrientation = CActor.PhysXActor.GlobalOrientation;
                CActor.PhysXActor.RaiseBodyFlag(BodyFlag.Kinematic);
                _initialized = true;
            }
            IsAttached = true;
            
        }
    }
}
