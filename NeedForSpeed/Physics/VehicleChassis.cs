using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StillDesign.PhysX;
using OneamEngine;
using NFSEngine;
using PlatformEngine;
using Carmageddon;
using Carmageddon.Parsers;

namespace Carmageddon.Physics
{
    
    class VehicleChassis
    {
        #region Fields

        private Actor VehicleBody;
        //private WheelShape FLWheel;
        //private WheelShape FRWheel;
        //private WheelShape RLWheel;
        //private WheelShape RRWheel;
        public List<VehicleWheel> Wheels {get; private set; }

        float _desiredSteerAngle = 0f; // Desired steering angle
        public bool Backwards {get; private set; }
        float _currentTorque = 0f;
        Vector3 _centerOfMass;

        private float _handbrake;

        float _steerAngle = 0.0f;
        float _motorTorque = 0.0f;
        float _brakeTorque = 0.0f;
        float _speed = 0.0f;

        public Motor Motor { get; private set; }

        TireFunctionDescription _frontLateralTireFn, _rearLateralTireFn;

        #endregion


        public Actor Body
        {
            get { return VehicleBody; }
        }


        public float Speed { get { return _speed; } }
        


        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public VehicleChassis(Scene scene, Matrix pose, int id, CarFile carFile)
        {
            Wheels = new List<VehicleWheel>();
            
            ActorDescription actorDesc = new ActorDescription();
            BodyDescription bodyDesc = new BodyDescription(carFile.Mass);
            actorDesc.BodyDescription = bodyDesc;
            
            BoxShapeDescription boxDesc = new BoxShapeDescription();
            float w = carFile.BoundingBox.Max.X - carFile.BoundingBox.Min.X;
            float h = carFile.BoundingBox.Max.Y - carFile.BoundingBox.Min.Y;
            float l = carFile.BoundingBox.Max.Z - carFile.BoundingBox.Min.Z;
            boxDesc.Size = new Vector3(w, h, l);
            boxDesc.LocalPosition = carFile.BoundingBox.GetCenter();
            
            actorDesc.Shapes.Add(boxDesc);

            foreach (Vector3 extraPoint in carFile.ExtraBoundingBoxPoints)
            {
                boxDesc = new BoxShapeDescription(0.2f, 0.2f, 0.2f);
                boxDesc.LocalPosition = extraPoint;
                boxDesc.Mass = 0;
                actorDesc.Shapes.Add(boxDesc);
            }

            actorDesc.GlobalPose = pose;
            VehicleBody = scene.CreateActor(actorDesc);
            VehicleBody.Name = "Vehicle";
            VehicleBody.Group = 1;

            TireFunctionDescription lngTFD = new TireFunctionDescription();
            lngTFD.ExtremumSlip = 0.1f;
            lngTFD.ExtremumValue = 5f;
            lngTFD.AsymptoteSlip = 2.0f;
            lngTFD.AsymptoteValue = 5.0f;
            

            TireFunctionDescription _frontLateralTireFn = new TireFunctionDescription();

            float mul = 20f;
            _frontLateralTireFn.ExtremumSlip = 0.30f;
            _frontLateralTireFn.ExtremumValue = 2.0f;
            _frontLateralTireFn.AsymptoteSlip = 1.4f * mul;
            _frontLateralTireFn.AsymptoteValue = 0.01f;

            _rearLateralTireFn = new TireFunctionDescription();

            mul = 20f;
            _rearLateralTireFn.ExtremumSlip = 0.33f;
            _rearLateralTireFn.ExtremumValue = 2.1f;
            _rearLateralTireFn.AsymptoteSlip = 1.4f * mul;
            _rearLateralTireFn.AsymptoteValue = 0.01f;

            
            WheelShapeDescription wheelDesc = new WheelShapeDescription();
            wheelDesc.Radius = carFile.NonDrivenWheelRadius;

            wheelDesc.SuspensionTravel = carFile.NonDrivenWheelRadius / 2; // carFile.SuspensionGiveFront; // carFile.RideHeight * 2.8f; // 0.18f;
            wheelDesc.InverseWheelMass = 0.08f;
            wheelDesc.LongitudalTireForceFunction = lngTFD;
            wheelDesc.LateralTireForceFunction = _frontLateralTireFn;
            wheelDesc.Flags = WheelShapeFlag.ClampedFriction;

            
            MaterialDescription md = new MaterialDescription();
            md.Flags = MaterialFlag.DisableFriction;
            Material m = scene.CreateMaterial(md);
            wheelDesc.Material = m;

            SpringDescription spring = new SpringDescription();
            float heightModifier = (wheelDesc.SuspensionTravel + wheelDesc.Radius) / wheelDesc.SuspensionTravel;
            spring.SpringCoefficient = 3600 * heightModifier;
            spring.DamperCoefficient = carFile.SuspensionDamping * 4;
            //spring.TargetValue = 0.5f; // 0.5f * heightModifier;
            wheelDesc.Suspension = spring;

            foreach (CWheelActor wheel in carFile.WheelActors)
            {
                wheelDesc.LocalPosition = wheel.Position + new Vector3(0, wheelDesc.SuspensionTravel/2, 0);
                wheelDesc.Radius = wheel.IsDriven ? carFile.DrivenWheelRadius : carFile.NonDrivenWheelRadius;

                WheelShape ws = (WheelShape)VehicleBody.CreateShape(wheelDesc);
                ws.Name = wheel.Actor.Name;
                ws.LateralTireForceFunction = wheel.IsFront ? _frontLateralTireFn : _rearLateralTireFn;

                Wheels.Add(new VehicleWheel(this, wheel, ws, wheel.IsLeft ? 0.17f : -0.17f) { Index = Wheels.Count });
            }

            Vector3 massPos = VehicleBody.CenterOfMassLocalPosition;
            massPos = carFile.CenterOfMass;
            massPos.Y = carFile.WheelActors[0].Position.Y - carFile.NonDrivenWheelRadius + 0.48f /*carFile.CenterOfMass.Y*/ - 0.2f;
            
            _centerOfMass = massPos;
            VehicleBody.SetCenterOfMassOffsetLocalPosition(massPos);
            
            VehicleBody.WakeUp(60.0f);

            VehicleBody.RaiseBodyFlag(BodyFlag.Visualization);

            //a real power curve doesnt work too well in carmageddon :)
            List<float> power = new List<float>(new float[] { 0.5f, 0.5f, 0.5f, 1f, 1f, 1.0f, 1.0f, 0 });
            List<float> ratios = new List<float>(new float[] { 3.227f, 2.360f, 1.685f, 1.312f, 1.000f, 0.793f });

            BaseGearbox gearbox = BaseGearbox.Create(false, ratios, 0.4f);
            Motor = new Motor(power, carFile.EnginePower * 132.5f, 6f, carFile.TopSpeed, gearbox);
            Motor.Gearbox.CurrentGear = 0;
        }

