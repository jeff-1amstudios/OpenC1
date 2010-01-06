using StillDesign.PhysX;
using System;
using Particle3DSample;
using Carmageddon.Gfx;
using Microsoft.Xna.Framework;
using PlatformEngine;

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
        public delegate void CollisionHandler(float force, Vector3 position, Vector3 normal);
        public event CollisionHandler PlayerWorldCollision;

        ParticleEmitter _sparkEmitter;

        private ContactReport()
            : base()
        {
            _sparkEmitter = new ParticleEmitter(SparksParticleSystem.Instance, 10, Vector3.Zero);
            _sparkEmitter.Enabled = true;
        }

        public override void OnContactNotify(ContactPair contactInfo, ContactPairFlag events)
        {
            
            using (ContactStreamIterator iter = new ContactStreamIterator(contactInfo.ContactStream))
            {
                //if we are looking at the player car
                if (contactInfo.ActorB.Group == 1)
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
                                        //iter.GetPatchNormal();
                                        _sparkEmitter.Update(pos);
                                        if (force > 850000) _sparkEmitter.DumpParticles(pos, 6);
                                        PlayerWorldCollision(force, pos, iter.GetPatchNormal());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnItemPickup(Actor vehicle, Actor box)
        {
            //int playerId = ((VehicleUserData)vehicle.UserData).playerId;
            //Player player = this.gpScreen.GetPlayer(playerId);
            //if (player != null)
            //{
            //    this.gpScreen.ItemManager.ItemPickup(player, (ItemBox)box.UserData);
            //}
        }

        private void OnMineHit(Actor vehicle, Actor box)
        {
            //int playerId = ((VehicleUserData)vehicle.UserData).playerId;
            //Player player = this.gpScreen.GetPlayer(playerId);
            //if (player != null)
            //{
            //    if (box.Name == "Mine")
            //    {
            //        this.gpScreen.ItemManager.MineHit(player, (Mine)box.UserData);
            //    }
            //    else
            //    {
            //        this.gpScreen.ItemManager.ColorMineHit(player, (ColorMine)box.UserData);
            //    }
            //}
        }

        private void OnRocketCollision(Actor rocket, Actor target)
        {
            //if (rocket.UserData != null)
            //{
            //    int playerId = 0;
            //    if (target.Name == "Vehicle")
            //    {
            //        playerId = ((VehicleUserData)target.UserData).playerId;
            //    }
            //    this.gpScreen.ItemManager.RocketCollision((Rocket)rocket.UserData, playerId);
            //}
        }
    }
}

