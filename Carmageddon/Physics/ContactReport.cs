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
            GameVars.SparksEmitter = new ParticleEmitter(new SparksParticleSystem(), 5, Vector3.Zero);
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

                                        vehicle.OnCollision(force * 2, pos, normal, true);
                                        vehicle2.OnCollision(force * 2, pos, normal, true);
                                    }
                                    else
                                    {
                                        Vector3 normal = contactInfo.NormalForce;
                                        normal.Normalize();
                                        if (iter.GetPointNormalForce() != 0)
                                            force = iter.GetPointNormalForce();

                                        vehicle.OnCollision(force, pos, iter.GetPatchNormal(), true);
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
                    if (contactInfo.ActorB.LinearVelocity.Length() > 4)
                    {
                        while (iter.GoToNextPair())
                        {
                            while (iter.GoToNextPatch())
                            {
                                while (iter.GoToNextPoint())
                                {
                                    Vector3 pos = iter.GetPoint();

                                    GameVars.SparksEmitter.Update(pos);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void HandleNonCarCollision(Vehicle vehicle, NonCar nonCar, Vector3 pos, Vector3 patchnormal, Vector3 normalforce, ContactPairFlag events)
        {
            if (nonCar.LastTouchTime + 0.1f > Engine.TotalSeconds)
            {
                nonCar.LastTouchTime = Engine.TotalSeconds;
                return;
            }

            if (nonCar.IsAttached)
            {
                if (normalforce == Vector3.Zero) return;

                nonCar.Hit = true;

                float speed = vehicle.Chassis.LastSpeeds.GetMax();

                if (speed < 20) return;

                normalforce = Vector3.Normalize(normalforce);

                Vector3 directedForce = normalforce * speed * vehicle.Config.Mass;
                float factor = (1 / (nonCar.Config.TorqueRequiredToMove * nonCar.Config.MassWhenAttached)) * 0.005f;

                nonCar.Rotation.X += -directedForce.X * factor;
                nonCar.Rotation.Z += directedForce.Z * factor;

                nonCar.Hit = true;
                nonCar.LastTouchTime = Engine.TotalSeconds;
                nonCar.LastForcePosition = pos;
                nonCar.LastForce = directedForce;

                //multiply force to match the force generated by hitting a solid wall/barrier etc
                vehicle.OnCollision(directedForce.Length() * 15, pos, normalforce, true);
            }
            else
            {
                vehicle.OnCollision(normalforce.Length(), pos, normalforce, false);
            }

            nonCar.LastTouchTime = Engine.TotalSeconds;            
        }
    }
}

