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
            
            using (ContactStreamIterator iter = new ContactStreamIterator(contactInfo.ContactStream))
            {
                //if we are looking at the player car
                if (contactInfo.ActorB.Group == PhysXConsts.VehicleId)
                {
                    while (iter.GoToNextPair())
                    {
                        while (iter.GoToNextPatch())
                        {
                            while (iter.GoToNextPoint())
                            {
                                Shape shapeA = iter.GetShapeA();
                                Shape shapeB = iter.GetShapeB();
                                if (!(shapeB is WheelShape))
                                {
                                    Vector3 pos = iter.GetPoint();
                                    float force = contactInfo.NormalForce.Length();
                                    if (force > 0)
                                    {
                                        GameVariables.SparksEmitter.Update(pos);
                                        if (force > 850000) GameVariables.SparksEmitter.DumpParticles(pos, 6);
                                        Vehicle vehicle = (Vehicle)contactInfo.ActorB.UserData;
                                        vehicle.ContactReport_Collision(force, pos, iter.GetPatchNormal());

                                        if (contactInfo.ActorA.Group == PhysXConsts.VehicleId)
                                        {
                                            Vehicle vehicle2 = (Vehicle)contactInfo.ActorA.UserData;
                                            vehicle2.ContactReport_Collision(force, pos, iter.GetPatchNormal());
                                        }
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
                                    //GameConsole.WriteEvent("noncar collision");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

