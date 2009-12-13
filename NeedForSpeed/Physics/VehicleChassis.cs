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

namespace Carmageddon.Physics
{
    
    class VehicleChassis
    {
        #region Fields

        private Actor VehicleBody;
        private WheelShape FLWheel;
        private WheelShape FRWheel;
        private WheelShape RLWheel;
        private WheelShape RRWheel;
        public List<VehicleWheel> Wheels {get; private set; }

        float _desiredSteerAngle = 0f; // Desired steering angle
        bool _backwards = false;
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
        public VehicleChassis(Scene scene, Matrix pose, int id, Carmageddon.Parsers.PhysicalProperties properties)
        {
            Wheels = new List<VehicleWheel>();
            
            ActorDescription actorDesc = new ActorDescription();
            BodyDescription bodyDesc = new BodyDescription(properties.Mass);
            actorDesc.BodyDescription = bodyDesc;
            
            BoxShapeDescription boxDesc = new BoxShapeDescription();
            float w = properties.BoundingBox.Max.X - properties.BoundingBox.Min.X;
            float h = properties.BoundingBox.Max.Y - properties.BoundingBox.Min.Y;
            float l = properties.BoundingBox.Max.Z - properties.BoundingBox.Min.Z;
            boxDesc.Size = new Vector3(w, h, l);
            boxDesc.LocalPosition = properties.BoundingBox.GetCenter();
            
            actorDesc.Shapes.Add(boxDesc);

            foreach (Vector3 extraPoint in properties.ExtraBoundingBoxPoints)
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

            float mul = 70f;
            _frontLateralTireFn.ExtremumSlip = 0.35f;
            _frontLateralTireFn.ExtremumValue = 1.7f;
            _frontLateralTireFn.AsymptoteSlip = 1.4f * mul;
            _frontLateralTireFn.AsymptoteValue = 0.7f;

            _rearLateralTireFn = new TireFunctionDescription();

            mul = 100f;
            _rearLateralTireFn.ExtremumSlip = 0.35f;
            _rearLateralTireFn.ExtremumValue = 2.0f;
            _rearLateralTireFn.AsymptoteSlip = 1.4f * mul;
            _rearLateralTireFn.AsymptoteValue = 0.7f;

            
            WheelShapeDescription wheelDesc = new WheelShapeDescription();
            wheelDesc.Radius = properties.NonDrivenWheelRadius;
            wheelDesc.SuspensionTravel = 0.16f;
            wheelDesc.InverseWheelMass = 0.1f;
            wheelDesc.LongitudalTireForceFunction = lngTFD;
            wheelDesc.LateralTireForceFunction = _frontLateralTireFn;
            wheelDesc.Flags = (WheelShapeFlag)64;  // clamp force mode

            
            MaterialDescription md = new MaterialDescription();
            md.Restitution = 0.2f;
            md.Flags = MaterialFlag.DisableFriction;
            Material m = scene.CreateMaterial(md);
            wheelDesc.Material = m;

            SpringDescription spring = new SpringDescription(11000, properties.SuspensionDamping, 0);
            //float heightModifier = (suspensionSettings.WheelSuspension + wheelDesc.Radius) / suspensionSettings.WheelSuspension;
            //spring.SpringCoefficient = suspensionSettings.SpringRestitution * heightModifier;
            //spring.DamperCoefficient = suspensionSettings.SpringDamping * heightModifier;
            //spring.TargetValue = suspensionSettings.SpringBias * heightModifier;
            wheelDesc.Suspension = spring;

            wheelDesc.LocalPosition = properties.WheelPositions[0];
            FLWheel = (WheelShape)VehicleBody.CreateShape(wheelDesc);
            FLWheel.Name = "FL-Wheel";
            FLWheel.LateralTireForceFunction = _frontLateralTireFn;
            
            wheelDesc.LocalPosition = properties.WheelPositions[1];
            FRWheel = (WheelShape)VehicleBody.CreateShape(wheelDesc);
            FRWheel.Name = "FR-Wheel";
            FRWheel.LateralTireForceFunction = _frontLateralTireFn;

            wheelDesc.Radius = properties.DrivenWheelRadius;

            wheelDesc.LocalPosition = properties.WheelPositions[2];
            RLWheel = (WheelShape)VehicleBody.CreateShape(wheelDesc);
            RLWheel.Name = "RL-Wheel";
            RLWheel.LateralTireForceFunction = _rearLateralTireFn;

            wheelDesc.LocalPosition = properties.WheelPositions[3];
            RRWheel = (WheelShape)VehicleBody.CreateShape(wheelDesc);
            RRWheel.Name = "RR-Wheel";
            RRWheel.LateralTireForceFunction = _rearLateralTireFn;

            Wheels.Add(new VehicleWheel(this, FLWheel, 0.17f));
            Wheels.Add(new VehicleWheel(this, FRWheel, -0.17f));
            Wheels.Add(new VehicleWheel(this, RLWheel, 0.17f));
            Wheels.Add(new VehicleWheel(this, RRWheel, -0.17f));

            Vector3 massPos = VehicleBody.CenterOfMassLocalPosition;
            massPos = properties.CenterOfMass;
            massPos.Y = ((properties.WheelPositions[0].Y + properties.WheelPositions[2].Y) / 2) -0.18f;
            
            _centerOfMass = massPos;
            VehicleBody.SetCenterOfMassOffsetLocalPosition(massPos);
            
            VehicleBody.WakeUp(60.0f);

            VehicleBody.RaiseBodyFlag(BodyFlag.Visualization);

            //a real power curve doesnt work too well in carmageddon :)
            List<float> power = new List<float>(new float[] { 0.5f, 0.5f, 1f, 1f, 1f, 1.0f, 1.0f, 0 });
            List<float> ratios = new List<float>(new float[] { 2.8f, 2.160f, 1.685f, 1.212f, 0.900f });

            BaseGearbox gearbox = BaseGearbox.Create(false, ratios, 0.4f);
            Motor = new Motor(power, 180, 6f, gearbox);

            //List<float> power = new List<float>(new float[] { 0.2f, 0.3f, 0.4f, 0.7f, 0.8f, 1.0f, 1.0f, 1.0f, 0 });
            //List<float> ratios = new List<float>(new float[] { 2.5f, 2.0f, 1.685f, 1.712f, 1.000f });

            //BaseGearbox gearbox = BaseGearbox.Create(false, ratios, 0.4f);
            //Motor = new Motor(power, 480, 7f, gearbox);
        }

