using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.Parsers;
using NFSEngine.Audio;
using PlatformEngine;

namespace Carmageddon
{
    static class SoundCache
    {
        static List<CSound> _sounds;
        public static bool IsInitialized;

        public static void Initialize()
        {
            SoundsFile soundFile = new SoundsFile(GameVariables.BasePath + "sound\\sound.txt");
            _sounds = soundFile.Sounds;
            IsInitialized = true;
        }

        public static ISound CreateInstance(int id)
        {
            CSound csound = _sounds.Find(a => a.Id == id);
            ISound sound = Engine.Instance.Audio.Load(GameVariables.BasePath + "sound\\" + csound.FileName, false);
            return sound;
        }
    }
}
