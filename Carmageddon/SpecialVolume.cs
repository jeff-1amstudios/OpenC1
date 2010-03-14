using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Carmageddon.Physics;
using StillDesign.PhysX;
using PlatformEngine;

namespace Carmageddon
{
    class SpecialVolume
    {
        public int Id;
        public Matrix Matrix;
        public float Gravity, Viscosity, CarDamagePerMs, PedDamagePerMs;
        public int CameraEffectIndex, SkyColor, EntrySoundId, ExitSoundId;
        public int EngineSoundIndex, MaterialIndex;
        public string WindscreenMaterial;

        public SpecialVolume Copy()
        {
            SpecialVolume vol2 = new SpecialVolume();
            vol2.Id = Id;
            vol2.Gravity = Gravity;
            vol2.Viscosity = Viscosity;
            vol2.CarDamagePerMs = CarDamagePerMs;
            vol2.PedDamagePerMs = PedDamagePerMs;
            vol2.CameraEffectIndex = CameraEffectIndex;
            vol2.SkyColor = SkyColor;
            vol2.EntrySoundId = EntrySoundId;
            vol2.ExitSoundId = ExitSoundId;
            vol2.EngineSoundIndex = EngineSoundIndex;
            vol2.MaterialIndex = MaterialIndex;
            vol2.WindscreenMaterial = WindscreenMaterial;
            return vol2;
        }

        public void Enter(Vehicle vehicle)
        {
            if (Gravity < 1)
                vehicle.Chassis.Actor.RaiseBodyFlag(StillDesign.PhysX.BodyFlag.DisableGravity);
            else
                vehicle.Chassis.Actor.ClearBodyFlag(StillDesign.PhysX.BodyFlag.DisableGravity);
            vehicle.EngineSoundIndex = EngineSoundIndex;
            //vehicle.Chassis.Motor.MaxPower = vehicle.Config.EnginePower / (Viscosity / 35f);
            //vehicle.Chassis.Body.LinearDamping = Viscosity / 200f;
            //vehicle.Chassis.Body.AngularDamping = Viscosity / 80f;
            //vehicle.Chassis.Body.SetCenterOfMassOffsetLocalPosition(new Vector3(0, 0.25f, 0));
            
            //vehicle.Chassis.Body.Mass = vehicle.Config.Mass * Gravity;
            
            if (EntrySoundId > 0)
                SoundCache.Play(EntrySoundId);
        }

        public void Update(Vehicle vehicle)
        {
            if (Gravity < 1)
            {
                vehicle.Chassis.Actor.AddForce(new Vector3(0, PhysX.Instance.Gravity * Gravity * 15f, 0), ForceMode.SmoothImpulse);
            }
        }

        public void Exit()
        {
            if (ExitSoundId > 0)
                SoundCache.Play(ExitSoundId);
        }

        public void Reset(Vehicle vehicle)
        {
            vehicle.Chassis.Actor.ClearBodyFlag(StillDesign.PhysX.BodyFlag.DisableGravity);
            vehicle.EngineSoundIndex = 0;
            //vehicle.Chassis.Motor.MaxPower = vehicle.Config.EnginePower;
            //vehicle.Chassis.Body.Mass = vehicle.Config.Mass;
            //vehicle.Chassis.Body.LinearDamping = 0.0f;
            //vehicle.Chassis.Body.AngularDamping = 0.05f;
        }
    }
}
