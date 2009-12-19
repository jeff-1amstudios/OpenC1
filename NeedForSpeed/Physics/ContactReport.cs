using StillDesign.PhysX;
using System;

namespace Carmageddon.Physics
{
    internal class ContactReport : UserContactReport
    {
        
        public ContactReport(Scene scene)
            : base()
        {
            
        }
        
        public override void OnContactNotify(ContactPair contactInfo, ContactPairFlag events)
        {
            Actor actorA = contactInfo.ActorA;
            Actor actorB = contactInfo.ActorB;

            if (actorA.Name == "Rocket")
            {
                this.OnRocketCollision(actorA, actorB);
                Console.WriteLine("Rocket Collision detected");
            }
            else if (actorB.Name == "Rocket")
            {
                this.OnRocketCollision(actorB, actorA);
                Console.WriteLine("Rocket Collision detected");
            }
            else if (actorA.Name == "ItemBox")
            {
                this.OnItemPickup(actorB, actorA);
            }
            else if (actorB.Name == "ItemBox")
            {
                this.OnItemPickup(actorA, actorB);
            }
            else if (actorA.Name == "Mine")
            {
                this.OnMineHit(actorB, actorA);
            }
            else if (actorB.Name == "Mine")
            {
                this.OnMineHit(actorA, actorB);
            }
            else if (actorA.Name == "ColorMine")
            {
                this.OnMineHit(actorB, actorA);
            }
            else if (actorB.Name == "ColorMine")
            {
                this.OnMineHit(actorA, actorB);
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

