using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using PlatformEngine;
using NFSEngine.Audio;

namespace NFSEngine
{
    
    public class XactSoundEngine : ISoundEngine
    {
        private List<SoundInstance> _sounds = new List<SoundInstance>();

        

        public ISound Load(string name, bool is3d)
        {
            XactSound sound = new XactSound(Engine.Instance.ContentManager.Load<SoundEffectInstance>(name));
            return sound;
        }

        public void Play(ISound sound, float duration)
        {
            SoundInstance inst = new SoundInstance() { Sound = sound, RemainingDuration = duration };
            _sounds.Add(inst);
        }

        public IListener CreateListener()
        {
            return null;
        }

        public void Update()
        {
            for (int i = _sounds.Count - 1; i >= 0; i--)
            {
                _sounds[i].RemainingDuration -= Engine.Instance.ElapsedSeconds;
                if (_sounds[i].RemainingDuration <= 0)
                {
                    _sounds[i].Sound.Stop();
                    _sounds.RemoveAt(i);
                }
            }
        }
    }
}
