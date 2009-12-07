using System;
using System.Collections.Generic;

using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework;
using StillDesign.PhysX;
using NFSEngine;
using Particle3DSample;
using Carmageddon.Gfx;

namespace Carmageddon.Physics
{
    class VehicleWheel
    {
        public WheelShape WheelShape { get; private set; }
        private const float MaxAirTime = 2;
        private float _airTime;
        private float _tireOverride;
        private Matrix _renderMatrix=Matrix.Identity;
        private Matrix _rotationMatrix=Matrix.Identity;
        private VehicleChassis _chassis;
        private bool _isRear;
        private float _axleOffset;
        float _lerper;
        ParticleEmitter _smokeEmitter;
        public float LateralStiffness { get; private set; }

        public bool InAir
        {
            get
            {
                return WheelShape.GetContactData().ContactForce == 0; 
            }
        }

        public VehicleWheel(VehicleChassis chassis, WheelShape wheel, float axleOffset)
        {
            WheelShape = wheel;
            _chassis = chassis;
            _axleOffset = axleOffset;
            if (WheelShape.Name.StartsWith("R")) _isRear = true;

            if (_smokeEmitter == null)
                _smokeEmitter = new ParticleEmitter(TyreSmokeParticleSystem.Instance, 15, Vector3.Zero);
        }

        public Vector3 GlobalPosition
        {
            get { return WheelShape.GlobalPosition; }
        }

        public void Update()
        {
            WheelContactData wcd = WheelShape.GetContactData();

            UpdateMatrices(wcd);

            //GameConsole.WriteLine("latImpulse", wcd.LongitudalSlip);
            
            _smokeEmitter.Enabled = false;
            if (_chassis.Speed > 5 && Math.Abs(wcd.LateralSlip) > 0.2f)
            {
                _smokeEmitter.Enabled = true;
                _smokeEmitter.Update(Engine.Instance.ElapsedSeconds, wcd.ContactPoint);
            }
        }


        private void UpdateMatrices(WheelContactData wcd)
        {
            float suspensionLength = 0;

            if (wcd.ContactShape == null)
                suspensionLength = WheelShape.SuspensionTravel;
            else
                suspensionLength = wcd.ContactPosition - WheelShape.Radius;

            _rotationMatrix *= Matrix.CreateRotationX(MathHelper.ToRadians(WheelShape.AxleSpeed));
            
            Matrix translation = Matrix.CreateTranslation(_axleOffset, -suspensionLength, 0.0f);
            _renderMatrix = _rotationMatrix * Matrix.CreateRotationY(WheelShape.SteeringAngle) * translation;
        }

        public Matrix GetRenderMatrix()
        {
            Matrix pose = WheelShape.GlobalPose;
            Vector3 trans = pose.Translation;
            return _renderMatrix * pose;
        }
    }
}