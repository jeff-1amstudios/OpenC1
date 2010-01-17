using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Particle3DSample;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;

namespace Carmageddon.Gfx
{
    class SparksParticleSystem : ParticleSystem
    {

        public SparksParticleSystem()
        {
            InitializeSystem();
        }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.Texture = Engine.Instance.ContentManager.Load<Texture2D>("Content/sparks");

            settings.MaxParticles = 100;

            settings.Duration = TimeSpan.FromSeconds(1.6f);

            settings.MinHorizontalVelocity = 1f;
            settings.MaxHorizontalVelocity = 2.5f;

            settings.MinVerticalVelocity = 1f;
            settings.MaxVerticalVelocity = 2f;
            settings.DurationRandomness = 0.8f;
            settings.EmitterVelocitySensitivity = 0f;

            settings.Gravity = new Vector3(0, -1.3f, 0);

            settings.EndVelocity = 1f;

            settings.MinStartSize = 0.1f;
            settings.MaxStartSize = 0.2f;

            settings.MinEndSize = 0.1f;
            settings.MaxEndSize = 0.2f;

            settings.MinRotateSpeed = 0f;
            settings.MaxRotateSpeed = 2f;
        }
    }
}
