using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.DirectSound;
using Microsoft.Xna.Framework;
using System.IO;

namespace OneAmEngine.Audio
{

	public class MdxSoundEngine : ISoundEngine
	{
		Device _audioDevice;
        int _defaultVolume;
        private List<ISound> _sounds = new List<ISound>();
        IListener _listener;

		public MdxSoundEngine()
		{
			_audioDevice = new Device();
			_audioDevice.SetCooperativeLevel(Engine.Game.Window.Handle, CooperativeLevel.Priority);
		}

        public void SetDefaultVolume(int volume)
        {
            _defaultVolume = volume;
        }

		public IListener GetListener()
		{
            if (_listener == null)
                _listener = new MdxListener(_audioDevice);
            return _listener;
		}

		public ISound Load(string name, bool is3d)
		{
            if (!File.Exists(name)) return null;
			ISound sound = new MdxSound(_audioDevice, name, is3d);
            sound.Volume = _defaultVolume;
			return sound;
		}

        public void Register3dSound(ISound sound)
        {
            _sounds.Add(sound);
        }

        public void Unregister3dSound(ISound sound)
        {
            _sounds.Remove(sound);
        }


        public void Update()
        {
            if (_listener == null) return;

            Vector3 listenerPos = _listener.Position;

            for (int i = _sounds.Count - 1; i >= 0; i--)
            {
                float distance = Vector3.Distance(_sounds[i].Position, listenerPos);
                if (distance > _sounds[i].MaximumDistance && _sounds[i].IsPlaying)
                {
                    _sounds[i].Pause();
                    _sounds.RemoveAt(i);
                }
                else if (distance < _sounds[i].MaximumDistance && !_sounds[i].IsPlaying)
                {
                    _sounds[i].Play(true);
                }
            }
        }

        public void StopAll()
        {
            foreach (ISound sound in _sounds)
                sound.Stop();

            _sounds.Clear();
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