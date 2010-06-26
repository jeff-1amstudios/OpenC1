using System;
using System.Collections.Generic;
using System.Text;
using Particle3DSample;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Carmageddon.Gfx
{
    class DamageSmokeParticleSystem : ParticleSystem
    {
        Color _color;

        public DamageSmokeParticleSystem(Color color)
        {
            _color = color;
            InitializeSystem();
        }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.Texture = Engine.ContentManager.Load<Texture2D>("Content/damage-smoke");
            
            settings.MaxParticles = 150;

            settings.Duration = TimeSpan.FromSeconds(1.2f);

            settings.MinColor = _color;
            settings.MaxColor = _color;
            
            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = 2;
            settings.MaxVerticalVelocity = 3;

            //settings.Gravity = new Vector3(0, -5, 0);
            settings.DurationRandomness = 0.5f;
            

            settings.EndVelocity = 0.7f;

            settings.MinStartSize = 0.3f;
            settings.MaxStartSize = 1f;

            settings.MinEndSize = 2.5f;
            settings.MaxEndSize = 3;
        }
    }
}
