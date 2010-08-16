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
        public WheelShape Shape { get; private set; }
        public CWheelActor CActor { get; private set; }
        private Matrix _renderMatrix = Matrix.Identity;
        private Matrix _rotationMatrix = Matrix.Identity;
        private VehicleChassis _chassis;
        public bool IsRear;
        private float _axleOffset;
        private float _handbrake;
        public ParticleEmitter SmokeEmitter;
        public bool IsSkiddingLat, IsSkiddingLng, ShouldPlaySkidSound;
        public Vector3 ContactPoint;
        public int Index;
        private TireFunctionDescription _latTireFn, _lngTireFn;
        private float _defaultLatExtremum, _defaultLngExtremum;
        public float LatSlip;
        public float CurrentSuspensionTravel;

        public bool InAir
        {
            get { return Shape.GetContactData().ContactForce == 0; }
        }

        public VehicleWheel(VehicleChassis chassis, CWheelActor cactor, WheelShape wheel, float axleOffset)
        {
            Shape = wheel;
            _chassis = chassis;
            _axleOffset = axleOffset;
            CActor = cactor;

            SmokeEmitter = new ParticleEmitter(null, 15, Vector3.Zero);

            IsRear = !CActor.IsFront;

            _latTireFn = Shape.LateralTireForceFunction;
            _lngTireFn = Shape.LongitudalTireForceFunction;
            _defaultLatExtremum = _latTireFn.ExtremumValue;
            _defaultLngExtremum = _lngTireFn.ExtremumValue;
        }

        public Vector3 GlobalPosition
        {
            get { return Shape.GlobalPosition; }
        }

        public void Update()
        {
            WheelContactData wcd = Shape.GetContactData();
            UpdateMatrices(wcd);
            ContactPoint = wcd.ContactPoint;

            SmokeEmitter.Enabled = false;
            IsSkiddingLat = IsSkiddingLng = ShouldPlaySkidSound = false;

            if (wcd.ContactForce != 0)
            {
                int materialIndex = (int)wcd.OtherShapeMaterialIndex;
                MaterialModifier materialModifier = Race.Current.ConfigFile.MaterialModifiers[materialIndex];
                materialModifier.UpdateWheelShape(_chassis, this);

                LatSlip = wcd.LateralSlip;

                if (_chassis.Speed > 10 && (_handbrake == 1 || Math.Abs(wcd.LateralSlip) > 0.23f))
                {
                    IsSkiddingLat = true;
                    SmokeEmitter.Enabled = true;
                }
                else if (_chassis.Speed > 3 && Shape.MotorTorque != 0 && CActor.IsDriven && wcd.LongitudalSlip > 0.04f)
                {
                    IsSkiddingLng = true;
                    SmokeEmitter.Enabled = true;
                }

                // Setup tire functions taking into account handbrake and terrain
                float latExtremum = _defaultLatExtremum;
                if (IsRear)
                    latExtremum = MathHelper.Lerp(2.1f, 1.2f, _handbrake);
                latExtremum *= materialModifier.TyreRoadFriction;
                latExtremum *= _chassis._lateralFrictionMultiplier;
                _latTireFn.ExtremumValue = latExtremum;

                _lngTireFn.ExtremumValue = _defaultLngExtremum * materialModifier.TyreRoadFriction;
                Shape.LateralTireForceFunction = _latTireFn;
                Shape.LongitudalTireForceFunction = _lngTireFn;

                ShouldPlaySkidSound = IsSkiddingLat | IsSkiddingLng && materialIndex == 0;
                SmokeEmitter.Update(wcd.ContactPoint);

                if (IsSkiddingLat | IsSkiddingLng)
                {
                    _chassis.Vehicle.SkidMarkBuffer.AddSkid(this, wcd.ContactPoint);
                }
            }
        }

        public void ApplyHandbrake(float amount)
        {
            _handbrake = amount;
            if (amount == 1)
            {
                Shape.MotorTorque = 0;
                Shape.BrakeTorque = 800;
            }
        }


        private void UpdateMatrices(WheelContactData wcd)
        {
            if (wcd.ContactShape == null)
                CurrentSuspensionTravel = Shape.SuspensionTravel;
            else
                CurrentSuspensionTravel = wcd.ContactPosition - Shape.Radius;

            if (_handbrake != 1)
            {
                if (IsSkiddingLng && Shape.MotorTorque != 0)
                {
                    _rotationMatrix *= Matrix.CreateRotationX(-0.3f);
                }
                else
                    _rotationMatrix *= Matrix.CreateRotationX(MathHelper.ToRadians(Shape.AxleSpeed));
            }
            else
            {
            }

            Matrix translation = Matrix.CreateTranslation(_axleOffset, -CurrentSuspensionTravel, 0.0f);
            _renderMatrix = _rotationMatrix * Matrix.CreateRotationY(Shape.SteeringAngle) * translation;
        }

        public Matrix GetRenderMatrix()
        {
            Matrix pose = Shape.GlobalPose;
            Vector3 trans = pose.Translation;
            return _renderMatrix * pose;
        }
    }
}