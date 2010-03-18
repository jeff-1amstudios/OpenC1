using System;
using System.Collections.Generic;
using System.Text;
using NFSEngine.Audio;

namespace Carmageddon
{
    class VehicleAudio
    {
        Vehicle _vehicle;

        List<ISound> _engineSounds = new List<ISound>();
        ISound _fromSound, _sound;
        

        public VehicleAudio(Vehicle vehicle)
        {
            foreach (int id in vehicle.Config.EngineSoundIds)
            {
                ISound sound = SoundCache.CreateInstance(id, vehicle.Driver is CpuDriver);
                sound.MaximumDistance = 10;
                _engineSounds.Add(sound);
            }

            _sound = _engineSounds[0];
            _vehicle = vehicle;
        }

        public void Play()
        {
            if (!(_vehicle.Driver is PlayerDriver))
            {
                if (_sound != null)
                {
                    _sound.Play(true);
                    //_engineSound.Volume -= 1000;
                }
            }
        }

        public void Update()
        {
            if (_sound != null)
            {
                _sound.Frequency = 8000 + (int)(_vehicle.Chassis.Motor.Rpm * 2500);
                _sound.Position = _vehicle.Position;
                _sound.Velocity = _vehicle.Chassis.Actor.LinearVelocity;
            }
        }

        public void SetSound(int index)
        {
            if (_sound != _engineSounds[index])
            {
                _sound.Stop();
                _sound = _engineSounds[index];
                _sound.Play(true);
            }
        }
    }
}
