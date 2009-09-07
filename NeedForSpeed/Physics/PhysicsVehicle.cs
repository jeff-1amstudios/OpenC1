#region File Description
/* -----------------------------------------------------------------------------
 * Vehicle.cs
 * 
 * SOFTWAREPRAKTIKUM (SS2008)
 * 
 * Projekt:			HexaChrome (Gruppe-05)
 * Programmierer:	Oleg Stobbe
 * -------------------------------------------------------------------------- */
#endregion

#region Using Statements
using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StillDesign.PhysX;
using OneamEngine;
#endregion

namespace Carmageddon.Physics
{
    struct VehicleUserData
    {
        public int playerId;
    }

    class PhysicsVehicle
    {
        #region Fields

        // VehicleTeile
        private Actor VehicleBody;
        private WheelShape FLWheel;
        private WheelShape FRWheel;
        private WheelShape BLWheel;
        private WheelShape BRWheel;
        private bool wheelOnGround; // Cause we only colorize the ground if the car touches it
        private float airTime; // Time spent in air

        float endSteerangle = 0f; // Desired steering angle
        bool backwards = false;
        float currentTorque = 0f;
        // aktuelle Lenkwinkel, Motor- und Bremskraft
        float cSteerangle = 0.0f;
        float cMotortorque = 0.0f;
        float cBraketorque = 0.0f;

        // Buggy Pose
        Matrix mBuggyPose = Matrix.Identity;
        // Globale Positionsmatrizen der Räder
        Matrix mFLWheelGlobalPose = Matrix.Identity;
        Matrix mFRWheelGlobalPose = Matrix.Identity;
        Matrix mBLWheelGlobalPose = Matrix.Identity;
        Matrix mBRWheelGlobalPose = Matrix.Identity;
        Matrix mFRRimGloabalPose = Matrix.Identity;
        Matrix mBRRimGloabalPose = Matrix.Identity;

        // Positionen der Aufhängung
        Matrix sPoseBL = Matrix.Identity;
        Matrix sPoseBR = Matrix.Identity;
        Matrix sPoseFL = Matrix.Identity;
        Matrix sPoseFR = Matrix.Identity;

        // Stossdämpfer
        Matrix mBLSpring = (Matrix.CreateRotationZ(MathHelper.ToRadians(-20)) * Matrix.CreateTranslation(new Vector3(-0.55f, 0.05f, 1.197f)));
        Matrix mBRSpring = (Matrix.CreateRotationZ(MathHelper.ToRadians(20)) * Matrix.CreateTranslation(new Vector3(0.55f, 0.05f, 1.197f)));
        Matrix mFLSpring = (Matrix.CreateRotationZ(MathHelper.ToRadians(-20)) * Matrix.CreateTranslation(new Vector3(-0.5f, 0.05f, -1.473f)));
        Matrix mFRSpring = (Matrix.CreateRotationZ(MathHelper.ToRadians(20)) * Matrix.CreateTranslation(new Vector3(0.5f, 0.05f, -1.473f)));

        Matrix BLSpring;
        Matrix BRSpring;
        Matrix FLSpring;
        Matrix FRSpring;

        float d_left = -0.79f;
        float d_right = 0.79f;
        float dh_front;
        float dh_back;
        // -------------------------------------------------------------

        Matrix rotY180 = Matrix.CreateRotationY(MathHelper.Pi);

        // Wheel rotation (While driving)
        Matrix cAxleSpeed_RightSide = Matrix.Identity;
        Matrix cAxleSpeed_LeftSide = Matrix.Identity;

        // Contact info of the wheels (to ground)
        float cSuspensionLength;
        WheelContactData wcd;
        Matrix mWorld;
        Matrix mTranslation;
        Matrix mRotation;

        Vector3 vSpeed = Vector3.Zero;

        // Wheel-IDs
        const short FL_WHEEL_ID = 1;
        const short FR_WHEEL_ID = 2;
        const short BL_WHEEL_ID = 3;
        const short BR_WHEEL_ID = 4;

