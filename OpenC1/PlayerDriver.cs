using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using OpenC1.Physics;
using Microsoft.Xna.Framework.Input;
using OneAmEngine;
using OneAmEngine.Audio;

namespace OpenC1
{
    class PlayerDriver : IDriver
    {
        List<Matrix> _recoverPositions = new List<Matrix>();
        public float _lastRecoverTime = 0;
        public Vehicle Vehicle {get; set; }
        

        IListener _audioListener;

        public PlayerDriver()
        {
            _audioListener = Engine.Audio.GetListener();
            _audioListener.DistanceFactor = 2f;
            _audioListener.RolloffFactor = 1f;
        }

        public bool ModerateSteeringAtSpeed { get { return true; } }

        public void OnRaceStart()
        {
            Vehicle.Chassis.Motor.Gearbox.CurrentGear = 1;
        }

        public void Update()
        {
            VehicleChassis chassis = Vehicle.Chassis;
            
            if (PlayerVehicleController.Brake != 0)
                chassis.Brake(PlayerVehicleController.Brake);
            else
                chassis.Accelerate(PlayerVehicleController.Acceleration);
            
            chassis.Steer(-PlayerVehicleController.Turn);

            if (PlayerVehicleController.Handbrake)
                chassis.PullHandbrake();
            else
                chassis.ReleaseHandbrake();

            if (Engine.Input.WasPressed(Keys.R))
            {
                if (_recoverPositions.Count > 0)
                {
                    Vehicle.Recover(_recoverPositions[_recoverPositions.Count - 1]);
                    _recoverPositions.RemoveAt(_recoverPositions.Count - 1);
                }
                else
                    Vehicle.Chassis.Reset();
            }

            if (Engine.Input.WasPressed(Keys.Back))
            {
                Vehicle.Repair();
            }

            _audioListener.BeginUpdate();
            _audioListener.Position = Vehicle.Chassis.Actor.GlobalPosition;
            _audioListener.Orientation = Vehicle.Chassis.Actor.GlobalOrientation;
            _audioListener.Velocity = Vector3.Zero;
            _audioListener.CommitChanges();

            if (Engine.TotalSeconds > _lastRecoverTime + 5)
            {
                _recoverPositions.Add(Vehicle.Chassis.Actor.GlobalPose);
                if (_recoverPositions.Count > 10)
                    _recoverPositions.RemoveAt(0);

                _lastRecoverTime = Engine.TotalSeconds;
            }
        }
    }
}
