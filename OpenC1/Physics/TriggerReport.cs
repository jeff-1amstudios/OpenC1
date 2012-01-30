using System;
using System.Collections.Generic;
using System.Text;
using StillDesign.PhysX;
using OneAmEngine;

namespace OpenC1.Physics
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
            if (otherShape.Actor.UserData is Vehicle)
            {
                Vehicle vehicle = (Vehicle)otherShape.Actor.UserData;
                if (otherShape.Name != PhysXConsts.VehicleBody)
                    return;

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
                else if (triggerShape.Actor.UserData is Pedestrian)
                {
                    Race.Current.OnPedestrianHit((Pedestrian)triggerShape.Actor.UserData, vehicle);
                }
            }
            else if (otherShape.Actor.UserData is NonCar)
            {
                NonCar noncar = otherShape.Actor.UserData as NonCar;

                // if the trigger is a noncar that was hit within 7 seconds
                if (triggerShape.Actor.UserData is Pedestrian && noncar.LastTouchTime + 7f > Engine.TotalSeconds)
                {
                    Pedestrian ped = (Pedestrian)triggerShape.Actor.UserData;
                    if (!ped.IsHit)
                    {
                        Race.Current.OnPedestrianHit(ped, noncar.LastVehicleToHit);
                        if (GameVars.Emulation == EmulationMode.Demo)  // billiard.pix doesn't exist in c1 demo
                            MessageRenderer.Instance.PostHeaderMessage("Nice shot, sir!", 3);  
                        else
                            MessageRenderer.Instance.PostMainMessage("billiard.pix", 3, 0.7f, 0.003f, 1.4f);
                    }
                }
            }
        }
    }
}
