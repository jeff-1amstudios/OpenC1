using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.DirectSound;
using Carmageddon.Parsers;

namespace Carmageddon.Audio
{

	static class SoundEngine
	{
		static Device _audioDevice;
        static SoundsFile _soundsFile;

		public static void Initialize(IntPtr windowHandle, string soundsFileName)
		{
			_audioDevice = new Device();
			_audioDevice.SetCooperativeLevel(windowHandle, CooperativeLevel.Priority);

            _soundsFile = new SoundsFile(soundsFileName);
		}

		public static SoundListener CreateListener()
		{
			return new SoundListener(_audioDevice);
		}

		public static Sound LoadSound(string filename)
		{
			Sound sound = new Sound(_audioDevice, filename);
			return sound;
		}

        public static Sound LoadSound(int id)
        {
            CSound soundDescription = _soundsFile.Sounds.Find(s => s.Id == id);
            Sound sound = new Sound(_audioDevice, soundDescription.FileName);
            return sound;
        }
	}
}

public static class DirectSoundExtensions
{
    public static Microsoft.Xna.Framework.Vector3 ToXna(this Microsoft.DirectX.Vector3 mdxVec3)
    {
        return new Microsoft.Xna.Framework.Vector3(mdxVec3.X, mdxVec3.Y, mdxVec3.Z);
    }

    public static Microsoft.DirectX.Vector3 ToMdx(this Microsoft.Xna.Framework.Vector3 xnaVec3)
    {
        return new Microsoft.DirectX.Vector3(xnaVec3.X, xnaVec3.Y, xnaVec3.Z);
    }
}