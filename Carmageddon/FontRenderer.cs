using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using OneAmEngine;
using Microsoft.Xna.Framework;
using OpenC1.Gfx;
using OpenC1.Parsers;

namespace OpenC1
{
    enum Fonts 
    {
        Blue,
        Text,
        Timer,
        White,
        Gears,
        Speedo
    }

    class FontRenderer
    {
        static SpriteFont _default;
        static bool _useDefaultFont;
        static PixMapFont[] _fonts;

        static FontRenderer()
        {
            _default = Engine.ContentManager.Load<SpriteFont>("content\\fonts\\Arial_14");

            PixMapFont font = new PixMapFont("BLUEHEAD");
            if (font.Exists)
            {
                _fonts = new PixMapFont[6];
                _fonts[0] = font;
                _fonts[1] = new PixMapFont("MEDIUMHD");
                _fonts[2] = new PixMapFont("TIMER");
                _fonts[3] = new PixMapFont("NEWHITE");
                _fonts[4] = new PixMapFont("GEARS", new FontDescriptionFile() { Height = 16, FirstChar=0 });
                _fonts[5] = new PixMapFont("SPEEDO0", new FontDescriptionFile() { Height = 16, FirstChar = 48 });
            }
            else
                _useDefaultFont = true;
        }


        public static void Render(Fonts font, string text, Vector2 position, Color color)
        {
            if (_useDefaultFont)
                Engine.SpriteBatch.DrawString(_default, text, position, color);
            else
                _fonts[(int)font].DrawString(text, position, color, 1);
        }

        public static void Render(Fonts font, string text, Vector2 position, Color color, float scale)
        {
            if (_useDefaultFont)
                Engine.SpriteBatch.DrawString(_default, text, position, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            else
                _fonts[(int)font].DrawString(text, position, color, scale);
        }

        public static void RenderGear(int gear, Vector2 position, Color color, float scale)
        {
            if (_useDefaultFont)
                Engine.SpriteBatch.DrawString(_default, gear.ToString(), position, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            else
                _fonts[(int)Fonts.Gears].DrawChar(gear, position, color, scale);
        }
    }
}
