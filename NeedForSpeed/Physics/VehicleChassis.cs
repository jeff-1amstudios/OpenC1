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

        public Motor Motor { get; private set; }

       
        Vector3 vSpeed = Vector3.Zero;

        // sonstige
        float _speed = 0.0f;
        
        Vector3 DOWNFORCE = new Vector3(0.0f, -20000.0f, 0.0f); // To keep the car from flipping over too easily
        Vector3 ROCKETIMPACT = new Vector3(0, -900, 0); // Impact of fireing a rocket

        TireFunctionDescription LS_latTFD;

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
            lngTFD.ExtremumSlip = 1.0f;
            lngTFD.ExtremumValue = 20000f;
            lngTFD.AsymptoteSlip = 2.0f;
            lngTFD.AsymptoteValue = 10000f;
            lngTFD.StiffnessFactor = 2.5f;

            // ----------------------
            TireFunctionDescription latTfd = new TireFunctionDescription();
            latTfd.ExtremumSlip = 1.0f;
            latTfd.ExtremumValue = 1250f;
            latTfd.AsymptoteSlip = 1.01f; // 2.0f;
            latTfd.AsymptoteValue = 1249f; // 625f;
            latTfd.StiffnessFactor = 10.0f; // war 60000.0f

            LS_latTFD = latTfd;

            
            WheelShapeDescription wheelDesc = new WheelShapeDescription();
            wheelDesc.Radius = properties.NonDrivenWheelRadius;
            wheelDesc.SuspensionTravel = 0.16f;
            wheelDesc.InverseWheelMass = 0.1f;
            wheelDesc.LongitudalTireForceFunction = lngTFD;
            wheelDesc.LateralTireForceFunction = LS_latTFD;
            

            MaterialDescription md = new MaterialDescription();
            md.Restitution = 0.3f;
            md.DynamicFriction = 0.0f;
            md.StaticFriction = 0.5f;
            md.Flags = MaterialFlag.DisableFriction;
            Material m = scene.CreateMaterial(md);
            wheelDesc.Material = m;

            SpringDescription spring = new SpringDescription(10000, properties.SuspensionDamping, 0);
            //float heightModifier = (suspensionSettings.WheelSuspension + wheelDesc.Radius) / suspensionSettings.WheelSuspension;
            //spring.SpringCoefficient = suspensionSettings.SpringRestitution * heightModifier;
            //spring.DamperCoefficient = suspensionSettings.SpringDamping * heightModifier;
            //spring.TargetValue = suspensionSettings.SpringBias * heightModifier;
            wheelDesc.Suspension = spring;

            
            // front left
            wheelDesc.LocalPosition = properties.WheelPositions[0];
            FLWheel = (WheelShape)VehicleBody.CreateShape(wheelDesc);
            FLWheel.Name = "FL-Wheel";
            
            // front right
            wheelDesc.LocalPosition = properties.WheelPositions[1];
            FRWheel = (WheelShape)VehicleBody.CreateShape(wheelDesc);
            FRWheel.Name = "FR-Wheel";

            wheelDesc.Radius = properties.DrivenWheelRadius;
            TireFunctionDescription tfd = wheelDesc.LateralTireForceFunction;
            tfd.StiffnessFactor = 12;
            wheelDesc.LateralTireForceFunction = tfd;

            // back left
            wheelDesc.LocalPosition = properties.WheelPositions[2];
            RLWheel = (WheelShape)VehicleBody.CreateShape(wheelDesc);
            RLWheel.SteeringAngle = 0.0f;
            RLWheel.Name = "RL-Wheel";

            // back right
            wheelDesc.LocalPosition = properties.WheelPositions[3];
            RRWheel = (WheelShape)VehicleBody.CreateShape(wheelDesc);
            RRWheel.SteeringAngle = 0.0f;
            RRWheel.Name = "RR-Wheel";

            Wheels.Add(new VehicleWheel(this, FLWheel, 0.17f));
            Wheels.Add(new VehicleWheel(this, FRWheel, -0.17f));
            Wheels.Add(new VehicleWheel(this, RLWheel, 0.17f));
            Wheels.Add(new VehicleWheel(this, RRWheel, -0.17f));

            Vector3 massPos = VehicleBody.CenterOfMassLocalPosition;
            massPos = properties.CenterOfMass;// new Vector3(0, 0, -0.6f); // VehicleBody.CenterOfMassLocalPosition;
            massPos.Y = ((properties.WheelPositions[0].Y + properties.WheelPositions[2].Y) / 2) -0.2f;
            //massPos.Y = ((properties.WheelPositions[0].Y + properties.WheelPositions[2].Y) / 2) -0.19f;
            //massPos.Z *= 1f;
            _centerOfMass = massPos;
            VehicleBody.SetCenterOfMassOffsetLocalPosition(massPos);
            massPos = VehicleBody.CenterOfMassGlobalPosition;
            //massPos.Y = -15;// 0.5f;
            //VehicleBody.SetCenterOfMassOffsetGlobalPosition(massPos);
            
            
            VehicleBody.WakeUp(60.0f);

            VehicleBody.RaiseBodyFlag(BodyFlag.Visualization);

            List<float> power = new List<float>(new float[] { 0.2f, 0.3f, 0.4f, 0.7f, 0.8f, 1.0f, 1.0f, 1.0f, 0 });
            List<float> ratios = new List<float>(new float[] { 3.227f, 2.360f, 1.885f, 1.512f, 1.200f });

            BaseGearbox gearbox = BaseGearbox.Create(false, ratios, 0.4f);
            Motor = new Motor(power, 480, 7f, gearbox);
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
            vSpeed = VehicleBody.LinearVelocity;
            Vector3 vDirection = VehicleBody.GlobalOrientation.Forward;
            Vector3 vNormal = vSpeed * vDirection;
            _speed = vNormal.Length() * 2f;

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
                Reset();

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

            // NPS (non-player stabilizer)
            //Vector3 v2 = VehicleBody.GlobalOrientation.Up;
            //v2.Z = 0;
            //v2.Normalize();
            //Vector3 v1 = new Vector3(0, 1, 0);
            //_sideTilt = MathHelper.ToDegrees((float)Math.Acos(Vector3.Dot(v1, v2)));

            //if ((_sideTilt > 15)) // && (_speed > 60))
            //{
            //    BRWheel.LateralTireForceFunction = HS_latTFD;
            //    BLWheel.LateralTireForceFunction = HS_latTFD;

            //    GameConsole.WriteLine("slide");
            //    if (!ax)
            //    {
            //        Vector3 massPos = VehicleBody.CenterOfMassLocalPosition;
            //        massPos.Y -= 0.40f;
            //        //VehicleBody.SetCenterOfMassOffsetLocalPosition(massPos);
            //    }
            //    ax = true;
            //}

            //else
            //{
            //    if (ax)
            //    {
            //        Vector3 massPos = VehicleBody.CenterOfMassLocalPosition;
            //        massPos.Y += 0.40f;
            //        //VehicleBody.SetCenterOfMassOffsetLocalPosition(massPos);
            //    }
            //    BRWheel.LateralTireForceFunction = LS_latTFD;
            //    BLWheel.LateralTireForceFunction = LS_latTFD;
            //    ax = false;
            //}

            //LS_latTFD.StiffnessFactor = speed * 150000.0f;
            //GameConsole.WriteLine(LS_latTFD.StiffnessFactor);
            UpdateTireStiffness();

            GameConsole.WriteLine("MotorRpm", Motor.Rpm);
            GameConsole.WriteLine("MotorGear", Motor.Gearbox.CurrentGear);

            //PlatformEngine.Engine.Instance.GraphicsUtils.AddSolidShape(PlatformEngine.ShapeType.Cube, VehicleBody.CenterOfMassGlobalPose, Color.Yellow, null);
            
        }

        private void UpdateTireStiffness()
        {
            //Vector3 com = _centerOfMass;
            //float val = Math.Min(_speed * 0.02f, 0.53f);
            //com.Y -= val;
            //GameConsole.WriteLine("Center of mass: " + val);
            //VehicleBody.SetCenterOfMassOffsetLocalPosition(com);

            Vector3 vNormal = VehicleBody.LinearVelocity * VehicleBody.GlobalOrientation.Left;
            float lateralSpeed = vNormal.Length();
            GameConsole.WriteLine("Lat Speed", lateralSpeed);
            GameConsole.WriteLine("Handbrake", _handbrake);
            
            //float val = Math.Min(0.70f + _speed * 0.0030f, 0.96f);
            
            float angVel = 12;

            // Lerp between handbrake on and off
            //if (_handbrake > 0)
            //    angVel = MathHelper.Lerp(Math.Max(0, 6 - (Math.Abs(VehicleBody.AngularVelocity.Y * 2.4f) * (_speed * 0.08f))), 0.5f, _handbrake);
            //else
                angVel = Math.Max(0, 11 - (Math.Abs(VehicleBody.AngularVelocity.Y * 2.4f) * (_speed * 0.08f)));
            

            Wheels[0].Update(angVel);
            Wheels[1].Update(angVel);

            angVel = MathHelper.Lerp(12, 0.12f, _handbrake);
            Wheels[2].Update(angVel);
            Wheels[3].Update(angVel);

            LS_latTFD.StiffnessFactor = Wheels[0].LateralStiffness;
            FLWheel.LateralTireForceFunction = LS_latTFD;
            LS_latTFD.StiffnessFactor = Wheels[1].LateralStiffness;
            FRWheel.LateralTireForceFunction = LS_latTFD;
            LS_latTFD.StiffnessFactor = Wheels[2].LateralStiffness;
            RLWheel.LateralTireForceFunction = LS_latTFD;
            LS_latTFD.StiffnessFactor = Wheels[3].LateralStiffness;
            RRWheel.LateralTireForceFunction = LS_latTFD;
            
            GameConsole.WriteLine("Tire", LS_latTFD.StiffnessFactor);
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
                    _brakeTorque = -torque;
                }
            }

            else
            {
                _motorTorque = 0.0f;
                _brakeTorque = Motor.CurrentFriction * 0.6f;
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
            if (_handbrake == 1) return;
            if (_handbrake == 0) _handbrake = 0.85f;
            _handbrake = 1;
            if (_handbrake > 1) _handbrake = 1;
        }

        public void ReleaseHandbrake()
        {
            if (_handbrake == 0) return;
            _handbrake -= Engine.Instance.ElapsedSeconds*0.2f;
            if (_handbrake < 0) _handbrake = 0;
        }

        #endregion

        #region Helper

        private void UpdateTorque()
        {
            if (_handbrake > 0.8f)
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
            VehicleBody.AddLocalForceAtLocalPosition(ROCKETIMPACT, new Vector3(1, 0, .5f), ForceMode.Impulse);
        }
    }
}
