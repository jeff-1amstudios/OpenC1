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
        public Matrix NewOrientation;
        public Vector3 Rotation;
        private Matrix _origOrientation;
        bool _initialized;

        public void OnHit()
        {
            if (!IsAttached) return;  //let physx handle it :)

            
            CActor.PhysXActor.GlobalOrientation = _origOrientation * NewOrientation;            
            float angle = MathHelper.ToDegrees(Helpers.UnsignedAngleBetweenTwoV3(Vector3.Up, NewOrientation.Up));
            //GameConsole.WriteEvent("ang " + angle);

            if (angle >= Config.BendAngleBeforeSnapping)
            {
                IsAttached = false;
                CActor.PhysXActor.ClearBodyFlag(BodyFlag.Kinematic);
            }
            else
            {
                AttachToGround();
            }

            Hit = false;
        }

        public void AttachToGround()
        {
            //FixedJointDescription jointDesc = new FixedJointDescription()
            //{
            //    Actor1 = CActor.PhysXActor,
            //    Actor2 = null
            //};

            // if this is the first time we weld to ground, initialize
            if (!_initialized)
            {
                _origOrientation = CActor.PhysXActor.GlobalOrientation;
                CActor.PhysXActor.RaiseBodyFlag(BodyFlag.Kinematic);
                _initialized = true;
            }
            IsAttached = true;
            //    Anchor = CActor.PhysXActor.Shapes[0].GlobalPosition;
            //    Anchor.Y = CActor.PhysXActor.GlobalPosition.Y;
            //    Position = CActor.PhysXActor.GlobalPosition;

            //    // physx joints can be unstable with long thin objects like lamposts so we widen them out
            //    using (UtilitiesLibrary lib = new UtilitiesLibrary())
            //    {
            //        Vector3 size = Config.BoundingBox.GetSize();
            //        if (size.X < size.Y / 4) size.X = size.Y / 4;
            //        if (size.Z < size.Y / 4) size.Z = size.Y / 4;
            //        Vector3 inertiaTensor = lib.ComputeBoxInteriaTensor(Vector3.Zero, Config.Mass, size);
            //        CActor.PhysXActor.MassSpaceInertiaTensor = inertiaTensor;
            //    }
            //    //CActor.PhysXActor.SolverIterationCount = 1;
            //}

            //CActor.PhysXActor.GlobalPosition = Position;
            //jointDesc.SetGlobalAnchor(Anchor);
            //jointDesc.SetGlobalAxis(new Vector3(0.0f, 1.0f, 0.0f));
            //Joint = PhysX.Instance.Scene.CreateJoint(jointDesc);
                        
           
        }
    }
}
