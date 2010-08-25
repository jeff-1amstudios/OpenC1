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
        public Motor Motor { get; private set; }
        
        public bool Backwards {get; private set; }
        public float Speed { get; private set; }
        
        public CircularList LastSpeeds = new CircularList(5);
        
        private Actor _physXActor;
        float _currentTorque;
        float _desiredSteerAngle = 0f; // Desired 
        float _handbrake;
        public float _steerAngle = 0.0f;
        float _motorTorque = 0.0f;
        float _brakeTorque = 0.0f;
        TireFunctionDescription _frontLateralTireFn, _rearLateralTireFn;
        public float _heightOffset;
        private Vector3 _massPos;
                
        
        public VehicleChassis(Vehicle vehicle, CActor bodycactor)
        {
            Vehicle = vehicle;

            Wheels = new List<VehicleWheel>();

            CarFile carFile = vehicle.Config;
            
            ActorDescription actorDesc = new ActorDescription();

            actorDesc.BodyDescription = new BodyDescription();
            actorDesc.BodyDescription.Mass = carFile.Mass;
            var boxDesc = new BoxShapeDescription();
            boxDesc.Size = carFile.BoundingBox.GetSize();
            boxDesc.LocalPosition = carFile.BoundingBox.GetCenter();
            boxDesc.Name = PhysXConsts.VehicleBody;
            boxDesc.Flags |= ShapeFlag.PointContactForce;
            actorDesc.Shapes.Add(boxDesc);

            foreach (Vector3 extraPoint in carFile.ExtraBoundingBoxPoints)
            {
                var extraDesc = new SphereShapeDescription(0.2f);
                extraDesc.LocalPosition = extraPoint;
                extraDesc.Mass = 0;
                actorDesc.Shapes.Add(extraDesc);
            }

            using (UtilitiesLibrary lib = new UtilitiesLibrary())
            {
                Vector3 size = carFile.Size;
                Vector3 inertiaTensor = lib.ComputeBoxInteriaTensor(Vector3.Zero, carFile.Mass, size);
                //actorDesc.BodyDescription.MassSpaceInertia = inertiaTensor;
            }

                        
            TireFunctionDescription lngTFD = new TireFunctionDescription();
            lngTFD.ExtremumSlip = 0.1f;
            lngTFD.ExtremumValue = 7f;
            lngTFD.AsymptoteSlip = 2.0f;
            lngTFD.AsymptoteValue = 5.9f;
            
            _rearLateralTireFn = new TireFunctionDescription();            
            
            _rearLateralTireFn.ExtremumSlip = 0.2f;
            _rearLateralTireFn.ExtremumValue = 2.1f;
            _rearLateralTireFn.AsymptoteSlip = 0.0013f * carFile.Mass;
            _rearLateralTireFn.AsymptoteValue = 0.02f;

            _frontLateralTireFn = _rearLateralTireFn;
            _frontLateralTireFn.ExtremumValue = 1.9f;           
            
            MaterialDescription md = new MaterialDescription();
            md.Flags = MaterialFlag.DisableFriction;
            Material m = PhysX.Instance.Scene.CreateMaterial(md);

            int wheelIndex = 0;

            foreach (CWheelActor wheel in carFile.WheelActors)
            {
                WheelShapeDescription wheelDesc = new WheelShapeDescription();
                wheelDesc.InverseWheelMass = 0.08f;
                wheelDesc.LongitudalTireForceFunction = lngTFD;
                wheelDesc.Flags = WheelShapeFlag.ClampedFriction;
                wheelDesc.Material = m;

                wheelDesc.Radius = wheel.IsDriven ? carFile.DrivenWheelRadius : carFile.NonDrivenWheelRadius;
                wheelDesc.SuspensionTravel = (wheel.IsFront ? carFile.SuspensionGiveFront : carFile.SuspensionGiveRear) * 18;
                
                float heightModifier = (wheelDesc.SuspensionTravel + wheelDesc.Radius) / wheelDesc.SuspensionTravel;

                SpringDescription spring = new SpringDescription();
                spring.SpringCoefficient = 5.5f * heightModifier * Math.Min(1000, carFile.Mass);
                spring.DamperCoefficient = carFile.SuspensionDamping * 5.5f;
                
                wheelDesc.Suspension = spring;
                wheelDesc.LocalPosition = wheel.Position;
                wheelDesc.Name = (wheelIndex).ToString();
                wheelIndex++;

                wheelDesc.LateralTireForceFunction = wheel.IsFront ? _frontLateralTireFn : _rearLateralTireFn;
                actorDesc.Shapes.Add(wheelDesc);    
            }
            
            _physXActor = PhysX.Instance.Scene.CreateActor(actorDesc);

            
            _heightOffset = _physXActor.Shapes[0].LocalPosition.Y * -2;
            if (_heightOffset < 0) _heightOffset = 0;
                                    
            foreach (Shape shape in _physXActor.Shapes)
            {
                shape.LocalPosition += new Vector3(0, _heightOffset, 0);
                if (shape is WheelShape)
                {
                    wheelIndex = int.Parse(shape.Name);
                    Wheels.Add(new VehicleWheel(this, carFile.WheelActors[wheelIndex], (WheelShape)shape, carFile.WheelActors[wheelIndex].IsLeft ? 0.17f : -0.17f) { Index = wheelIndex });
                }
            }

            _physXActor.Group = PhysXConsts.VehicleId;
            _physXActor.UserData = vehicle;

            ((CDeformableModel)bodycactor.Model)._actor = _physXActor;
            ((CDeformableModel)bodycactor.Model)._carFile = carFile;
            
            _physXActor.WakeUp(60.0f);

            //_physXActor.RaiseBodyFlag(BodyFlag.DisableGravity);

            //set center of mass
            Vector3 massPos = carFile.CenterOfMass;
            massPos.Y = carFile.WheelActors[0].Position.Y - carFile.NonDrivenWheelRadius + _heightOffset + 0.17f;
            _massPos = massPos;
            _physXActor.SetCenterOfMassOffsetLocalPosition(massPos);
            
            //a real power curve doesnt work too well in carmageddon :)
            List<float> power = new List<float>(new float[] { 0.5f, 0.5f, 0.5f, 1f, 1f, 1.0f, 1.0f, 0 });
            List<float> ratios = new List<float>(new float[] { 3.227f, 2.360f, 1.685f, 1.312f, 1.000f, 0.793f });

            BaseGearbox gearbox = BaseGearbox.Create(false, ratios, 0.4f);
            Motor = new Motor(power, carFile.EnginePower, 6f, carFile.TopSpeed, gearbox);
            Motor.Gearbox.CurrentGear = 0;

        }

        /// <summary>
        /// We know where the wheel should be from car .txt file, and we know where it is now after PhysX 
        /// has taken over.  Move the wheel to where it should be.
        /// </summary>
        public void FixSuspension()
        {
            bool doneBumper = false;

            foreach (VehicleWheel wheel in this.Wheels)
            {
                Vector3 localPos = wheel.Shape.LocalPosition;
                localPos.Y += wheel.CurrentSuspensionTravel;
                wheel.Shape.LocalPosition = localPos;
               
            }
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
            LastSpeeds.Add(Speed);
            //Actor.GlobalOrientation *= Matrix.CreateRotationZ(Engine.ElapsedSeconds);
            GameConsole.WriteLine("Height", Actor.GlobalPosition.Y);
            //LastLinearMomentum = Actor.LinearMomentum;

            //Vector3 lin = Actor.LinearVelocity;
            //float dot2 = Vector3.Dot(Actor.GlobalPose.Forward, lin);
            //dot2 /= lin.Length();
            //GameConsole.WriteEvent("nrml " + Math.Round(dot2, 3));

            Vector3 vDirection = _physXActor.GlobalOrientation.Forward;
            Vector3 vNormal = _physXActor.LinearVelocity * vDirection;
            Speed = vNormal.Length() * 2.9f;

            float endLocal = _desiredSteerAngle;

            float diff = Math.Abs(endLocal - _steerAngle);
            float max = 0.001f;
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


                float steerFactor = Vehicle.Driver.ModerateSteeringAtSpeed ? Math.Min(Math.Max(0.1f, (1 - Speed / 175)), 1) : 1;
                
                foreach (VehicleWheel wheel in Wheels)
                {
                    if (wheel.CActor.IsSteerable)
                        wheel.Shape.SteeringAngle = _steerAngle * steerFactor;
                }
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
            float maxlat = 0;
            bool allWheelsInAir = true;
            foreach (VehicleWheel wheel in Wheels)
            {
                wheel.Update();
                if (!wheel.InAir) allWheelsInAir = false;
                if (wheel.CActor.IsDriven && (wheel.IsSkiddingLng || wheel.InAir))
                {
                    Motor.WheelsSpinning = true;
                }
                if (maxlat < Math.Abs(wheel.LatSlip)) maxlat = Math.Abs(wheel.LatSlip);
                if (Math.Abs(wheel.LatSlip) > 0.8f && !wheel.InAir) isSkiddingTooMuch = true;
            }

            if (!InAir)
            {
                _physXActor.MaximumAngularVelocity = 3.5f;

                if (Speed < 10)
                {
                    _physXActor.LinearDamping = Motor.IsAccelerating ? 1 : 4f;
                    _physXActor.AngularDamping = 0.02f;
                    GameConsole.WriteLine("mode slow");
                }
                else if ((_steerAngle < -0.1f && Wheels[0].LatSlip > 0.35f) || (_steerAngle > 0.1f && Wheels[0].LatSlip < -0.35f))
                {
                    _physXActor.AngularDamping = maxlat * 3.6f;
                    _physXActor.LinearDamping = maxlat * 0.5f;  //stop insane sliding
                    Motor.WheelsSpinning = true;
                    GameConsole.WriteLine("mode alt steer");
                }
                else if ((_steerAngle <= 0f && Wheels[0].LatSlip > 0.4f) || (_steerAngle >= 0f && Wheels[0].LatSlip < -0.4f))
                {
                    _physXActor.AngularDamping = maxlat * 1.4f;
                    _physXActor.LinearDamping = Speed > 20 ? maxlat * 0.5f : maxlat * 0.8f;  //stop insane sliding
                    GameConsole.WriteLine("mode no steer");
                }
                else if (isSkiddingTooMuch)
                {
                    if (Speed < 40)
                        _physXActor.LinearDamping = maxlat * 0.8f;
                    else
                        _physXActor.LinearDamping = maxlat * 0.4f;
                    _physXActor.AngularDamping = 0.01f;
                    GameConsole.WriteLine("mode overskid");
                }
                else
                {
                    _physXActor.LinearDamping = 0;
                    _physXActor.AngularDamping = 0.01f;
                }

                if (_physXActor.GlobalOrientation.Up.Y < 0) //car sliding along on the roof
                {
                    _physXActor.LinearDamping = 4f;  //stop insane sliding
                    _physXActor.AngularDamping = 2f;
                    GameConsole.WriteLine("mode on roof");
                }
            }
            if (allWheelsInAir)
            {
                _physXActor.AngularDamping = 0.00f;
                _physXActor.MaximumAngularVelocity = 10f;
                _physXActor.LinearDamping = 0;
                GameConsole.WriteLine("mode in air");
            }
        }


        public void OutputDebugInfo()
        {
            GameConsole.WriteLine("Speed", Speed);
            GameConsole.WriteLine("Brake", _brakeTorque);
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
                _brakeTorque = Math.Max(750, _brakeTorque + 0.8f);
            }
            UpdateTorque();
            //_physXActor.WakeUp();
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
            //_handbrake += Engine.ElapsedSeconds * 1.5f; // *0.5f;
            //if (_handbrake > 1) _handbrake = 1;
            _handbrake = 1;
        }

        public void ReleaseHandbrake()
        {
            if (_handbrake == 0) return;
            _handbrake -= Engine.ElapsedSeconds * 1.8f;
            if (_handbrake < 0) _handbrake = 0;
        }


        private void UpdateTorque()
        {
            foreach (VehicleWheel wheel in Wheels)
            {
                if (wheel.CActor.IsDriven)
                    wheel.Shape.MotorTorque = -_motorTorque;
                if (wheel.IsRear)
                    wheel.ApplyHandbrake(_handbrake);
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
            //m.Right *= new Vector3(1, 0, 1);
            _physXActor.GlobalOrientation = m;
            _physXActor.GlobalPosition += new Vector3(0.0f, 2.0f, 0.0f);
            _physXActor.LinearMomentum = _physXActor.LinearVelocity = Vector3.Zero;
            _physXActor.AngularMomentum = _physXActor.AngularVelocity = Vector3.Zero;
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
            _physXActor.AddForce(_physXActor.GlobalOrientation.Forward * 1000, ForceMode.Force);
        }
    }
}