        public void Delete()
        {
            VehicleBody.Dispose();
            VehicleBody = null;
        }

        #endregion

        #region Update

        public void Update(GameTime gameTime)
        {
            Vector3 vDirection = VehicleBody.GlobalOrientation.Forward;
            Vector3 vNormal = VehicleBody.LinearVelocity * vDirection;
            _speed = vNormal.Length() * 3.5f;

            float endLocal = _desiredSteerAngle / (1 + _speed * 0.02f);
            float diff = Math.Abs(endLocal - _steerAngle);
            float max = 0.0007f;
            if (_desiredSteerAngle == 0) max = 0.0005f;
            if (diff > 0.0025f) // Is the current steering angle ~= desired steering angle?
            { // If not, adjust carefully
                
                if (diff > max)
                    diff = max; // Steps shouldn't be too large
                else
                    diff *= 0.05f;

                diff *= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (endLocal > _steerAngle)
                {
                    _steerAngle += diff;
                }
                else
                {
                    _steerAngle -= diff;
                }
                FLWheel.SteeringAngle = _steerAngle;
                FRWheel.SteeringAngle = _steerAngle;
            }
            
            Vector3 orientation = VehicleBody.GlobalOrientation.Up;

            if (orientation.Y < 0 && _speed < 1f)
            {
                Reset();
                return;
            }

            if (_speed < 1f) // Change between breaking and accelerating;
            {
                if (_backwards && _currentTorque > 0.01f)
                {
                    _backwards = false;
                    //Accelerate(_currentTorque);
                }
                else if (!_backwards && _currentTorque < 0.01f)
                {
                    _backwards = true;
                    //Accelerate(_currentTorque);
                }
            }
            Vector3 down = VehicleBody.GlobalOrientation.Down;
            down = Vector3.Normalize(down);
            VehicleBody.AddLocalForceAtLocalPosition(down * new Vector3(5, 5, 5), Vector3.Zero, ForceMode.SmoothImpulse);
            
            UpdateTireStiffness();

        }

        private void UpdateTireStiffness()
        {
            Vector3 vNormal = VehicleBody.LinearVelocity * VehicleBody.GlobalOrientation.Left;
            float lateralSpeed = vNormal.Length();
            GameConsole.WriteLine("Lat Speed", lateralSpeed);
            GameConsole.WriteLine("Handbrake", _handbrake);
            

            Wheels[0].Update();
            Wheels[1].Update();

            float angVel = MathHelper.Lerp(2, 1.5f, _handbrake);
            Wheels[2].Update();
            Wheels[3].Update();

            _rearLateralTireFn.ExtremumValue = angVel;
            Wheels[2].WheelShape.LateralTireForceFunction = _rearLateralTireFn;
            Wheels[3].WheelShape.LateralTireForceFunction = _rearLateralTireFn;
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
                if (_backwards)
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
                if (_backwards)
                {
                    _motorTorque = -torque;
                    _brakeTorque = 0f;
                }
                else
                {
                    _motorTorque = 0.0f;
                    _brakeTorque = -torque * 2;
                }
            }
            else
            {
                _motorTorque = 0.0f;
                _brakeTorque = Motor.CurrentFriction * 0.55f;
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
            _handbrake -= Engine.Instance.ElapsedSeconds*0.4f;
            if (_handbrake < 0) _handbrake = 0;
        }

        #endregion

        #region Helper

        private void UpdateTorque()
        {
            if (_handbrake == 1)
            {
                RLWheel.MotorTorque = RRWheel.MotorTorque = 0;
                float brakeTorque = MathHelper.Lerp(0, 700, _handbrake);
                RLWheel.BrakeTorque = brakeTorque;
                RRWheel.BrakeTorque = brakeTorque;
                return;
            }
            else
            {
                RLWheel.MotorTorque = _motorTorque;
                RRWheel.MotorTorque = _motorTorque;
            }

            FLWheel.BrakeTorque = _brakeTorque;
            FRWheel.BrakeTorque = _brakeTorque;
            RLWheel.BrakeTorque = _brakeTorque;
            RRWheel.BrakeTorque = _brakeTorque;
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
                return Wheels[0].InAir && Wheels[1].InAir && Wheels[2].InAir && Wheels[3].InAir;
            }
        }
    }
}
