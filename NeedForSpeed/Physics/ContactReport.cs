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

            using (ContactStreamIterator iter = new ContactStreamIterator(contactInfo.ContactStream))
            {
                //if we are looking at the player car
                if (actorB.Group == 1)
                {
                    //int pairs = iter.GetNumberOfPairs();
                    //if (pairs < 5)
                    //    return;

                    //while (iter.GoToNextPair())
                    //{
                    //    Shape shapeA = iter.GetShapeA();
                    //    Shape shapeB = iter.GetShapeB();
                    //    if (!(shapeB is WheelShape))
                    //    {
                    //    }
                    //}
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

