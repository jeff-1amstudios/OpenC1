using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using OneAmEngine;
using Microsoft.Xna.Framework;

namespace OpenC1
{
    enum Fonts 
    {
        White,
        Blue,
        Text,
        Timer,
        Speedo
    }

    class FontRenderer
    {
        static SpriteFont _default;

        static FontRenderer()
        {
            _default = Engine.ContentManager.Load<SpriteFont>("content\\fonts\\Arial_14");
        }

        public static void Render(Fonts font, string text, Vector2 position, Color color)
        {
            Engine.SpriteBatch.DrawString(_default, text, position, color);
        }

        //public static void Render(Fonts font, string text, Vector2 position, Color color, float scale)
        //{

        public static void Render(Fonts font, string text, Vector2 position, Color color, float scale)
        {
            Engine.SpriteBatch.DrawString(_default, text, position, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
    }
}
