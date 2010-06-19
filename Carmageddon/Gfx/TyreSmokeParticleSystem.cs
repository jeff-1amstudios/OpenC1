using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Particle3DSample;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;

namespace Carmageddon.Gfx
{
    class TyreSmokeParticleSystem : ParticleSystem
    {
        
        public TyreSmokeParticleSystem()
        {
            InitializeSystem();
        }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.Texture = Engine.ContentManager.Load<Texture2D>("Content/smoke");


            settings.MaxParticles = 150;

            settings.Duration = TimeSpan.FromSeconds(1);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = 2;
            settings.MaxVerticalVelocity = 4;

            settings.Gravity = new Vector3(0, -5, 0);

            settings.EndVelocity = 0.75f;

            settings.MinStartSize = 1;
            settings.MaxStartSize = 2;

            settings.MinEndSize = 3;
            settings.MaxEndSize = 7;
        }
    }
}
