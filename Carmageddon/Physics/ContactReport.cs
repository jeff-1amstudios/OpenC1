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
                if (events == ContactPairFlag.OnEndTouch)
                {
                }
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
                                    HandleNonCarCollision(vehicle, (NonCar)contactInfo.ActorA.UserData, pos, iter.GetPatchNormal(), contactInfo.NormalForce, events);
                                    return;
                                }

                                float force = contactInfo.NormalForce.Length(); // iter.GetPointNormalForce();
                                if (force == 0) continue;

                                //GameConsole.WriteEvent(force.ToString());   
                                
                                if (force > 0)
                                {
                                    if (contactInfo.ActorA.Group == PhysXConsts.VehicleId)
                                    {
                                        Vehicle vehicle2 = (Vehicle)contactInfo.ActorA.UserData;
                                        //2 vehicle collision
                                        force = iter.GetPointNormalForce();
                                        Vector3 normal = contactInfo.NormalForce;
                                        normal.Normalize();
                                        if (vehicle.Driver is CpuDriver && vehicle2.Driver is PlayerDriver)
                                        {
                                            ((CpuDriver)vehicle.Driver).OnPlayerHit(force);
                                        }
                                        else if (vehicle2.Driver is CpuDriver && vehicle.Driver is PlayerDriver)
                                        {
                                            ((CpuDriver)vehicle2.Driver).OnPlayerHit(force);
                                        }

                                        if (vehicle.Chassis.Wheels.Find(a => !a.IsRear && a.InAir) != null)
                                        {
                                            vehicle.Chassis.Actor.AddForceAtPosition(Vector3.Down * 0.35f * force, pos, ForceMode.Force);
                                        }
                                        if (vehicle2.Chassis.Wheels.Find(a => !a.IsRear && a.InAir) != null)
                                        {
                                            vehicle2.Chassis.Actor.AddForceAtPosition(Vector3.Down * 0.35f * force, pos, ForceMode.Force);
                                        }

                                        vehicle.OnCollision(force * 2, pos, normal, events);
                                        vehicle2.OnCollision(force * 2, pos, normal, events);
                                    }
                                    else
                                    {
                                        Vector3 normal = contactInfo.NormalForce;
                                        normal.Normalize();
                                        if (iter.GetPointNormalForce() != 0)
                                            force = iter.GetPointNormalForce();
                                        
                                        vehicle.OnCollision(force, pos, iter.GetPatchNormal(), events);
                                        //return;
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

        private void HandleNonCarCollision(Vehicle vehicle, NonCar nonCar, Vector3 pos, Vector3 patchnormal, Vector3 normalforce, ContactPairFlag events)
        {
            if (nonCar.IsAttached)
            {
                if (normalforce == Vector3.Zero) return;

                nonCar.Hit = true;

                float speed = vehicle.Chassis.LastSpeeds.GetMax();

                if (speed < 20) return;

                if (nonCar.LastTouchTime + 0.3f > Engine.TotalSeconds)
                    return;

                normalforce = Vector3.Normalize(normalforce);

                Vector3 directedForce = normalforce * speed * vehicle.Config.Mass;
                float factor = (1 / (nonCar.Config.TorqueRequiredToMove * nonCar.Config.MassWhenAttached)) * 0.005f;

                nonCar.Rotation.X += -directedForce.X * factor;
                nonCar.Rotation.Z += directedForce.Z * factor;

                nonCar.Hit = true;
                nonCar.LastTouchTime = Engine.TotalSeconds;

                //multiply force by 8.5 to match the force generated by hitting a solid wall/barrier etc
                vehicle.OnCollision(directedForce.Length(), pos, normalforce, events);
                return;
            }

            //multiply force by 8.5 to match the force generated by hitting a solid wall/barrier etc
            vehicle.OnCollision(normalforce.Length(), pos, normalforce, events);
        }

        //private void HandleVehicleOnVehicleCollision(Vehicle v1, Vehicle v2, float force, Vector3 position, Vector3 normal, ContactPairFlag events)
        //{
        //    if (v1.Driver is CpuDriver && v2.Driver is PlayerDriver)
        //        ((CpuDriver)v1.Driver)._state = CpuDriverState.Attacking;
        //    else if (v2.Driver is CpuDriver && v1.Driver is PlayerDriver)
        //        ((CpuDriver)v2.Driver)._state = CpuDriverState.Attacking;

        //    v1.ContactReport_Collision(force * 2, position, normal, events);
        //    v2.ContactReport_Collision(force * 2, position, normal, events);

        //    //GameConsole.WriteEvent("caroncar");
        //    //if (v1.Chassis.Speed > 3 || v2.Chassis.Speed > 3)
        //    //{
        //      //  GameVariables.SparksEmitter.DumpParticles(position, 6);
        //      //  SoundCache.PlayCrash(v1, force);
        //    //}

        //    //float product = Math.Abs(Vector3.Dot(Chassis.Actor.GlobalPose.Forward, normal));
        //    //if (product < 0.3f)
        //    //{
        //    //    SoundCache.PlayScrape(this);
        //    //}
        //    //else if (force > 200)
        //    //    SoundCache.PlayCrash(this);
        //}
    }
}