        // sonstige
        float speed = 0.0f;
        bool boost = false;

        Vector3 DOWNFORCE = new Vector3(0.0f, -20000.0f, 0.0f); // To keep the car from flipping over too easily
        Vector3 ROCKETIMPACT = new Vector3(0, -900, 0); // Impact of fireing a rocket
        public float sideTilt = 0;

        // We use two different tire descriptions depending on the car's speed
        TireFunctionDescription HS_latTFD;
        TireFunctionDescription LS_latTFD;

        // tmp
        WheelShape cWheel;

        #endregion

        #region Properties

        public Scene Scene
        {
            get { return VehicleBody.Scene; }
        }

        public Matrix VehicleGlobalPose
        {
            get
            {
                return VehicleBody.GlobalPose;
            }
        }


        public Vector3 VehicleGlobalPosition
        {
            get { return VehicleBody.GlobalPosition; }
        }


        public Matrix VehicleGlobalOrientation
        {
            get { return VehicleBody.GlobalOrientation; }
        }


        public float Speed { get { return speed; } }
        public float CurrentMotortorque { get { return cMotortorque; } }
        public Vector3 Velocity
        {
            get { return vSpeed; }
        }

        public bool OnGround
        {
            get { return wheelOnGround; }
        }

        public bool Boost
        {
            get { return boost; }
            set
            {
                boost = value;
                Accelerate(currentTorque);
            }
        }

