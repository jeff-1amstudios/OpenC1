using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.HUD;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework;
using NFSEngine;

namespace Carmageddon
{
    class MessageRenderer : BaseHUDItem
    {
        private static MessageRenderer _instance;
        public static MessageRenderer Instance
        {
            get
            {
                if (_instance == null) _instance = new MessageRenderer();
                return _instance;
            }
        }

        Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
        string _headerText;
        int _timerSeconds;
        Texture2D _messageTexture;
        float _headerTtl, _messageTtl, _timerTtl, _progress, _scale, _y, _messageX, _startY, _animationSpeed, _pauseTime, _centerX;
        Rectangle _headerRect, _messageRect, _timerRect;
        bool _hasPaused;
        
        int _screenWidth;

        private MessageRenderer()
        {
            _screenWidth = Engine.Window.Width;
        }

        public override void Update()
        {
            if (_headerTtl > 0) _headerTtl -= Engine.ElapsedSeconds;
            if (_animationSpeed == 0 && _messageTtl > 0) _messageTtl -= Engine.ElapsedSeconds;
            if (_timerTtl > 0) _timerTtl -= Engine.ElapsedSeconds;

            if (_animationSpeed > 0)
            {
                if (_pauseTime <= 0)
                {
                    _progress += _animationSpeed * Engine.ElapsedSeconds;
                    _messageX = MathHelper.Lerp(_screenWidth, -300, _progress);
                    if (_messageX < _centerX && !_hasPaused)
                    {
                        _messageX = _centerX;
                        _pauseTime = _messageTtl;
                        _hasPaused = true;
                    }
                }
                else
                {
                    _pauseTime -= Engine.ElapsedSeconds;
                }
            }
        }

        public void PostTimerMessage(int seconds)
        {
            if (_timerTtl <= 0) _timerSeconds = 0;
            _timerTtl = 2;
            _timerSeconds += seconds;
            _timerRect = CenterRectX(0.08f, 0.09f, 0.075f);
        }

        public void PostHeaderMessage(string message, float displayTime)
        {
            _headerText = message;
            _headerTtl = displayTime;
             _headerRect = CenterRectX(0.13f, _headerText.Length * _scale * 7.5f, 50 * _scale);
        }

        public void PostMainMessage(string pixname, float displayTime, float y, float scale, float animationSpeed)
        {
            if (!_textures.ContainsKey(pixname))
            {
                PixFile pix = new PixFile(GameVars.BasePath + "data\\pixelmap\\" + pixname);
                _textures.Add(pixname, pix.PixMaps[0].Texture);
            }

            _hasPaused = false;
            _pauseTime = 0;
            _messageTexture = _textures[pixname];
            _scale = scale;
            _y = y;
            _messageTtl = displayTime;
            _animationSpeed = animationSpeed;
            _progress = 0;
            _messageRect = CenterRectX(_y, _messageTexture.Width * _scale, _messageTexture.Height * _scale);
            _centerX = _messageRect.X;
            _messageX = _screenWidth;
        }


        public override void Render()
        {
            if (_headerTtl > 0)
            {
                DrawString(_textFont, _headerText, new Vector2(_headerRect.Left, _headerRect.Top), Color.White);
            }

            if (_timerTtl > 0)
            {
                TimeSpan ts = TimeSpan.FromSeconds(_timerSeconds);
                DrawString(_blueFont, "+" + ts.Minutes + ":" + ts.Seconds.ToString("00"), new Vector2(_timerRect.Left, _timerRect.Top), Color.White);
            }

            if (_animationSpeed == 0)
            {
                if (_messageTtl > 0)
                    Engine.SpriteBatch.Draw(_messageTexture, _messageRect, Color.White);
            }
            else
            {
                if (_progress < 1)
                {
                    _messageRect.X = (int)_messageX;
                    Engine.SpriteBatch.Draw(_messageTexture, _messageRect, Color.White);
                }
            }
        }
    }
}
