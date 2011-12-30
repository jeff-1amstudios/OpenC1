using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OneAmEngine;

namespace OpenC1.Screens
{
    interface IMenuOption
    {
        void RenderInSpriteBatch();
        void RenderOutsideSpriteBatch();
		bool CanBeSelected { get; }
    }

    class TextureMenuOption : IMenuOption
    {
        Rectangle _rect;
        Texture2D _texture;

        public TextureMenuOption(Rectangle rect, Texture2D texture)
        {
            _rect = rect;
            _texture = texture;
        }

		public bool CanBeSelected
		{
			get { return true; }
		}

        #region IMenuOption Members

        public void RenderInSpriteBatch()
        {
            Engine.SpriteBatch.Draw(_texture, _rect, Color.White);
        }

        public void RenderOutsideSpriteBatch()
        {
        }

        #endregion
    }
}
