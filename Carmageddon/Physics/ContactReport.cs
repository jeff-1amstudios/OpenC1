using StillDesign.PhysX;
using System;
using Particle3DSample;
using Carmageddon.Gfx;
using Microsoft.Xna.Framework;
using PlatformEngine;
using NFSEngine;
using Carmageddon.Parsers;

namespace Carmageddon.Physics
{
    internal class ContactReport : UserContactReport
    {
        private static ContactReport _instance;
        public static ContactReport Instance
        {
            get
            {
                if (_instance == null) _instance = new ContactReport();
                return _instance;
            }
        }

        private ContactReport()
            : base()
        {
            GameVariables.SparksEmitter = new ParticleEmitter(new SparksParticleSystem(), 10, Vector3.Zero);
        }

        public override void OnContactNotify(ContactPair contactInfo, ContactPairFlag events)
        {
            
            using (ContactStreamIterator iter = new ContactStreamIterator(contactInfo.ContactStream))
            {
                
                //if we are looking at the player car
                if (contactInfo.ActorB.Group == PhysXConsts.VehicleId)
                {
                    Vehicle vehicle = (Vehicle)contactInfo.ActorB.UserData;

                    while (iter.GoToNextPair())
                    {
                        while (iter.GoToNextPatch())
                        {
                            while (iter.GoToNextPoint())
                            {
                                if (contactInfo.ActorA.Group == PhysXConsts.TrackId && iter.GetShapeB() is WheelShape)
                                    continue; //we dont want to know each time a wheel is touching the ground

                                Vector3 pos = iter.GetPoint();                                
                                
                                if (contactInfo.ActorA.UserData is NonCar)
                                {
                                    HandleNonCarCollision(vehicle, (NonCar)contactInfo.ActorA.UserData, pos, iter.GetPatchNormal(), contactInfo.NormalForce);
                                    return;
                                }

                                float force = iter.GetPointNormalForce();
                                if (force == 0) continue;

                                //GameConsole.WriteEvent(force.ToString());   
                                
                                if (force > 0)
                                {
                                    if (contactInfo.ActorA.Group == PhysXConsts.VehicleId)
                                    {
                                        //2 vehicle collision
                                        HandleVehicleOnVehicleCollision((Vehicle)contactInfo.ActorA.UserData, vehicle, force, pos);
                                        return;
                                    }
                                    else
                                    {
                                        vehicle.ContactReport_Collision(force, pos, iter.GetPatchNormal());
                                    }
                                }
                            }
                        }
                    }
                }

                // a non-car object sliding along the track
                else if (contactInfo.ActorB.Group == PhysXConsts.NonCarId && contactInfo.ActorA.Group == PhysXConsts.TrackId)
                {
                    if (contactInfo.ActorB.LinearVelocity.Length() > 2)
                    {
                        while (iter.GoToNextPair())
                        {
                            while (iter.GoToNextPatch())
                            {
                                while (iter.GoToNextPoint())
                                {
                                    Vector3 pos = iter.GetPoint();

                                    GameVariables.SparksEmitter.Update(pos);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void HandleNonCarCollision(Vehicle vehicle, NonCar nonCar, Vector3 pos, Vector3 patchnormal, Vector3 normalforce)
        {
            if (nonCar.IsAttached)
            {

                if (normalforce == Vector3.Zero) return;


                float forcelen = normalforce.Length();

                if (vehicle.Chassis.Speed < 2) return;

                //if (nonCar.LastTouchTime + 0.3f > Engine.TotalSeconds)
                //    return;
                //Vector3 directedForce = normalforce;
                //float factor = 0;
                //if (vehicle.Chassis.Speed > 10) directedForce *= vehicle.Chassis.LastSpeed; // force* normal;

                float product = Math.Abs(Vector3.Dot(vehicle.Chassis.Actor.GlobalPose.Forward, patchnormal));
                if (product < 0.1f)
                {
                    //return;
                }
                
                //Vector3 lin = vehicle.Chassis.Actor.LinearMomentum;
                //float dot2 = Vector3.Dot(vehicle.Chassis.Actor.GlobalPose.Forward, lin);
                //if (dot2 
                //normalforce.Normalize();
                //if (dot2 < 0 || product < 0.5f)

                Vector3 directedForce = normalforce; // *vehicle.Chassis.LastSpeed * vehicle.Config.Mass;
                    //float factor = (1 / (nonCar.Config.TorqueRequiredToMove * nonCar.Config.MassWhenAttached)) * 0.004f;

                    float factor = (1 / (nonCar.Config.TorqueRequiredToMove * nonCar.Config.MassWhenAttached)) * 0.0003f;
                //else
                //{
                //    directedForce = vehicle.Chassis.LastLinearMomentum * product;
                //    factor = (1 / (nonCar.Config.TorqueRequiredToMove * nonCar.Config.MassWhenAttached)) * 0.09f;
                //}
                GameConsole.WriteEvent("spd: " + Math.Round(forcelen,2));

                nonCar.Rotation.X += -directedForce.X * factor;
                nonCar.Rotation.Z += directedForce.Z * factor;
                                
                Matrix orientation = Matrix.CreateRotationX(nonCar.Rotation.Z) * Matrix.CreateRotationZ(nonCar.Rotation.X);
                nonCar.NewOrientation = orientation;
                nonCar.Hit = true;
                nonCar.LastTouchTime = Engine.TotalSeconds;
            }

            // multiply force by 8.5 to match the force generated by hitting a solid wall/barrier etc
            //vehicle.ContactReport_Collision(force, pos, normal);
        }

        private void HandleVehicleOnVehicleCollision(Vehicle v1, Vehicle v2, float force, Vector3 position)
        {
            if (v1.Driver is CpuDriver && v2.Driver is PlayerDriver)
                ((CpuDriver)v1.Driver).State = CpuDriverState.Attacking;
            else if (v2.Driver is CpuDriver && v1.Driver is PlayerDriver)
                ((CpuDriver)v2.Driver).State = CpuDriverState.Attacking;

            //GameConsole.WriteEvent("caroncar");
            //if (v1.Chassis.Speed > 3 || v2.Chassis.Speed > 3)
            //{
                GameVariables.SparksEmitter.DumpParticles(position, 6);
                SoundCache.PlayCrash(v1, force);
            //}

            //float product = Math.Abs(Vector3.Dot(Chassis.Actor.GlobalPose.Forward, normal));
            //if (product < 0.3f)
            //{
            //    SoundCache.PlayScrape(this);
            //}
            //else if (force > 200)
            //    SoundCache.PlayCrash(this);
        }
    }
}

