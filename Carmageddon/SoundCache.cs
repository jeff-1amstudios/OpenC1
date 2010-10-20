using System;
using System.Collections.Generic;
using System.Text;
using OpenC1.Parsers;

using Microsoft.Xna.Framework;
using OneAmEngine.Audio;
using OneAmEngine;

namespace OpenC1
{
    static class SoundIds
    {
        public const int Repair = 5200;
        public const int CrashStart = 5000;
        public const int CrashEnd = 5004;
        public const int ScrapeStart = 5010;
        public const int ScrapeEnd = 5012;
        public const int SkidStart = 9000;
        public const int SkidEnd = 9002;
        public const int ScrubStart = 9003;
        public const int ScrubEnd = 9004;
        public const int OutOfTime = 8010;
        public const int RaceCompleted = 8011;
        public const int Checkpoint = 8012;
        public const int WrongCheckpoint = 8013;
        public const int Clapping = 8015;
        public const int CopSiren = 5350;
        public const int UI_UpDown = 3000;
        public const int UI_Ok = 3004;
        public const int UI_Esc = 3005;
    }

    static class SoundCache
    {
        static bool _enabled = true;
        static List<SoundDesc> _soundDescriptions;
        public static bool IsInitialized;
        static List<ISound> _playerInstances = new List<ISound>();
        static List<ISound> _aiInstances = new List<ISound>();
        static ISound _currentSkid, _currentScrub, _currentCrash;

        public static void Initialize()
        {
            SoundsFile soundFile = new SoundsFile(GameVars.BasePath + "data\\sound\\sound.txt");
            _soundDescriptions = soundFile.Sounds;
            IsInitialized = true;

            if (!_enabled) return;

            //foreach (SoundDesc desc in _soundDescriptions)
            //    CreateInstance(desc.Id, true);

            CreateInstance(5000, true);
            CreateInstance(5001, true);
            CreateInstance(5002, true);
            CreateInstance(5003, true);
            CreateInstance(5004, true);
        }

        public static ISound CreateInstance(int id)
        {
            return CreateInstance(id, false);
        }

        public static ISound CreateInstance(int id, bool is3d)
        {
            if (!_enabled) return null;
            SoundDesc csound = _soundDescriptions.Find(a => a.Id == id);
            ISound instance = Engine.Audio.Load(GameVars.BasePath + "data\\sound\\" + csound.FileName, is3d);

            if (_playerInstances.Exists(a => a.Id == id))
            {
            }
           
            instance.MinimumDistance = 10;
            instance.MaximumDistance = 200;
            instance.Id = csound.Id;
            _playerInstances.Add(instance);
            return instance;
        }



        public static ISound Play(int id, Vehicle vehicle, bool is3d)
        {
            if (vehicle == null
                || (vehicle.Driver is CpuDriver && ((CpuDriver)vehicle.Driver).InPlayersView)
                || vehicle.Driver is PlayerDriver)
            {
                ISound instance = _playerInstances.Find(a => a.Id == id);
                if (instance == null)
                {
                    instance = CreateInstance(id, is3d);
                }
                if (instance != null)
                {
                    if (is3d) instance.Position = vehicle.Position;
                    instance.Owner = vehicle;
                    instance.Play(false);
                    //GameConsole.WriteEvent("PlaySound " + id.ToString());
                }
                return instance;
            }
            else
                return null;
        }

        public static void PlayCrash(Vehicle vehicle, float force)
        {
            PlayGroup(SoundIds.CrashStart, SoundIds.CrashEnd, ref _currentCrash, vehicle);
            
        }

        public static void PlayScrape(Vehicle vehicle)
        {
            PlayGroup(SoundIds.ScrapeStart, SoundIds.ScrapeEnd, ref _currentCrash, vehicle);
        }

        public static void PlaySkid(Vehicle vehicle, float factor)
        {
            if (factor > 0.35f)
                PlayGroup(SoundIds.SkidStart, SoundIds.SkidEnd, ref _currentSkid, vehicle);
            PlayGroup(SoundIds.ScrubStart, SoundIds.ScrubEnd, ref _currentScrub, vehicle);
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
                int id = Engine.Random.Next(startId, endId+1);
                instance = Play(id, vehicle, true);
            }
        }
    }
}
