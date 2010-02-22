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
        string _messageText;
        Texture2D _messageTexture;
        float _ttl, _progress, _scale, _y, _x, _startY, _animationSpeed, _pauseTime, _centerX;
        Rectangle _rect;
        bool _hasPaused;
        
        int _screenWidth;

        private MessageRenderer()
        {
            _screenWidth = Engine.Instance.Window.Width;
        }

        public override void Update()
        {
            if (_ttl > 0)
                _ttl -= Engine.Instance.ElapsedSeconds;

            if (_animationSpeed > 0)
            {
                if (_pauseTime <= 0)
                {
                    _progress += _animationSpeed * Engine.Instance.ElapsedSeconds;
                    _x = MathHelper.Lerp(_screenWidth, -300, _progress);
                    if (_x < _centerX && !_hasPaused)
                    {
                        _x = _centerX;
                        _pauseTime = 0.5f;
                        _hasPaused = true;
                    }
                }
                else
                {
                    _pauseTime -= Engine.Instance.ElapsedSeconds;
                }
            }
        }

        public void PostMessage(string message, float displayTime)
        {
        }


        public void PostMessagePix(string pixname, float displayTime, float y, float scale, float animationSpeed)
        {
            if (!_textures.ContainsKey(pixname))
            {
                PixFile pix = new PixFile(GameVariables.BasePath + "data\\pixelmap\\" + pixname);
                _textures.Add(pixname, pix.PixMaps[0].Texture);
            }

            _hasPaused = false;
            _pauseTime = 0;
            _messageTexture = _textures[pixname];
            _scale = scale;
            _y = y;
            _ttl = displayTime;
            _animationSpeed = animationSpeed;
            _progress = 0;
            _rect = CenterRectX(_y, _messageTexture.Width * _scale, _messageTexture.Height * _scale);
            _centerX = _rect.X;
            _x = _screenWidth;

        }


        public override void Render()
        {
            if (_animationSpeed == 0)
            {
                if (_ttl > 0)
                    Engine.Instance.SpriteBatch.Draw(_messageTexture, _rect, Color.White);
            }
            else
            {
                if (_progress < 1)
                {
                    _rect.X = (int)_x;
                    Engine.Instance.SpriteBatch.Draw(_messageTexture, _rect, Color.White);
                }
            }
        }
    }
}
