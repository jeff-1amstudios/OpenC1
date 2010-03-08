    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Xna.Framework;
    using Particle3DSample;
    using PlatformEngine;
    using Microsoft.Xna.Framework.Graphics;

namespace Carmageddon.Gfx
{
    class MaterialSmokeParticleSystem : ParticleSystem
    {

        Color _color;
        public MaterialSmokeParticleSystem(Color color)
        {
            _color = color;
            InitializeSystem();
        }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.Texture = Engine.ContentManager.Load<Texture2D>("Content/material-modifier-smoke");
            
            settings.MaxParticles = 200;
            settings.MinColor = _color;
            settings.MaxColor = _color;

            settings.Duration = TimeSpan.FromSeconds(0.7f);

            settings.MinHorizontalVelocity = 0.2f;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = 1;
            settings.MaxVerticalVelocity = 4;

            settings.Gravity = new Vector3(0, -5, 0);

            settings.EndVelocity = 0.75f;

            settings.MinStartSize = 1;
            settings.MaxStartSize = 1;

            settings.MinEndSize = 2;
            settings.MaxEndSize = 2;
        }
    }
}
    