        public float AirTime
        {
            get
            {
                if (wheelOnGround) return -1f;
                return airTime;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public PhysicsVehicle(Scene scene, Vector3 position, int id)
        {
            // Einstellungen einlesen
            VehicleDimensions vehicleDims;
            try
            {
                // We read the physX dimensions of the car from an XML file for easier finetuning
                vehicleDims = IOHelper.XMLDeserialize<VehicleDimensions>(".\\Settings\\Buggy\\VehicleDimensions.xml");
            }
            catch (Exception e)
            {
                IOHelper.WriteToErrorLog("Could not load Vehicle dimensions: " + e.ToString());
                vehicleDims = new VehicleDimensions();
            }
            //ScreenManager.vehicleDataHash = Hasher.GetMD5HashFromString(vehicleDims.ToString());

            float x;
            float y;
            float z;

            PhysXGroupIDs physxGroupIDs = new PhysXGroupIDs();

            ActorDescription actorDesc = new ActorDescription();
            BodyDescription bodyDesc = new BodyDescription();

            bodyDesc.Mass = vehicleDims.VehicleMass;
            actorDesc.BodyDescription = bodyDesc;

            // untere Collision-Box des Vehicles
            BoxShapeDescription boxDesc = new BoxShapeDescription();
            float w = vehicleDims.Width;
            float h = vehicleDims.Heigth - vehicleDims.dTop;
            float l = vehicleDims.Length;
            boxDesc.Size = new Vector3(w, h, l);
            boxDesc.LocalPosition = new Vector3(0.0f, -(vehicleDims.Heigth - h) / 2, 0.0f);
            actorDesc.Shapes.Add(boxDesc);

            // obere Collision-Box des Vehicles
            w -= vehicleDims.Width / 4;
            h = vehicleDims.dTop;
            l = vehicleDims.Length - (vehicleDims.dFront + vehicleDims.dBack);
            boxDesc = new BoxShapeDescription(w, h, l);

            z = (float)((vehicleDims.dFront + l / 2) - vehicleDims.Length / 2);
            boxDesc.LocalPosition = new Vector3(0.0f, (vehicleDims.Heigth - h) / 2, z);
            actorDesc.Shapes.Add(boxDesc);

            // Actor erstellen
            Vector3 rotation = position;
            rotation.Y = 0;
            rotation.Normalize();
            float rot = (float)Math.Acos(Vector3.Dot(rotation, Vector3.Forward));
            Console.WriteLine("ROT: " + rot);
            if (Math.Acos(Vector3.Dot(rotation, Vector3.Right)) < 0.01) rot += MathHelper.Pi;
            actorDesc.GlobalPose = Matrix.CreateRotationY(rot) * Matrix.CreateTranslation(position);
            VehicleUserData vuData = new VehicleUserData();
            vuData.playerId = id;
            VehicleBody = scene.CreateActor(actorDesc);
            VehicleBody.Name = "Vehicle";
            VehicleBody.Group = physxGroupIDs.VehicleGroupID;
            VehicleBody.UserData = vuData;

            TireFunctionDescription lngTFD = new TireFunctionDescription();
            lngTFD.ExtremumSlip = 1.0f;
            lngTFD.ExtremumValue = 0.02f;
            lngTFD.AsymptoteSlip = 2.0f;
            lngTFD.AsymptoteValue = 0.01f;
            lngTFD.StiffnessFactor = 10000000.0f;

            // ----------------------
            TireFunctionDescription latTfd = new TireFunctionDescription();
            latTfd.ExtremumSlip = 1.0f;
            latTfd.ExtremumValue = 0.01f;
            latTfd.AsymptoteSlip = 2.0f;
            latTfd.AsymptoteValue = 0.008f;
            latTfd.StiffnessFactor = 60000.0f; // war 60000.0f

            LS_latTFD = latTfd; // low speed

            latTfd.ExtremumSlip = 1.0f;
            latTfd.ExtremumValue = 0.02f;
            latTfd.AsymptoteSlip = 2.0f;
            latTfd.AsymptoteValue = 0.01f;
            latTfd.StiffnessFactor = 5.0f; // war 0.0f

            HS_latTFD = latTfd; // high speed
            // -------------------

            // Einstellungen einlesen
            SuspensionSettings suspensionSettings;
            suspensionSettings = IOHelper.XMLDeserialize<SuspensionSettings>(".\\Settings\\Buggy\\SuspensionSettings.xml");

            WheelShapeDescription wheelDesc = new WheelShapeDescription();
            wheelDesc.Radius = vehicleDims.FW_Radius;
            wheelDesc.SuspensionTravel = suspensionSettings.WheelSuspension;
            wheelDesc.InverseWheelMass = vehicleDims.InverseWheelMass;
            wheelDesc.LongitudalTireForceFunction = lngTFD;
            wheelDesc.LateralTireForceFunction = LS_latTFD;

            MaterialDescription md = new MaterialDescription();
            md.Restitution = 0.3f;
            md.DynamicFriction = 0.0f;
            md.StaticFriction = 0.9f;
            Material m = scene.CreateMaterial(md);
            wheelDesc.Material = m;
            //wheelDesc.Material.Flags = MaterialFlag.DisableFriction;

            SpringDescription spring = new SpringDescription();
            float heightModifier = (suspensionSettings.WheelSuspension + wheelDesc.Radius) / suspensionSettings.WheelSuspension;
            spring.SpringCoefficient = suspensionSettings.SpringRestitution * heightModifier;
            spring.DamperCoefficient = suspensionSettings.SpringDamping * heightModifier;
            spring.TargetValue = suspensionSettings.SpringBias * heightModifier;
            wheelDesc.Suspension = spring;

            // wheels
            x = (float)(vehicleDims.Width / 2);
            y = 0.0f;
            z = (float)(vehicleDims.Length / 2);

            Vector3 wheelPosFront = new Vector3((x + vehicleDims.FW_dPosSide), y, -(z - vehicleDims.FW_dPosFront));
            Vector3 wheelPosBack = new Vector3((x + vehicleDims.BW_dPosSide), y, (z - vehicleDims.BW_dPosBack));

            // front left
            wheelDesc.LocalPosition = wheelPosFront;
            FLWheel = (WheelShape)VehicleBody.CreateShape(wheelDesc);
            FLWheel.Name = "FL-Wheel";

            // back left
            wheelDesc.LocalPosition = wheelPosBack;
            BLWheel = (WheelShape)VehicleBody.CreateShape(wheelDesc);
            BLWheel.SteeringAngle = 0.0f;
            BLWheel.Name = "BL-Wheel";

            // front right
            wheelPosFront.X *= -1;
            wheelDesc.LocalPosition = wheelPosFront;
            FRWheel = (WheelShape)VehicleBody.CreateShape(wheelDesc);
            FRWheel.Name = "FR-Wheel";

            // back right
            wheelPosBack.X *= -1;
            wheelDesc.LocalPosition = wheelPosBack;
            BRWheel = (WheelShape)VehicleBody.CreateShape(wheelDesc);
            BRWheel.SteeringAngle = 0.0f;
            BRWheel.Name = "BR-Wheel";

            Vector3 massPos = VehicleBody.CenterOfMassLocalPosition;
            massPos.Y -= 0.75f;
            VehicleBody.SetCenterOfMassOffsetLocalPosition(massPos);
            VehicleBody.AngularDamping = 2.0f;
            VehicleBody.LinearDamping = 0.0005f; // war 0.005f
            VehicleBody.WakeUp(60.0f);

            // --- SuspensionPose ------------------------------
            d_left = -(vehicleDims.Width / 2 - vehicleDims.FW_dPosSide);
            d_right = -d_left;
            dh_back = (FLWheel.SuspensionTravel - vehicleDims.FW_Radius) / 0.5f;
            dh_front = (FLWheel.SuspensionTravel - vehicleDims.FW_Radius) / 0.6f;
            // -------------------------------------------------

            // Matritzen für RM
            //RenderingManager.AddBuggyTransformsMatrix(mBuggyPose);
            //RenderingManager.AddTireTransformsList(mFLWheelGlobalPose);
            //RenderingManager.AddTireTransformsList(mFRWheelGlobalPose);
            //RenderingManager.AddTireTransformsList(mBLWheelGlobalPose);
            //RenderingManager.AddTireTransformsList(mBRWheelGlobalPose);
            //RenderingManager.AddRimTransformsList(mFLWheelGlobalPose);
            //RenderingManager.AddRimTransformsList(mBLWheelGlobalPose);
            //RenderingManager.AddRimTransformsList(mFRRimGloabalPose);
            //RenderingManager.AddRimTransformsList(mBRRimGloabalPose);
            //RenderingManager.AddBSuspensionLeftTransforms(sPoseBL);
            //RenderingManager.AddBSuspensionRightTransforms(sPoseBR);
            //RenderingManager.AddFSuspensionLeftTransforms(sPoseFL);
            //RenderingManager.AddFSuspensionRightTransforms(sPoseFR);

            //BLSpring = Matrix.CreateRotationZ(MathHelper.ToRadians(30));
            BLSpring = mBLSpring;
            BRSpring = mBRSpring;
            FLSpring = mFLSpring;
            FRSpring = mFRSpring;
            //RenderingManager.AddSpringTransforms(BLSpring);
            //RenderingManager.AddSpringTransforms(BRSpring);
            //RenderingManager.AddSpringTransforms(FLSpring);
            //RenderingManager.AddSpringTransforms(FRSpring);



			VehicleBody.RaiseBodyFlag(BodyFlag.Visualization);

            //VehicleBody.ClearBodyFlag(BodyFlag.Visualization);

        }

        public void Delete()
        {
            VehicleBody.Dispose();
            VehicleBody = null;
#if DEBUG
            Console.WriteLine("Auto deleted");
#endif
        }

        #endregion

        #region Update

        public void Update(GameTime gameTime)
        {
            vSpeed = VehicleBody.LinearVelocity;
            Vector3 vDirection = VehicleBody.GlobalOrientation.Forward;
            Vector3 vNormal = vSpeed * vDirection;
            speed = vNormal.Length() * 5f; // Changed to LinearVelocity.. Any disadvantages?
            //if (Math.Sign(vNormal.X) != Math.Sign(vDirection.X) && Math.Sign(vNormal.Z) != Math.Sign(vDirection.Z)) speed = -speed;
            float endLocal = endSteerangle / (1 + speed * 0.02f);
            float diff = Math.Abs(endLocal - cSteerangle);
            if (diff > 0.0025f) // Is the current steering angle ~= desired steering angle?
            { // If not, adjust carefully
                if (diff > 0.4f)
                    diff = 0.05f; // Steps shouldn't be too large
                else
                    diff *= 0.125f;
                // This will make steering independent of framerate
                diff *= (gameTime.ElapsedGameTime.Milliseconds / 16.66667f); // Magic number: duration of a frame @ 60fps;
                if (endLocal > cSteerangle)
                {
                    cSteerangle += diff;
                }
                else
                {
                    cSteerangle -= diff;
                }
                FLWheel.SteeringAngle = cSteerangle;
                FRWheel.SteeringAngle = cSteerangle;
            }
            cAxleSpeed_LeftSide *= Matrix.CreateRotationX(MathHelper.ToRadians(BLWheel.AxleSpeed));
            cAxleSpeed_RightSide *= Matrix.CreateRotationX(MathHelper.ToRadians(BRWheel.AxleSpeed));

            wheelOnGround = false;

            // Positionen Updaten
            mBuggyPose = VehicleBody.GlobalPose;
            mFRWheelGlobalPose = cAxleSpeed_RightSide * UpdateWheelPosition(FR_WHEEL_ID);
            sPoseFR = Matrix.CreateRotationZ((float)Math.Tan((cSuspensionLength - dh_front) / d_right)) * VehicleBody.GlobalPose;
            mBRWheelGlobalPose = cAxleSpeed_RightSide * UpdateWheelPosition(BR_WHEEL_ID);
            sPoseBR = Matrix.CreateRotationZ((float)Math.Tan((cSuspensionLength - dh_back) / d_right)) * VehicleBody.GlobalPose;
            mFLWheelGlobalPose = cAxleSpeed_LeftSide * UpdateWheelPosition(FL_WHEEL_ID);
            sPoseFL = Matrix.CreateRotationZ((float)Math.Tan((cSuspensionLength - dh_front) / d_left)) * VehicleBody.GlobalPose;
            mBLWheelGlobalPose = cAxleSpeed_LeftSide * UpdateWheelPosition(BL_WHEEL_ID);
            sPoseBL = Matrix.CreateRotationZ((float)Math.Tan((cSuspensionLength - dh_back) / d_left)) * VehicleBody.GlobalPose;
            mFRRimGloabalPose = rotY180 * mFRWheelGlobalPose;
            mBRRimGloabalPose = rotY180 * mBRWheelGlobalPose;

            BLSpring = mBLSpring * VehicleBody.GlobalPose;
            BRSpring = mBRSpring * VehicleBody.GlobalPose;
            FLSpring = mFLSpring * VehicleBody.GlobalPose;
            FRSpring = mFRSpring * VehicleBody.GlobalPose;
            // -----------------

            if (wheelOnGround)
                airTime = 0;
            else
                airTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 orientation = VehicleBody.GlobalOrientation.Up;

            if (orientation.Y < 0 && speed < 1f)
                Reset();

            if (speed < 10f) // Change between breaking and accelerating;
            {
                if (backwards && currentTorque > 0.01f)
                {
                    backwards = false;
                    Accelerate(currentTorque);
                }
                else if (!backwards && currentTorque < 0.01f)
                {
                    backwards = true;
                    Accelerate(currentTorque);
                }
            }


            // NPS (non-player stabilizer)
            Vector3 v2 = VehicleBody.GlobalOrientation.Up;//Vector3.Transform(v1, VehicleBody.GlobalOrientation);
            v2.Z = 0;
            v2.Normalize();
            Vector3 v1 = new Vector3(0, 1, 0);
            sideTilt = MathHelper.ToDegrees((float)Math.Acos(Vector3.Dot(v1, v2)));//MathHelper.ToDegrees((float) Math.Acos( Vector3.Dot(v1, v2)));

            if ((sideTilt > 25) && (speed > 60))
            {
                BRWheel.LateralTireForceFunction = HS_latTFD;
                BLWheel.LateralTireForceFunction = HS_latTFD;
            }

            else
            {
                BRWheel.LateralTireForceFunction = LS_latTFD;
                BLWheel.LateralTireForceFunction = LS_latTFD;
            }

            if (!wheelOnGround || boost)
            {
                VehicleBody.AddForce(DOWNFORCE, ForceMode.Force);
            }
        }

        #endregion

        #region HandleInput

        public void Accelerate(float torque)
        {
            //bool forward = VehicleBody.;

            currentTorque = torque;
            if (torque > 0.0001f)
            {
                if (backwards)
                {
                    cMotortorque = 0f;
                    cBraketorque = torque * 300f;
                }
                else
                {
                    cMotortorque = -torque * 300f;
                    if (boost) cMotortorque *= 1.25f;
                    cBraketorque = 0.0f;
                }
            }

            else if (torque < -0.0001f)
            {
                if (backwards)
                {
                    cMotortorque = -torque * 90f;
                    cBraketorque = 0f;
                }
                else
                {
                    cMotortorque = 0.0f;
                    cBraketorque = -torque * 700f;
                }
            }

            else
            {
                cMotortorque = 0.0f;
                cBraketorque = 50f;
            }

            UpdateTorque();
            VehicleBody.WakeUp();
        }

        public void Steer(float angle)
        {
            endSteerangle = -angle; // Just set the desired steering angle, steering is handled in Update()
        }

        #endregion

        #region Helper

        private void UpdateTorque()
        {
            //FLWheel.MotorTorque = cMotortorque;
            //FRWheel.MotorTorque = cMotortorque;
            BLWheel.MotorTorque = cMotortorque;
            BRWheel.MotorTorque = cMotortorque;

            FLWheel.BrakeTorque = cBraketorque;
            FRWheel.BrakeTorque = cBraketorque;
            BLWheel.BrakeTorque = cBraketorque;
            BRWheel.BrakeTorque = cBraketorque;
        }


        /// <summary>
        /// Globale Position/Ausrichtung des Rades berechnen
        /// </summary>
        private Matrix UpdateWheelPosition(short wheelID)
        {
            cWheel = null;
            bool steeringWheel = false;

            if (wheelID == FL_WHEEL_ID)
            {
                cWheel = FLWheel;
                steeringWheel = true;
            }

            else if (wheelID == FR_WHEEL_ID)
            {
                cWheel = FRWheel;
                steeringWheel = true;
            }

            else if (wheelID == BL_WHEEL_ID)
            {
                cWheel = BLWheel;
            }

            else if (wheelID == BR_WHEEL_ID)
            {
                cWheel = BRWheel;
            }

            wcd = cWheel.GetContactData();

            // Federweg, wenn kein Bodenkontakt
            if (wcd.ContactShape == null)
            {
                cSuspensionLength = cWheel.SuspensionTravel;
            }
            else
            {
                wheelOnGround = true;
                cSuspensionLength = wcd.ContactPosition - cWheel.Radius;
            }

            mWorld = cWheel.GlobalPose;
            mTranslation = Matrix.CreateTranslation(0.0f, -cSuspensionLength, 0.0f);

            if (steeringWheel)
            {
                mRotation = Matrix.CreateRotationY(cWheel.SteeringAngle);
                return mRotation * mTranslation * mWorld;
            }

            else
                return mTranslation * mWorld;
        }


        /// <summary>
        /// Vehicle nach dem Überschlag wieder horizontal ausrichten
        /// </summary>
        private void Reset()
        {
            VehicleBody.GlobalOrientation = Matrix.Identity;
            VehicleBody.GlobalPosition += new Vector3(0.0f, 1.0f, 0.0f);
            VehicleBody.LinearMomentum = Vector3.Zero;
            VehicleBody.LinearVelocity = Vector3.Zero;
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

        public void Force(Vector3 vector3)
        {
            vector3.Y += 25f;
            VehicleBody.AddForceAtLocalPosition(vector3, Vector3.Backward, ForceMode.Impulse, true);
        }
    }
}
