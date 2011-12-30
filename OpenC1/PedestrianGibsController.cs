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
		static List<ParticleEmitter> _particles;

		static PedestrianGibsController()
		{
			_particles = new List<ParticleEmitter>();
			_particles.Add(new ParticleEmitter(new PedestrianGibsParticleSystem("CHUNK01.PIX", 0.5f), 10, Vector3.Zero));
			_particles.Add(new ParticleEmitter(new PedestrianGibsParticleSystem("BIGGIBS2.PIX", 1f), 10, Vector3.Zero));
			_particles.Add(new ParticleEmitter(new PedestrianGibsParticleSystem("BIGGIBS3.PIX", 1f), 10, Vector3.Zero));
		}

		public static void AddGibs(Vector3 position, Vector3 velocity, float carSpeed)
		{
			if (carSpeed < 90)
			{
				_particles[0].DumpParticles(position, 4, velocity);
				//_particles[1].DumpParticles(position, 2, velocity);
			}
			else
			{
				_particles[0].DumpParticles(position, (carSpeed - 90) * 0.07f, velocity);
				_particles[1].DumpParticles(position, (carSpeed - 90) * 0.07f, velocity);
				_particles[2].DumpParticles(position, (carSpeed - 90) * 0.07f, velocity);
			}
		}
    }
}
