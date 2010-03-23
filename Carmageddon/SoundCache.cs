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
        public const int OutOfTime = 8010;
        public const int RaceCompleted = 8011;
        public const int Checkpoint = 8012;
        public const int WrongCheckpoint = 8013;
    }

    static class SoundCache
    {
        static bool _enabled = true;
        static List<SoundDesc> _soundDescriptions;
        public static bool IsInitialized;
        static List<ISound> _instances = new List<ISound>();
        static ISound _currentSkid, _currentCrash, _currentScrape;

        public static void Initialize()
        {
            SoundsFile soundFile = new SoundsFile(GameVariables.BasePath + "data\\sound\\sound.txt");
            _soundDescriptions = soundFile.Sounds;
            IsInitialized = true;
        }

        public static ISound CreateInstance(int id)
        {
            return CreateInstance(id, false);
        }

        public static ISound CreateInstance(int id, bool is3d)
        {
            if (!_enabled) return null;
            SoundDesc csound = _soundDescriptions.Find(a => a.Id == id);
            ISound instance = Engine.Audio.Load(GameVariables.BasePath + "data\\sound\\" + csound.FileName, is3d);
           
            instance.MinimumDistance = 10;
            instance.MaximumDistance = 200;
            instance.Id = csound.Id;
            _instances.Add(instance);
            return instance;
        }



        public static ISound Play(int id, Vehicle vehicle, bool is3d)
        {
            if (vehicle == null
                || (vehicle.Driver is CpuDriver && ((CpuDriver)vehicle.Driver).InPlayersView)
                || vehicle.Driver is PlayerDriver)
            {
                ISound instance = _instances.Find(a => a.Id == id);
                if (instance == null)
                {
                    instance = CreateInstance(id, is3d);
                }
                if (instance != null)
                {
                    if (is3d) instance.Position = vehicle.Position;
                    instance.Owner = vehicle;
                    instance.Play(false);
                    GameConsole.WriteEvent("PlaySound " + id.ToString());
                }
                return instance;
            }
            else
                return null;
        }

        public static void PlayCrash(Vehicle vehicle)
        {
            PlayGroup(SoundIds.CrashStart, SoundIds.CrashEnd, ref _currentCrash, vehicle);
        }

        public static void PlayScrape(Vehicle vehicle)
        {
            PlayGroup(SoundIds.ScrapeStart, SoundIds.ScrapeEnd, ref _currentScrape, vehicle);
        }

        public static void PlaySkid(Vehicle vehicle)
        {
            PlayGroup(SoundIds.SkidStart, SoundIds.SkidEnd, ref _currentSkid, vehicle);
        }

        private static void PlayGroup(int startId, int endId, ref ISound instance, Vehicle vehicle)
        {
            if (instance != null && instance.IsPlaying && instance.Owner != vehicle)
            {
                if (vehicle.Driver is PlayerDriver)  //priority
                    instance.Reset();
                else if (((Vehicle)instance.Owner).Driver is PlayerDriver)
                {
                    return; //dont steal player's sound
                }
            }

            if (instance == null || !instance.IsPlaying)
            {
                int id = Engine.Random.Next(startId, endId);
                instance = Play(id, vehicle, true);
            }
        }
    }
}
