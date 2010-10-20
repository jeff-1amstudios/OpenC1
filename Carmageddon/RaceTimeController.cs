using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using OpenC1.Parsers;
using Microsoft.Xna.Framework;
using OpenC1.HUD;
using OpenC1.GameModes;
using OneAmEngine;
using OneAmEngine.Audio;

namespace OpenC1
{
    class RaceTimeController
    {
        public bool IsStarted, IsOver;
        public bool CountingDown { get; private set; }
        public float CountdownTime;
        int _lastSecond = -1;
        public float TimeRemaining = 90; //1:30
        List<string> _countdownTextures = new List<string>();
        List<int> _countdownSoundIds = new List<int>();
        public float TotalTime;

        public RaceTimeController()
        {
            _countdownTextures.Add("number5.pix");
            _countdownTextures.Add("number4.pix");
            _countdownTextures.Add("number3.pix");
            _countdownTextures.Add("number2.pix");
            _countdownTextures.Add("number1.pix");
            _countdownTextures.Add("go.pix");

            _countdownSoundIds.Add(8005);
            _countdownSoundIds.Add(8004);
            _countdownSoundIds.Add(8003);
            _countdownSoundIds.Add(8002);
            _countdownSoundIds.Add(8001);
            _countdownSoundIds.Add(8000);
        }

        public void StartCountdown()
        {
            CountingDown = true;
        }

        public void Update()
        {
            TotalTime += Engine.ElapsedSeconds;

            if (IsOver) return;

            if (IsStarted)
            {
                TimeRemaining -= Engine.ElapsedSeconds;
                if (TimeRemaining < 0)
                {
                    TimeRemaining = 0;
                    IsOver = true;
                    GameMode.Current = new RaceCompletedMode(CompletionType.TimeUp);
                }
            }
            if (CountingDown)
            {
                CountdownTime += Engine.ElapsedSeconds;
                if (CountdownTime > 5)
                {
                    IsStarted = true;
                    foreach (IDriver driver in Race.Current.Drivers)
                        driver.OnRaceStart();
                }
                if (CountdownTime > 6)
                    CountingDown = false;
            }
        }

        public void Render()
        {
            if (CountingDown)
            {
                int second = (int)CountdownTime;
                if (second > _lastSecond)
                {
                    ISound sound = SoundCache.Play(_countdownSoundIds[second], null, false);
                    sound.Volume = -1600;
                    MessageRenderer.Instance.PostMainMessage(_countdownTextures[second], 0.7f, 0.24f, 0.003f, 0);
                }

                _lastSecond = second;
            }            
        }
    }
}
