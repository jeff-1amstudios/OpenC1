using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework;
using Carmageddon.HUD;

namespace Carmageddon
{
    class RaceTimeController
    {
        public bool IsStarted, IsOver;
        public bool CountingDown { get; private set; }
        public float CountdownTime;
        int _lastSecond=-1;
        public float TimeRemaining = 1; // 90; //1:30
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
                    //SoundCache.Play(SoundIds.OutOfTime);
                }
            }
            if (CountingDown)
            {
                CountdownTime += Engine.ElapsedSeconds;
                if (CountdownTime > 5)
                {
                    IsStarted = true;
                    ((Driver)Engine.Player).VehicleModel.Chassis.Motor.Gearbox.CurrentGear = 1;
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
                    SoundCache.Play(_countdownSoundIds[second]);
                    MessageRenderer.Instance.PostMessagePix(_countdownTextures[second], 0.7f, 0.24f, 0.003f, 0);
                }

                _lastSecond = second;
            }            

            if (TimeRemaining == 0)
            {
                //MessageRenderer.Instance.PostMessagePix("timeup.pix", 10, 0.6f, 0.0033f);   
            }
        }
    }
}
