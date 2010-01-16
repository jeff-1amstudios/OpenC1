using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using NFSEngine;
using Microsoft.Xna.Framework;
using PlatformEngine;

namespace Carmageddon.HUD
{
    abstract class BaseHUDItem
    {
        protected static Texture2D _shadow;
        public abstract void Update();
        public abstract void Render();
        private static Rectangle _window;
        protected static float FontScale = 1f;

        static BaseHUDItem()
        {
            _shadow = TextureGenerator.Generate(new Color(0f, 0f, 0f, 0.6f));
            _window = Engine.Instance.Window;
            FontScale = _window.Width / 800f;
        }

        protected static Rectangle ScaleRect(float x, float y, float width, float height)
        {
            return new Rectangle((int)(x * _window.Width), (int)(y * _window.Height), (int)(width * _window.Width), (int)(height * _window.Height));
        }

        protected static Rectangle CenterRectX(float y, float width, float height)
        {
            Rectangle rect = ScaleRect(0, y, width, height);
            rect.X = _window.Width / 2 - rect.Width / 2;
            return rect;
        }

        protected static Vector2 ScaleVec2(float x, float y)
        {
            return new Vector2(x * _window.Width, y * _window.Height);
        }


    }
}
