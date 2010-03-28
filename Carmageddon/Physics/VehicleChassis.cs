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
        public Vehicle Vehicle { get; set; }
        public Actor Actor { get { return _physXActor; } }
        public List<VehicleWheel> Wheels {get; private set; }
        public float _lateralFrictionMultiplier = 1;
        public Motor Motor { get; private set; }
        
        public bool Backwards {get; private set; }
        public float Speed { get; private set; }
        public float LastSpeed { get; private set; }

        private Actor _physXActor;
        float _currentTorque;
        float _desiredSteerAngle = 0f; // Desired 
        float _handbrake;
        float _steerAngle = 0.0f;
        float _motorTorque = 0.0f;
        float _brakeTorque = 0.0f;
        TireFunctionDescription _frontLateralTireFn, _rearLateralTireFn;
                
        
        public VehicleChassis(Matrix pose, Vehicle vehicle)
        {
            Vehicle = vehicle;

            Wheels = new List<VehicleWheel>();

            CarFile carFile = vehicle.Config;
            
            ActorDescription actorDesc = new ActorDescription();
            BodyDescription bodyDesc = new BodyDescription(carFile.Mass);
            
            actorDesc.BodyDescription = bodyDesc;
                        
            BoxShapeDescription boxDesc = new BoxShapeDescription();
            boxDesc.Size = carFile.BoundingBox.GetSize();
            boxDesc.LocalPosition = carFile.BoundingBox.GetCenter();
            boxDesc.Name = PhysXConsts.VehicleBody;
            actorDesc.Shapes.Add(boxDesc);

            foreach (Vector3 extraPoint in carFile.ExtraBoundingBoxPoints)
            {
                boxDesc = new BoxShapeDescription(0.2f, 0.2f, 0.2f);
                boxDesc.LocalPosition = extraPoint;
                boxDesc.Mass = 0;
                actorDesc.Shapes.Add(boxDesc);
            }

            using (UtilitiesLibrary lib = new UtilitiesLibrary())
            {
                Vector3 inertiaTensor = lib.ComputeBoxInteriaTensor(Vector3.Zero, carFile.Mass, carFile.Size);
                actorDesc.BodyDescription.MassSpaceInertia = inertiaTensor;
            }
            
            actorDesc.GlobalPose = pose;
            _physXActor = PhysX.Instance.Scene.CreateActor(actorDesc);

            
            _physXActor.Name = "Vehicle";
            _physXActor.Group = PhysXConsts.VehicleId;
            
            _physXActor.UserData = vehicle;
            _physXActor.MaximumAngularVelocity = 1.5f;
            
            

            TireFunctionDescription lngTFD = new TireFunctionDescription();
            lngTFD.ExtremumSlip = 0.1f;
            lngTFD.ExtremumValue = 5f;
            lngTFD.AsymptoteSlip = 2.0f;
            lngTFD.AsymptoteValue = 4.9f;
            

            _frontLateralTireFn = new TireFunctionDescription();
            _frontLateralTireFn.ExtremumSlip = 0.26f;
            _frontLateralTireFn.ExtremumValue = 1.8f;
            _frontLateralTireFn.AsymptoteSlip = 20;
            _frontLateralTireFn.AsymptoteValue = 0.001f;

            _rearLateralTireFn = new TireFunctionDescription();            
            _rearLateralTireFn.ExtremumSlip = 0.35f;
            
            _rearLateralTireFn.AsymptoteSlip = 20f;
            _rearLateralTireFn.AsymptoteValue = 0.001f;


            _rearLateralTireFn.ExtremumSlip = 0.2f;
            _rearLateralTireFn.ExtremumValue = 2.1f;
            _rearLateralTireFn.AsymptoteSlip = 0.0013f * carFile.Mass;
            _rearLateralTireFn.AsymptoteValue = 0.016f;

            _frontLateralTireFn = _rearLateralTireFn;
            //_frontLateralTireFn.AsymptoteSlip = 0.8f;
            _frontLateralTireFn.ExtremumValue = 2.1f;
           
            
            WheelShapeDescription wheelDesc = new WheelShapeDescription();

            wheelDesc.InverseWheelMass = 0.08f;
            wheelDesc.LongitudalTireForceFunction = lngTFD;
            wheelDesc.Flags = WheelShapeFlag.ClampedFriction;

            MaterialDescription md = new MaterialDescription();
            md.Flags = MaterialFlag.DisableFriction;
            Material m = PhysX.Instance.Scene.CreateMaterial(md);
            wheelDesc.Material = m;

            foreach (CWheelActor wheel in carFile.WheelActors)
            {
                wheelDesc.Radius = wheel.IsDriven ? carFile.DrivenWheelRadius : carFile.NonDrivenWheelRadius;
                wheelDesc.SuspensionTravel = (wheel.IsFront ? carFile.SuspensionGiveFront : carFile.SuspensionGiveRear) * 18; // Math.Max(wheelDesc.Radius / 2f, 0.21f);
                wheelDesc.LocalPosition = wheel.Position + new Vector3(0, wheelDesc.SuspensionTravel * carFile.Mass * 0.00045f, 0);
                
                SpringDescription spring = new SpringDescription();
                float heightModifier = (wheelDesc.SuspensionTravel + wheelDesc.Radius) / wheelDesc.SuspensionTravel;
                spring.SpringCoefficient = 3.6f * heightModifier * Math.Max(1000, carFile.Mass); // *(1 - (wheel.IsFront ? carFile.SuspensionGiveFront : carFile.SuspensionGiveRear));
                spring.DamperCoefficient = carFile.SuspensionDamping * 4f;
                
                wheelDesc.Suspension = spring;

                WheelShape ws = (WheelShape)_physXActor.CreateShape(wheelDesc);
                ws.Name = wheel.Actor.Name;
                ws.LateralTireForceFunction = wheel.IsFront ? _frontLateralTireFn : _rearLateralTireFn;

                Wheels.Add(new VehicleWheel(this, wheel, ws, wheel.IsLeft ? 0.17f : -0.17f) { Index = Wheels.Count });
            }

            _physXActor.WakeUp(60.0f);

            //a real power curve doesnt work too well in carmageddon :)
            List<float> power = new List<float>(new float[] { 0.5f, 0.5f, 0.5f, 1f, 1f, 1.0f, 1.0f, 0 });
            List<float> ratios = new List<float>(new float[] { 3.227f, 2.360f, 1.685f, 1.312f, 1.000f, 0.793f });

            BaseGearbox gearbox = BaseGearbox.Create(false, ratios, 0.4f);
            Motor = new Motor(power, carFile.EnginePower, 6f, carFile.TopSpeed, gearbox);
            Motor.Gearbox.CurrentGear = 0;

            //set center of mass
            //Vector3 massPos = _physXActor.CenterOfMassLocalPosition;
            Vector3 massPos = carFile.CenterOfMass;
            massPos.Y = carFile.WheelActors[0].Position.Y - carFile.NonDrivenWheelRadius + 0.36f;

            _physXActor.SetCenterOfMassOffsetLocalPosition(massPos);
        }

        public void Delete()
        {
            _physXActor.Dispose();
            _physXActor = null;
        }


        public float SteerRatio
        {
            get { return _steerAngle / 0.5f; }
        }


        public void Update()
        {
            LastSpeed = Speed;

            Vector3 vDirection = _physXActor.GlobalOrientation.Forward;
            Vector3 vNormal = _physXActor.LinearVelocity * vDirection;
            Speed = vNormal.Length() * 2.9f;

            float endLocal = _desiredSteerAngle; // / (1 + _speed * 0.02f);

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

                diff *= Engine.ElapsedSeconds * 1000; //TotalMilliseconds;
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

            if (_physXActor.GlobalOrientation.Up.Y < 0 && Speed < 1f)
            {
                Reset();
                return;
            }

            if (Speed < 1f) // Change between braking and accelerating;
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

            bool isSkiddingTooMuch = false;
            Motor.WheelsSpinning = false;
            foreach (VehicleWheel wheel in Wheels)
            {
                wheel.Update();
                if (wheel.CActor.IsDriven && (wheel.IsSkiddingLng || wheel.InAir))
                {
                    Motor.WheelsSpinning = true;
                }
                if (wheel.LatSlip > 1) isSkiddingTooMuch = true;
            }

            if (isSkiddingTooMuch || _physXActor.GlobalOrientation.Up.Y < 0)
            {
                _physXActor.LinearDamping = 0.7f;  //stop insane sliding
                _physXActor.AngularDamping = 0.5f;
            }
            else
            {
                _physXActor.LinearDamping = 0.0f;
                _physXActor.AngularDamping = 0.05f;
            }
        }


        public void OutputDebugInfo()
        {
            GameConsole.WriteLine("Speed", Speed);
            GameConsole.WriteLine("Brake", _brakeTorque);
            //GameConsole.WriteLine("ang vel", Actor.AngularVelocity.Length());
            GameConsole.WriteLine("slip", Wheels[0].LatSlip);
        }
    

        public void Brake(float value)
        {
            Motor.Throttle = -value;
            Motor.Update(Speed);
            float motorTorque = Motor.CurrentPowerOutput;
            _currentTorque = motorTorque;

            if (Backwards)
            {              
                _motorTorque = motorTorque;
                _brakeTorque = 0f;
            }
            else
            {
                _motorTorque = 0.0f;
                _brakeTorque = Math.Max(700, _brakeTorque + 0.7f);
            }
            UpdateTorque();
            _physXActor.WakeUp();
        }

        public void Accelerate(float value)
        {
            Motor.Throttle = value;
            Motor.Update(Speed);
            float motorTorque = Motor.CurrentPowerOutput;                       
            _currentTorque = motorTorque;

            if (motorTorque > 0.0001f)
            {
                if (Backwards)
                {
                    _motorTorque = 0f;
                    _brakeTorque = Math.Max(700, _brakeTorque + 0.7f);
                }
                else
                {
                    _motorTorque = motorTorque;
                    _brakeTorque = 0.0f;
                }
            }
            else
            {
                _motorTorque = 0.0f;
                _brakeTorque = Motor.CurrentFriction;
            }

            UpdateTorque();
            _physXActor.WakeUp();
        }

        public void Steer(float angle)
        {
            _desiredSteerAngle = angle*0.33f;
        }

        public void PullHandbrake()
        {
            _handbrake = 1;
        }

        public void ReleaseHandbrake()
        {
            if (_handbrake == 0) return;
            _handbrake -= Engine.ElapsedSeconds*0.5f;
            if (_handbrake < 0) _handbrake = 0;
        }


        private void UpdateTorque()
        {
            foreach (VehicleWheel wheel in Wheels)
            {
                if (wheel.IsRear)
                    wheel.ApplyHandbrake(_handbrake);
                if (wheel.CActor.IsDriven)
                    wheel.Shape.MotorTorque = -_motorTorque;
            }

            if (_handbrake == 1) return;

            
            foreach (VehicleWheel wheel in Wheels)
            {
                wheel.Shape.BrakeTorque = _brakeTorque;
                
            }
        }


        public void Reset()
        {
            Matrix m = _physXActor.GlobalOrientation;
            m.Up = Vector3.Up;
            _physXActor.GlobalOrientation = m;
            _physXActor.GlobalPosition += new Vector3(0.0f, 2.0f, 0.0f);
            _physXActor.LinearMomentum = _physXActor.LinearVelocity = Vector3.Zero;
            _physXActor.AngularMomentum = _physXActor.AngularVelocity = Vector3.Zero;
        }

        //public void Explode(float strength)
        //{
        //    _body.AddForceAtLocalPosition(new Vector3(
        //        RandomGenerator.NextFloat() * 200f - 100f,
        //        155f + RandomGenerator.NextFloat() * 40f,
        //        RandomGenerator.NextFloat() * 200f - 100f
        //    ) * 100f * strength,
        //    new Vector3(RandomGenerator.NextFloat() - .5f, 0.1f, RandomGenerator.NextFloat() * 2f - 1f), ForceMode.Impulse, true);
        //}

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

        internal void Boost()
        {
            _physXActor.AddLocalForce(Vector3.Forward * 1000, ForceMode.Force);
        }

        internal void SetLateralFrictionMultiplier(float p)
        {
            foreach (VehicleWheel wheel in Wheels)
            {
                TireFunctionDescription tfd = wheel.Shape.LateralTireForceFunction;
                tfd.ExtremumValue *= p;
                tfd.AsymptoteSlip *= p;
                wheel.Shape.LateralTireForceFunction = tfd;
            }
            _lateralFrictionMultiplier = p;
        }
    }
}

