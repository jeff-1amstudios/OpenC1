using System;
using System.Collections.Generic;
using System.Text;
using StillDesign.PhysX;
using NFSEngine;

namespace Carmageddon.Physics
{
    class TriggerReport : UserTriggerReport
    {
        private static TriggerReport _instance;
        public static TriggerReport Instance
        {
            get
            {
                if (_instance == null) _instance = new TriggerReport();
                return _instance;
            }
        }

        public override void OnTrigger(Shape triggerShape, Shape otherShape, TriggerFlag status)
        {
            if (otherShape.Name != PhysXConsts.VehicleBody) return;

            Vehicle vehicle = (Vehicle)otherShape.Actor.UserData;
            
            if (triggerShape.Actor.UserData is Checkpoint)
            {
                if (vehicle.Driver is PlayerDriver)
                {
                    Checkpoint checkpoint = (Checkpoint)triggerShape.Actor.UserData;
                    Race.Current.OnCheckpointHit(checkpoint);
                }
            }
            else if (triggerShape.Actor.UserData is SpecialVolume)
            {
                if (status == TriggerFlag.OnEnter)
                {
                    Race.Current.OnVehicleEnterSpecVol((SpecialVolume)triggerShape.Actor.UserData, vehicle);
                }
                else if (status == TriggerFlag.OnLeave)
                {
                    Race.Current.OnVehicleExitSpecVol((SpecialVolume)triggerShape.Actor.UserData, vehicle);
                }
            }
        }
    }
}
