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
    class CMaterialModifier
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
        public string SkidMaterial;

        private float _lastBump, _nextWheel;
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
        }

        public void UpdateWheelShape(VehicleChassis chassis, VehicleWheel wheel)
        {
            if (Bumpiness > 0)
            {
                if (wheel.Index == _nextWheel && _lastBump + 0.3f < Engine.Instance.TotalSeconds)
                {
                    chassis.Body.AddForceAtLocalPosition(new Vector3(0, Bumpiness * 55, 0), wheel.Shape.LocalPosition, ForceMode.Impulse, true);
                    _lastBump = Engine.Instance.TotalSeconds;
                    _nextWheel = Engine.Instance.RandomNumber.Next(0, chassis.Wheels.Count - 1);
                }
            }

            if (SmokeParticles != null && wheel.Index == 1)
            {
                _emitter.Enabled = true;
                _emitter.Update(wheel.Shape.GlobalPosition);
            }
        }

        internal void Update()
        {
            if (SmokeParticles != null)
            {
                SmokeParticles.SetCamera(Engine.Instance.Camera);
                SmokeParticles.Update();
            }
        }

        internal void Render()
        {
            if (SmokeParticles != null)
            {
                SmokeParticles.Render();
            }
        }
    }
}
