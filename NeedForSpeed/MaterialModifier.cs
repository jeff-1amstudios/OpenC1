using System;
using System.Collections.Generic;
using System.Text;
using StillDesign.PhysX;
using Carmageddon.Physics;
using Microsoft.Xna.Framework;
using PlatformEngine;
using Particle3DSample;
using Carmageddon.Gfx;
using Carmageddon.Parsers;

namespace Carmageddon
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
                if (wheel.Index == _nextWheel && chassis.Speed > 5 && _lastBump + 0.3f < Engine.Instance.TotalSeconds)
                {
                    chassis.Body.AddForceAtLocalPosition(new Vector3(0, Bumpiness * 55, 0), wheel.Shape.LocalPosition, ForceMode.Impulse, true);
                    _lastBump = Engine.Instance.TotalSeconds;
                    _nextWheel = Engine.Instance.RandomNumber.Next(0, chassis.Wheels.Count - 1);
                }
            }

            if (SmokeParticles != null)
            {
                wheel.SmokeEmitter.ParticleSystem = SmokeParticles;
                wheel.SmokeEmitter.Enabled = chassis.Speed > 5;
            }
            else
                wheel.SmokeEmitter.ParticleSystem = _defaultTyreSmokeSystem;

            GameVariables.SkidMarkBuffer.SetTexture(SkidMaterial.Texture);
        }
    }
}
