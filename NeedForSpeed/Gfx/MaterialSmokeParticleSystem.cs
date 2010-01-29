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
            settings.Texture = Engine.Instance.ContentManager.Load<Texture2D>("Content/material-modifier-smoke");
            
            settings.MaxParticles = 150;
            settings.MinColor = _color;
            settings.MaxColor = _color;

            settings.Duration = TimeSpan.FromSeconds(1f);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = 1f;
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
    
