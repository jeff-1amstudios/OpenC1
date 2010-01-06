using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Particle3DSample;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;

namespace Carmageddon.Gfx
{
    class VehicleBitsParticleSystem : ParticleSystem
    {
        CMaterial _material;

        public VehicleBitsParticleSystem(List<CMaterial> materials)
        {
            _material = materials[0];
            InitializeSystem();
        }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.Texture = Engine.Instance.ContentManager.Load<Texture2D>("Content/blank-particle");

            settings.MaxParticles = 100;
            Color color = GameVariables.Palette.GetRGBColorForPixel(_material.BasePixel);
            settings.MinColor = color;
            settings.MaxColor = color;

            settings.Duration = TimeSpan.FromSeconds(1.5f);

            settings.MinHorizontalVelocity = 2f;
            settings.MaxHorizontalVelocity = 3f;

            settings.MinVerticalVelocity = 1.5f;
            settings.MaxVerticalVelocity = 3f;
            settings.DurationRandomness = 0.2f;
            settings.EmitterVelocitySensitivity = 0f;

            settings.Gravity = new Vector3(0, -6f, 0);

            settings.EndVelocity = 1f;

            settings.MinStartSize = 0.2f;
            settings.MaxStartSize = 0.8f;

            settings.MinEndSize = 0.2f;
            settings.MaxEndSize = 0.8f;

            settings.MinRotateSpeed = 6f;
            settings.MaxRotateSpeed = 8f;
        }
    }
}
