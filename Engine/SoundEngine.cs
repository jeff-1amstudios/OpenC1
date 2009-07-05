using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace PlatformEngine
{
    public class SoundEngine
    {
        private static SoundEngine _instance;

        public static SoundEngine Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SoundEngine();
                return _instance;
            }
        }

        private AudioEngine _engine;
        private WaveBank _wavebank;
        private SoundBank _soundbank;

        private SoundEngine()
        {
            return;
            _engine = new AudioEngine("Content\\Sounds\\Sounds.xgs");
            _wavebank = new WaveBank(_engine, "Content\\Sounds\\Wave Bank.xwb");
            _soundbank = new SoundBank(_engine, "Content\\Sounds\\Sound Bank.xsb");

        }

        public void Play(string name)
        {
            _soundbank.PlayCue(name);
        }

        public void Stop(Cue cue)
        {
            cue.Stop(AudioStopOptions.Immediate);
        }
        
        public void Update()
        {
            if (_engine != null)
                _engine.Update();
        }

        /// <summary>
        /// Shuts down the sound code tidily
        /// </summary>
        public void Shutdown()
        {
            _soundbank.Dispose();
            _wavebank.Dispose();
            _engine.Dispose();
        }
    }
}
