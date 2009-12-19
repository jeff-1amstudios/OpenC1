using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using PlatformEngine;
using Microsoft.Xna.Framework;

namespace NFSEngine.Audio
{
    
    class XactSound : ISound
    {
        public float  Duration { get; set; }

        SoundEffectInstance _effect;

        public XactSound(SoundEffectInstance effect)
        {
            _effect = effect;
        }

        #region ISoundDescription Members

        public void Stop()
        {
            _effect.Stop();
        }

        public void Play(bool loop)
        {
            _effect.Play();
        }

        public Vector3 Position
        {
            set
            {
            }
        }

        public Vector3 Velocity
        {
            set { }
        }

        public int Frequency { set { } }

        #endregion
    }
}
