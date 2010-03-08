using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.DirectSound;
using PlatformEngine;

namespace NFSEngine.Audio
{

	public class MdxSoundEngine : ISoundEngine
	{
		Device _audioDevice;
        private List<SoundInstance> _sounds = new List<SoundInstance>();

		public MdxSoundEngine()
		{
			_audioDevice = new Device();
			_audioDevice.SetCooperativeLevel(Engine.Game.Window.Handle, CooperativeLevel.Priority);
		}

		public IListener CreateListener()
		{
			return new MdxListener(_audioDevice);
		}

		public ISound Load(string name, bool is3d)
		{
			ISound sound = new MdxSound(_audioDevice, name, is3d);
			return sound;
		}

        public void Play(ISound sound, float duration)
        {
            SoundInstance inst = new SoundInstance() { Sound = sound, RemainingDuration = duration };
            _sounds.Add(inst);
        }

        public void Update()
        {
            for (int i = _sounds.Count - 1; i >= 0; i--)
            {
                _sounds[i].RemainingDuration -= Engine.ElapsedSeconds;
                if (_sounds[i].RemainingDuration <= 0)
                {
                    _sounds[i].Sound.Stop();
                    _sounds.RemoveAt(i);
                }
            }
        }
	}
}


internal static class MdxHelpers
{
    public static Microsoft.Xna.Framework.Vector3 ToXna(Microsoft.DirectX.Vector3 mdxVec3)
    {
        return new Microsoft.Xna.Framework.Vector3(mdxVec3.X, mdxVec3.Y, mdxVec3.Z);
    }

    public static Microsoft.DirectX.Vector3 ToMdx(Microsoft.Xna.Framework.Vector3 xnaVec3)
    {
        return new Microsoft.DirectX.Vector3(xnaVec3.X, xnaVec3.Y, xnaVec3.Z);
    }
}