using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework;

namespace Carmageddon
{
    class RaceTimeController
    {
        public bool IsStarted, IsOver;
        bool _countdownFinished;
        float _countdownTime;
        int _lastSecond=-1;
        public float TimeRemaining = 20; // 90; //1:30
        List<Texture2D> _countdownTextures = new List<Texture2D>();
        List<int> _countdownSoundIds = new List<int>();
        Texture2D _outOfTime;


        int x;
        int y;

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

            x = Engine.Instance.Window.Width / 2 - 16;
            y = 60;
        }

        public void StartCountdown()
        {   
        }

        public void Update()
        {
            if (IsOver) return;

            if (_countdownFinished)
            {
                TimeRemaining -= Engine.Instance.ElapsedSeconds;
                if (TimeRemaining < 0)
                {
                    TimeRemaining = 0;
                    IsOver = true;
                    SoundCache.Play(SoundIds.OutOfTime);
                }
            }
            else
            {
                _countdownTime += Engine.Instance.ElapsedSeconds;
                if (_countdownTime > 5)
                    IsStarted = true;
                if (_countdownTime > 6)
                    _countdownFinished = true;
            }
        }

        public void Render()
        {
            if (!_countdownFinished)
            {
                int second = (int)_countdownTime;
                if (second > _lastSecond)
                {
                    SoundCache.Play(_countdownSoundIds[second]);
                }
                Engine.Instance.SpriteBatch.Draw(_countdownTextures[second], new Vector2(x, y), Color.White);
                _lastSecond = second;
            }

            if (TimeRemaining == 0)
            {
                Engine.Instance.SpriteBatch.Draw(_outOfTime, new Vector2(x, y), Color.White);
            }
        }
    }
}
