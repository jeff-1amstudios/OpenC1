using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using OneAmEngine;
using Microsoft.Xna.Framework.Graphics;
using OpenC1.Parsers;

namespace OpenC1.Gfx
{
	class PedestrianGibsParticleSystem : ParticleSystem
	{
		CMaterial _material;

		public PedestrianGibsParticleSystem(CMaterial material)
		{
			_material = material;
			InitializeSystem();
		}


		protected override void InitializeSettings(ParticleSettings settings)
		{
			PixFile pix = new PixFile("BIGGIBS3.PIX");

			settings.Texture = pix.PixMaps[0].Texture;

			settings.MaxParticles = 200;

			settings.MinColor = new Color(1, 1, 1, 0.3f);
			settings.MaxColor = new Color(1, 1, 1, 1f);

			settings.Duration = TimeSpan.FromSeconds(1f);

			settings.MinHorizontalVelocity = 1f;
			settings.MaxHorizontalVelocity = 2f;

			settings.MinVerticalVelocity = 2.5f;
			settings.MaxVerticalVelocity = 4.5f;
			settings.DurationRandomness = 0.3f;
			settings.EmitterVelocitySensitivity = 0.7f;

			settings.Gravity = new Vector3(0, -3.5f, 0);

			settings.EndVelocity = 1f;

			settings.MinStartSize = 0.65f;
			settings.MaxStartSize = 1.25f;

			settings.MinEndSize = 0.45f;
			settings.MaxEndSize = 0.65f;

			settings.MinRotateSpeed = 0f;
			settings.MaxRotateSpeed = 6f;
		}
	}
}