        public void Delete()
        {
            VehicleBody.Dispose();
            VehicleBody = null;
        }

        #endregion

        public float SteerRatio
        {
            get { return _steerAngle / 0.5f; }
        }

        #region Update

        public void Update()
        {
            GameConsole.WriteLine("Steer2", _steerAngle);
            Vector3 vDirection = VehicleBody.GlobalOrientation.Forward;
            Vector3 vNormal = VehicleBody.LinearVelocity * vDirection;
            _speed = vNormal.Length() * 2.9f;

            float endLocal = _desiredSteerAngle / (1 + _speed * 0.02f);
            float diff = Math.Abs(endLocal - _steerAngle);
            float max = 0.0007f;
            if (_desiredSteerAngle == 0) max = 0.0005f;
            if (_desiredSteerAngle < -0.1f && _steerAngle > 0.1f || _desiredSteerAngle > 0.1f && _steerAngle < -0.1f) max = 0.002f;
            if (diff > 0.0025f) // Is the current steering angle ~= desired steering angle?
            { // If not, adjust carefully

                if (diff > max)
                    diff = max; // Steps shouldn't be too large
                else
                    diff *= 0.05f;

                diff *= Engine.Instance.ElapsedSeconds * 1000; //TotalMilliseconds;
                if (endLocal > _steerAngle)
                {
                    _steerAngle += diff;
                }
                else
                {
                    _steerAngle -= diff;
                }
                foreach (VehicleWheel wheel in Wheels)
                {
                    if (wheel.CActor.IsSteerable)
                        wheel.Shape.SteeringAngle = _steerAngle;
                }
            }
            
            Vector3 orientation = VehicleBody.GlobalOrientation.Up;

            if (orientation.Y < 0 && _speed < 1f)
            {
                Reset();
                return;
            }

            if (_speed < 1f) // Change between braking and accelerating;
            {
                if (Backwards && _currentTorque > 0.01f)
                {
                    Backwards = false;
                    Motor.Gearbox.CurrentGear = 1;
                }
                else if (!Backwards && _currentTorque < -0.01f)
                {
                    Backwards = true;
                    Motor.Gearbox.CurrentGear = -1;
                }
            }
            //Vector3 down = VehicleBody.GlobalOrientation.Down;
            //down = Vector3.Normalize(down);
            //VehicleBody.AddLocalForceAtLocalPosition(down * new Vector3(5), Vector3.Zero, ForceMode.SmoothImpulse);
            
            UpdateTireStiffness();

            Motor.WheelsSpinning = false;
            foreach (VehicleWheel wheel in Wheels)
            {
                if (wheel.CActor.IsDriven && (wheel.IsSkiddingLng || wheel.InAir))
                {
                    Motor.WheelsSpinning = true;
                    break;
                }
            }

        }

