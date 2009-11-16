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
        private WheelShape _wheel;
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

        public VehicleWheel(VehicleChassis chassis, WheelShape wheel, float axleOffset)
        {
            _wheel = wheel;
            _chassis = chassis;
            _axleOffset = axleOffset;
            if (_wheel.Name.StartsWith("R")) _isRear = true;

            if (_smokeEmitter == null)
                _smokeEmitter = new ParticleEmitter(TyreSmokeParticleSystem.Instance, 20, Vector3.Zero);
        }

        public Vector3 GlobalPosition
        {
            get { return _wheel.GlobalPosition; }
        }

        public void Update(float stiffness)
        {
            WheelContactData wcd = _wheel.GetContactData();

            UpdateMatrices(wcd);

            float latImpulse = Math.Abs(wcd.LateralImpulse) * 0.0001f;
            GameConsole.WriteLine("latImpulse", latImpulse);

            if (latImpulse > 0) _airTime = 0;
            else { _airTime += Engine.Instance.ElapsedSeconds * 2; if (_airTime > 1) _airTime = 1; }

            if (latImpulse == 0)
            {
                _tireOverride = MathHelper.Lerp(0.15f, 0.05f, _airTime);
                _lerper = 0;
            }
            else if (latImpulse > 6)
            {
                _tireOverride = 0.12f;
                _lerper = 0;
            }
            else if (latImpulse < 3 && _lerper == 0 && _tireOverride > 0)
            {
                _lerper = 0.001f;
            }

            if (_lerper > 0 && _lerper < 1)
            {
                _lerper += Engine.Instance.ElapsedSeconds * 0.4f;
                if (_lerper > 1)
                {
                    _lerper = 1;
                    _tireOverride = 0f;
                }
            }

            _smokeEmitter.Enabled = false;
            if (_chassis.Speed > 5 && wcd.ContactForce > 0 && (_tireOverride > 0 && _lerper == 0))
            {
                _smokeEmitter.Enabled = true;
                _smokeEmitter.Update(Engine.Instance.ElapsedSeconds, wcd.ContactPoint);
            }

            // Actually set the computed stiffness
            if (_tireOverride == 0) 
                LateralStiffness = stiffness;
            else  if (_lerper == 0)
                LateralStiffness = _tireOverride;
            else
            {
                LateralStiffness = MathHelper.Lerp(_tireOverride, stiffness / 2, _lerper);
            }
        }


        private void UpdateMatrices(WheelContactData wcd)
        {
            float suspensionLength = 0;

            if (wcd.ContactShape == null)
                suspensionLength = _wheel.SuspensionTravel;
            else
                suspensionLength = wcd.ContactPosition - _wheel.Radius;

            _rotationMatrix *= Matrix.CreateRotationX(MathHelper.ToRadians(_wheel.AxleSpeed));
            _renderMatrix = Matrix.Identity;
            Matrix translation = Matrix.CreateTranslation(_axleOffset, -suspensionLength, 0.0f);
            _renderMatrix = _rotationMatrix * Matrix.CreateRotationY(_wheel.SteeringAngle) * translation;
        }

        public Matrix GetRenderMatrix()
        {
            Matrix pose = _wheel.GlobalPose;
            Vector3 trans = pose.Translation;
            return _renderMatrix * pose;
        }
    }
}