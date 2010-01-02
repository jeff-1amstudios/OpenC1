using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Particle3DSample;

namespace Carmageddon.Gfx
{
    class SparksParticleSystem : ParticleSystem
    {

        static SparksParticleSystem _instance;
        public static SparksParticleSystem Instance
        {
            get
            {
                if (_instance == null) _instance = new SparksParticleSystem();
                return _instance;
            }
        }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "Content/sparks";

            settings.MaxParticles = 100;

            settings.Duration = TimeSpan.FromSeconds(2f);

            settings.MinHorizontalVelocity = 0f;
            settings.MaxHorizontalVelocity = 3;

            settings.MinVerticalVelocity = 1.5f;
            settings.MaxVerticalVelocity = 2f;
            settings.DurationRandomness = 0.5f;
            settings.EmitterVelocitySensitivity = 0f;

            settings.Gravity = new Vector3(0, -0.9f, 0);

            settings.EndVelocity = 0f;

            settings.MinStartSize = 0.2f;
            settings.MaxStartSize = 0.3f;

            settings.MinEndSize = 0.3f;
            settings.MaxEndSize = 0.4f;

            settings.MinRotateSpeed = 0f;
            settings.MaxRotateSpeed = 3f;
        }
    }
}
