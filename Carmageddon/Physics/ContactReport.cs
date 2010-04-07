using StillDesign.PhysX;
using System;
using Particle3DSample;
using Carmageddon.Gfx;
using Microsoft.Xna.Framework;
using PlatformEngine;
using NFSEngine;

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
            
            //using (ContactStreamIterator iter = new ContactStreamIterator(contactInfo.ContactStream))
            //{
            //    //if we are looking at the player car
            //    if (contactInfo.ActorB.Group == PhysXConsts.VehicleId)
            //    {
            //        while (iter.GoToNextPair())
            //        {
            //            while (iter.GoToNextPatch())
            //            {
            //                while (iter.GoToNextPoint())
            //                {
            //                    Shape shapeB = iter.GetShapeB();
            //                    if (contactInfo.ActorA.Group == PhysXConsts.TrackId && shapeB is WheelShape)
            //                        continue; //we dont want to know each time a wheel is touching the ground...

            //                    Vector3 pos = iter.GetPoint();
            //                    float force = contactInfo.NormalForce.Length();
            //                    if (force > 0)
            //                    {
            //                        GameVariables.SparksEmitter.Update(pos);
                                                                       

            //                        if (contactInfo.ActorA.Group == PhysXConsts.VehicleId)
            //                        {
            //                            //2 vehicle collision
            //                            HandleVehicleOnVehicleCollision((Vehicle)contactInfo.ActorA.UserData, (Vehicle)contactInfo.ActorB.UserData, force, pos);
            //                            return;
            //                        }
            //                        else
            //                        {
            //                            Vehicle vehicle = (Vehicle)contactInfo.ActorB.UserData;
            //                            vehicle.ContactReport_Collision(force, pos, iter.GetPatchNormal());
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    // a non-car object sliding along the track
            //    else if (contactInfo.ActorB.Group == PhysXConsts.NonCarId && contactInfo.ActorA.Group == PhysXConsts.TrackId)
            //    {
            //        if (contactInfo.ActorB.LinearVelocity.Length() > 2)
            //        {
            //            while (iter.GoToNextPair())
            //            {
            //                while (iter.GoToNextPatch())
            //                {
            //                    while (iter.GoToNextPoint())
            //                    {
            //                        Vector3 pos = iter.GetPoint();

            //                        GameVariables.SparksEmitter.Update(pos);
            //                        //GameConsole.WriteEvent("noncar collision");
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
        }

        private void HandleVehicleOnVehicleCollision(Vehicle v1, Vehicle v2, float force, Vector3 position)
        {
            //GameConsole.WriteEvent("caroncar");
            //if (v1.Chassis.Speed > 3 || v2.Chassis.Speed > 3)
            //{
                GameVariables.SparksEmitter.DumpParticles(position, 6);
                SoundCache.PlayCrash(v1);
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

