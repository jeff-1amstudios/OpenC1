using System;
using System.Collections.Generic;
using System.Text;
using OpenC1.Parsers;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using OneAmEngine;
using System.IO;

namespace OpenC1.Gfx
{
    class PixMapFont : BaseDataFile
    {
        private Texture2D _texture;
        private FontDescriptionFile _description;
        public bool Exists = true;

        public PixMapFont(string name) : this(name, null)
        {
        }

        public PixMapFont(string name, FontDescriptionFile description)
        {
            string path = FindDataFile(name + ".pix");

            PixFile pix = new PixFile(path);
            if (!pix.Exists)
            {
                Exists = false;
                return;
            }

            _texture = pix.PixMaps[0].Texture;

            if (description == null)
            {
                path = path.Replace(".pix", ".txt");
                _description = new FontDescriptionFile(path);
            }
            else
            {
                _description = description;
                _description.CharWidths = new int[100];
                for (int i = 0; i < 100; i++) _description.CharWidths[i] = _texture.Width;
            }
            if (path.Contains("32X20")) _description.Scale = 2;
        }

        public void DrawString(string text, Vector2 position, Color color, float scale)
        {
            scale *= _description.Scale;
            Rectangle srcRet = new Rectangle(0, 0, 0, _description.Height);
            
            foreach (char c in text)
            {
                int charIndex = c - _description.FirstChar;
                srcRet.Width = _description.CharWidths[charIndex];
                srcRet.Y = charIndex * _description.Height;
                Engine.SpriteBatch.Draw(_texture, position, srcRet, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                position.X += ((srcRet.Width + _description.Padding) * scale);
            }
        }

        public void DrawChar(int index, Vector2 position, Color color, float scale)
        {
            Rectangle srcRet = new Rectangle(0, index * _description.Height, _texture.Width, _description.Height);
            Engine.SpriteBatch.Draw(_texture, position, srcRet, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
    }
}
