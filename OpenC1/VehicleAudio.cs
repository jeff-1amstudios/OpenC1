using System;
using System.Collections.Generic;
using System.Text;
using OneAmEngine.Audio;

using Microsoft.Xna.Framework;

namespace OpenC1
{
    class VehicleAudio
    {
        Vehicle _vehicle;

        List<ISound> _engineSounds;
        ISound _fromSound, _sound;
        

        public VehicleAudio(Vehicle vehicle)
        {
            _vehicle = vehicle;
            if (vehicle.Driver is CpuDriver) return;

            _engineSounds = new List<ISound>();
            foreach (int id in vehicle.Config.EngineSoundIds)
            {
                ISound sound = SoundCache.CreateInstance(id, true);
                if (sound != null)
                {
                    sound.MinimumDistance = 20;
                    sound.MaximumDistance = 100;
                    _engineSounds.Add(sound);
                }
            }
            if (_engineSounds.Count > 0)
                _sound = _engineSounds[0];
            
        }

        public void Play()
        {
            if (_sound != null)
            {
                _sound.Play(true);
            }
        }

        public void Update()
        {
            if (_sound != null)
            {
                _sound.Frequency = 8000 + (int)(_vehicle.Chassis.Motor.Rpm * 2500);
                _sound.Position = _vehicle.Position;
                if (!(_vehicle.Driver is PlayerDriver))
                {
                    _sound.Velocity = _vehicle.Chassis.Actor.LinearVelocity;
                }
            }
        }

        public void SetSound(int index)
        {
            if (_engineSounds == null || _engineSounds.Count == 0) return;

            if (_sound != _engineSounds[index])
            {
                _sound.Stop();
                _sound = _engineSounds[index];
                _sound.Play(true);
            }
        }

		public void Stop()
		{
			if (_sound != null) _sound.Stop();
		}
    }
}
