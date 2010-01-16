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
        public float TimeRemaining = 20; // 90; //1:30
        List<Texture2D> _countdownTextures = new List<Texture2D>();
        List<int> _countdownSoundIds = new List<int>();
        Texture2D _outOfTime;
        public float TotalTime;

        public RaceTimeController()
        {
            PixFile pix = new PixFile(GameVariables.BasePath + "data\\pixelmap\\number5.pix");
            _countdownTextures.Add(pix.PixMaps[0].Texture);
            pix = new PixFile(GameVariables.BasePath + "data\\pixelmap\\number4.pix");
            _countdownTextures.Add(pix.PixMaps[0].Texture);
            pix = new PixFile(GameVariables.BasePath + "data\\pixelmap\\number3.pix");
            _countdownTextures.Add(pix.PixMaps[0].Texture);
            pix = new PixFile(GameVariables.BasePath + "data\\pixelmap\\number2.pix");
            _countdownTextures.Add(pix.PixMaps[0].Texture);
            pix = new PixFile(GameVariables.BasePath + "data\\pixelmap\\number1.pix");
            _countdownTextures.Add(pix.PixMaps[0].Texture);
            pix = new PixFile(GameVariables.BasePath + "data\\pixelmap\\go.pix");
            _countdownTextures.Add(pix.PixMaps[0].Texture);

            _countdownSoundIds.Add(8005);
            _countdownSoundIds.Add(8004);
            _countdownSoundIds.Add(8003);
            _countdownSoundIds.Add(8002);
            _countdownSoundIds.Add(8001);
            _countdownSoundIds.Add(8000);

            pix = new PixFile(GameVariables.BasePath + "data\\pixelmap\\timeup.pix");
            _outOfTime = pix.PixMaps[0].Texture;
        }

        public void StartCountdown()
        {
            CountingDown = true;
        }

        public void Update()
        {
            TotalTime += Engine.Instance.ElapsedSeconds;

            if (IsOver) return;

            if (IsStarted)
            {
                TimeRemaining -= Engine.Instance.ElapsedSeconds;
                if (TimeRemaining < 0)
                {
                    TimeRemaining = 0;
                    IsOver = true;
                    SoundCache.Play(SoundIds.OutOfTime);
                }
            }
            if (CountingDown)
            {
                CountdownTime += Engine.Instance.ElapsedSeconds;
                if (CountdownTime > 5)
                {
                    IsStarted = true;
                    ((Driver)Engine.Instance.Player).VehicleModel.Chassis.Motor.Gearbox.CurrentGear = 1;
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
                    MessageRenderer.Instance.PostMessage(_countdownTextures[second], 0.7f, 0.24f, 0.003f);
                }

                _lastSecond = second;
            }            

            if (TimeRemaining == 0)
            {
                MessageRenderer.Instance.PostMessage(_outOfTime, 10, 0.6f, 0.0033f);   
            }
        }
    }
}
