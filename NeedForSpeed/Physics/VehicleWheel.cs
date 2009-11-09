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

        public void Update(bool handbrake)
        {

            WheelContactData wcd = _wheel.GetContactData();

            UpdateMatrices(wcd);

            float latImpulse = Math.Abs(wcd.LateralImpulse) * 0.0001f;// *latSpeed;
            GameConsole.WriteLine("latImpulse", latImpulse);


            if (handbrake && _isRear)
            {
                _tireOverride = 0.09f;
                _lerper = 0;
            }
            else if (latImpulse > 6 || latImpulse == 0 || (handbrake && _isRear))
            {
                _tireOverride = 0.08f;
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

            _airTime = latImpulse;

            _smokeEmitter.Enabled = false;
            if (wcd.ContactForce > 0 && (_tireOverride > 0 || _lerper < 1))
            {
                _smokeEmitter.Enabled = true;
                _smokeEmitter.Update(Engine.Instance.ElapsedSeconds, wcd.ContactPoint);
            }
        }

        public float GetStiffness(float stiffness)
        {
            if (_tireOverride == 0) return stiffness;

            if (_lerper == 0)
                return _tireOverride;
            else
            {
                return MathHelper.Lerp(_tireOverride, stiffness / 2, _lerper);
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
            //trans.X -= 0.034f;
            //pose.Translation = trans;
            return _renderMatrix * pose;
        }

        
    }
}