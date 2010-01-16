using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.HUD;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;

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

        string _messageText;
        Texture2D _messageTexture;
        float _ttl, _scale, _y;

        public override void Update()
        {
            if (_ttl > 0)
                _ttl -= Engine.Instance.ElapsedSeconds;
            
        }

        public void PostMessage(string message, float displayTime)
        {
        }

        public void PostMessage(Texture2D texture, float displayTime, float y, float scale)
        {
            _messageTexture = texture;
            _ttl = displayTime;
            _scale = scale;
            _y = y;
        }

        public override void Render()
        {
            if (_ttl > 0)
            {
                Engine.Instance.SpriteBatch.Draw(_messageTexture, CenterRectX(_y, _messageTexture.Width * _scale, _messageTexture.Height * _scale), Color.White);
            }            
        }
    }
}
