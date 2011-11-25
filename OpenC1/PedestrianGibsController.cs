using System;
using System.Collections.Generic;
using System.Text;
using OneAmEngine;
using OpenC1.Gfx;
using Microsoft.Xna.Framework;

namespace OpenC1
{
    static class PedestrianGibsController
    {
		static ParticleEmitter _particles;

        static PedestrianGibsController()
        {
			_particles = new ParticleEmitter(new PedestrianGibsParticleSystem(null), 10, Vector3.Zero);
        }

		public static void AddGibs(Vector3 position, Vector3 velocity)
		{
			_particles.DumpParticles(position, 7, velocity);
		}
    }
}
