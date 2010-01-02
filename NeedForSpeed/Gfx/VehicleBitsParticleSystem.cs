using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Particle3DSample;

namespace Carmageddon.Gfx
{
    class VehicleBitsParticleSystem : ParticleSystem
    {

        static VehicleBitsParticleSystem _instance;
        public static VehicleBitsParticleSystem Instance
        {
            get
            {
                if (_instance == null) _instance = new VehicleBitsParticleSystem();
                return _instance;
            }
        }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "Content/sparks";

            settings.MaxParticles = 100;

            settings.Duration = TimeSpan.FromSeconds(1f);

            settings.MinHorizontalVelocity = 0f;
            settings.MaxHorizontalVelocity = 3;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 0;
            settings.EmitterVelocitySensitivity = 0f;

            settings.Gravity = new Vector3(0, 1.5f, 0);

            settings.EndVelocity = 0f;

            settings.MinStartSize = 0.2f;
            settings.MaxStartSize = 0.2f;

            settings.MinEndSize = 0.2f;
            settings.MaxEndSize = 0.5f;

            settings.MinRotateSpeed = 0f;
            settings.MaxRotateSpeed = 10f;
        }
    }
}
