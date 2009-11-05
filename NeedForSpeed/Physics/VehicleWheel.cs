using System;
using System.Collections.Generic;

using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework;
using StillDesign.PhysX;
using NFSEngine;

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
        private float _lastImpulse;
        private float _threshold;
        float _lerper;

        public VehicleWheel(VehicleChassis chassis, WheelShape wheel, float threshold)
        {
            _wheel = wheel;
            _chassis = chassis;
            _threshold = threshold;
        }

        public Vector3 GlobalPosition
        {
            get { return _wheel.GlobalPosition; }
        }

        public void Update(float speed, float latSpeed)
        {
            
            WheelContactData wcd = _wheel.GetContactData();

            UpdateMatrices(wcd);

            float latImpulse = Math.Abs(wcd.LateralImpulse) * 0.0001f;// *latSpeed;
            GameConsole.WriteLine("latImpulse", latImpulse);


            if (latImpulse > 6 || latImpulse == 0)
            {
                _tireOverride = 0.05f;
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


            //if (wcd.ContactForce == 0 || latSpeed > 30)
            //{
            //    if (_airTime < MaxAirTime)
            //    {
            //        if (_airTime == 0) _airTime = 0.3f;
            //        _airTime += Engine.Instance.ElapsedSeconds;
            //        _tire = 0.05f;
            //    }
                
            //}
            //else
            //{

            //    if (_airTime > 0)
            //    {
            //        _airTime -= Engine.Instance.ElapsedSeconds * 0.8f;
            //        if (_airTime < 0) _airTime = 0;
            //    }
                
            //}
            _lastImpulse = latImpulse;
            //GameConsole.WriteLine("airtime", _airTime);// > 0 ? "SKID" : "");
        }

        public float GetStiffness(float stiffness)
        {
            if (_tireOverride == 0) return stiffness;

            if (_lerper == 0)
                return _tireOverride;
            else
            {
                return MathHelper.Lerp(0.05f, stiffness/2, _lerper);
            }

            return stiffness;
            if (_airTime > 0f)
            {
                //_stiffness += Engine.Instance.ElapsedSeconds;
                //return _stiffness;
                _tireOverride += 0.0002f;
                //if (_tire > 0.8f) _tire = 0.8f;
                //if (tilt > 15) return 0.005f;
                return _tireOverride;
                //return 0.05f;
                return MathHelper.Lerp(0.7f, 0.01f, _airTime / MaxAirTime);
            }
            
            _airTime = 0;
            return stiffness;
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
            Matrix translation = Matrix.CreateTranslation(0.17f, -suspensionLength, 0.0f);
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