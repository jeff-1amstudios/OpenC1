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
		string _pixFileName;
		float _sizeMultiplier;
		public PedestrianGibsParticleSystem(string gibPixFile, float sizeMultiplier)
		{
			_pixFileName = gibPixFile;
			_sizeMultiplier = sizeMultiplier;
			InitializeSystem();
		}


		protected override void InitializeSettings(ParticleSettings settings)
		{
			PixFile pix = new PixFile(_pixFileName);

			settings.Texture = pix.PixMaps[0].Texture;

			settings.MaxParticles = 200;

			settings.Duration = TimeSpan.FromSeconds(1f);

			settings.MinHorizontalVelocity = 1f;
			settings.MaxHorizontalVelocity = 2f;

			settings.MinVerticalVelocity = 2.5f;
			settings.MaxVerticalVelocity = 4.5f;
			settings.DurationRandomness = 0.3f;
			settings.EmitterVelocitySensitivity = 0.7f;

			settings.Gravity = new Vector3(0, -3.5f, 0);

			settings.EndVelocity = 1f;

			settings.MinStartSize = 0.25f * _sizeMultiplier;
			settings.MaxStartSize = 0.65f * _sizeMultiplier;

			settings.MinEndSize = 0.25f * _sizeMultiplier;
			settings.MaxEndSize = 1.15f * _sizeMultiplier;

			settings.MinRotateSpeed = 0f;
			settings.MaxRotateSpeed = 6f;
		}
	}
}
