using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Particle3DSample;

namespace Carmageddon.Gfx
{
    class TyreSmokeParticleSystem : ParticleSystem
    {
        
        int _wheelSwitch;


        static TyreSmokeParticleSystem _instance;
        public static TyreSmokeParticleSystem Instance
        {
            get
            {
                if (_instance == null) _instance = new TyreSmokeParticleSystem();
                return _instance;
            }
        }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "Content/smoke";

            settings.MaxParticles = 200;

            settings.Duration = TimeSpan.FromSeconds(1);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 15;

            settings.MinVerticalVelocity = 5;
            settings.MaxVerticalVelocity = 10;

            // Create a wind effect by tilting the gravity vector sideways.
            settings.Gravity = new Vector3(0, -5, 0);

            settings.EndVelocity = 0.75f;

            //settings.MinRotateSpeed = -1;
            //settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 5;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 20;
            settings.MaxEndSize = 50;
        }
    }
}
