using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.Parsers;
using NFSEngine.Audio;
using PlatformEngine;
using NFSEngine;

namespace Carmageddon
{
    static class SoundIds
    {
        public const int CrashStart = 5000;
        public const int CrashEnd = 5004;
        public const int ScrapeStart = 5010;
        public const int ScrapeEnd = 5012;
        public const int SkidStart = 9000;
        public const int SkidEnd = 9004;
    }
    static class SoundCache
    {
        static List<CSoundDescription> _soundDescriptions;
        public static bool IsInitialized;
        static List<ISound> _instances = new List<ISound>();
        static ISound _currentSkid, _currentCrash, _scrape;

        public static void Initialize()
        {
            SoundsFile soundFile = new SoundsFile(GameVariables.BasePath + "data\\sound\\sound.txt");
            _soundDescriptions = soundFile.Sounds;
            IsInitialized = true;
        }

        public static ISound CreateInstance(int id)
        {
            CSoundDescription csound = _soundDescriptions.Find(a => a.Id == id);
            ISound instance = Engine.Instance.Audio.Load(GameVariables.BasePath + "data\\sound\\" + csound.FileName, false);
            instance.Id = csound.Id;
            _instances.Add(instance);
            return instance;
        }

        public static ISound Play(int id)
        {
            ISound instance = _instances.Find(a => a.Id == id);
            if (instance == null)
            {
                instance = CreateInstance(id);
            }
            instance.Play(false);
            return instance;
        }

        public static void PlayCrash()
        {
            PlayGroup(SoundIds.CrashStart, SoundIds.CrashEnd, ref _currentCrash);
        }

        public static void PlayScrape()
        {
            PlayGroup(SoundIds.ScrapeStart, SoundIds.ScrapeEnd, ref _scrape);
        }

        public static void PlaySkid()
        {
            PlayGroup(SoundIds.SkidStart, SoundIds.SkidEnd, ref _currentSkid);
        }

        private static void PlayGroup(int startId, int endId, ref ISound instance)
        {
            if (instance == null || !instance.IsPlaying)
            {
                int id = Engine.Instance.RandomNumber.Next(startId, endId);
                instance = Play(id);
                GameConsole.WriteEvent("Sound " + id);
            }
        }
    }
}
