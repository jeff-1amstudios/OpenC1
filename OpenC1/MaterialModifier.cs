using System;
using System.Collections.Generic;
using System.Text;
using StillDesign.PhysX;
using OpenC1.Physics;
using Microsoft.Xna.Framework;
using OneAmEngine;
using OpenC1.Gfx;
using OpenC1.Parsers;

namespace OpenC1
{
    class MaterialModifier
    {
        public float CarWallFriction;
        public float TyreRoadFriction;
        public float Downforce;
        public float Bumpiness;
        public int TyreSoundIndex;
        public int CrashSoundIndex;
        public int ScrapeSoundIndex;
        public float Sparkiness;
        public int SmokeTableIndex;
        public CMaterial SkidMaterial;

        private float _lastBump, _nextWheel;
        private static TyreSmokeParticleSystem _defaultTyreSmokeSystem;
        private MaterialSmokeParticleSystem SmokeParticles;
        private ParticleEmitter _emitter;

        public void Initialize(RaceFile race)
        {
            if (SmokeTableIndex > 1)
            {
                //-2 because index is 1-based in race file and #1 is the default (no smoke)
                SmokeParticles = new MaterialSmokeParticleSystem(race.SmokeTables[SmokeTableIndex - 2]);
                _emitter = new ParticleEmitter(SmokeParticles, 5, Vector3.Zero);
            }

            if (_defaultTyreSmokeSystem == null)
                _defaultTyreSmokeSystem = new TyreSmokeParticleSystem();
        }

        public void UpdateWheelShape(VehicleChassis chassis, VehicleWheel wheel)
        {
            if (Bumpiness > 0)
            {
                if (wheel.Index == _nextWheel && chassis.Speed > 5 && _lastBump + 0.3f < Engine.TotalSeconds && !wheel.InAir)
                {
                    chassis.Actor.AddForceAtLocalPosition(new Vector3(0, Bumpiness * 65, 0), wheel.Shape.LocalPosition, ForceMode.Impulse, true);
                    _lastBump = Engine.TotalSeconds;
                    _nextWheel = Engine.Random.Next(0, chassis.Wheels.Count - 1);
                }
            }

            if (SmokeParticles != null)
            {
                wheel.SmokeEmitter.ParticleSystem = SmokeParticles;
                wheel.SmokeEmitter.Enabled = chassis.Speed > 5;
            }
            else
                wheel.SmokeEmitter.ParticleSystem = _defaultTyreSmokeSystem;

            chassis.Vehicle.SkidMarkBuffer.SetTexture(SkidMaterial == null ? null : SkidMaterial.Texture);
        }
    }
}