        private void UpdateTireStiffness()
        {
            Vector3 vNormal = VehicleBody.LinearVelocity * VehicleBody.GlobalOrientation.Left;
            float lateralSpeed = vNormal.Length();
            GameConsole.WriteLine("Lat Speed", lateralSpeed);
            GameConsole.WriteLine("Handbrake", _handbrake);

            //float angVel = MathHelper.Lerp(2.1f, 1.7f, _handbrake);
            //_rearLateralTireFn.ExtremumValue = angVel;

            foreach (VehicleWheel wheel in Wheels)
            {
                wheel.Update();
                //if (wheel.IsRear)
                //{
                //    wheel.Shape.LateralTireForceFunction = _rearLateralTireFn;
                //}
            }
        }

        #endregion
    
        #region HandleInput

        public void Accelerate(float value)
        {
            Motor.Throttle = value;
            Motor.Update(_speed);

            float torque = Motor.CurrentPowerOutput;
            
            if (Motor.AtRedline && !Motor.WheelsSpinning)
            {
                torque *= 0.2f;
            }

            _currentTorque = torque;
            if (torque > 0.0001f)
            {
                if (Backwards)
                {
                    _motorTorque = 0f;
                    _brakeTorque = torque;
                }
                else
                {
                    _motorTorque = -torque;
                    _brakeTorque = 0.0f;
                }
            }

            else if (torque < -0.0001f)
            {
                if (Backwards)
                {
                    _motorTorque = -torque;
                    _brakeTorque = 0f;
                }
                else
                {
                    _motorTorque = 0.0f;
                    _brakeTorque = -torque * 1.2f;
                }
            }
            else
            {
                _motorTorque = 0.0f;
                _brakeTorque = Motor.CurrentFriction;
            }

            UpdateTorque();
            VehicleBody.WakeUp();
        }

        public void Steer(float angle)
        {
            _desiredSteerAngle = -angle*0.5f;
        }

        public void PullHandbrake()
        {
            _handbrake = 1;
        }

        public void ReleaseHandbrake()
        {
            if (_handbrake == 0) return;
            _handbrake -= Engine.Instance.ElapsedSeconds*0.8f;
            if (_handbrake < 0) _handbrake = 0;
        }

        #endregion

        #region Helper

        private void UpdateTorque()
        {
            foreach (VehicleWheel wheel in Wheels)
            {
                if (wheel.IsRear)
                    wheel.ApplyHandbrake(_handbrake);
                if (wheel.CActor.IsDriven)
                    wheel.Shape.MotorTorque = _motorTorque;
            }

            if (_handbrake == 1) return;

            foreach (VehicleWheel wheel in Wheels)
            {
                wheel.Shape.BrakeTorque = _brakeTorque;
            }
        }


        private void Reset()
        {
            VehicleBody.GlobalOrientation = Matrix.Identity;
            VehicleBody.GlobalPosition += new Vector3(0.0f, 1.0f, 0.0f);
            VehicleBody.LinearMomentum = VehicleBody.LinearVelocity = Vector3.Zero;
            VehicleBody.AngularMomentum = VehicleBody.AngularVelocity = Vector3.Zero;
        }

        #endregion

        public void Explode(float strength)
        {
            VehicleBody.AddForceAtLocalPosition(new Vector3(
                RandomNumber.NextFloat() * 200f - 100f,
                155f + RandomNumber.NextFloat() * 40f,
                RandomNumber.NextFloat() * 200f - 100f
            ) * 100f * strength,
            new Vector3(RandomNumber.NextFloat() - .5f, 0.1f, RandomNumber.NextFloat() * 2f - 1f), ForceMode.Impulse, true);
        }

        public void RocketImpact()
        {
            //VehicleBody.AddLocalForceAtLocalPosition(ROCKETIMPACT, new Vector3(1, 0, .5f), ForceMode.Impulse);
        }

        public bool InAir
        {
            get
            {
                foreach (VehicleWheel wheel in Wheels)
                    if (!wheel.InAir) return false;
                return true;
            }
        }
    }
}
