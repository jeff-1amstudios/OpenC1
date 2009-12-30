using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using NFSEngine;

namespace Carmageddon.HUD
{
    abstract class BaseHUDItem
    {
        protected static Texture2D _shadow;
        public abstract void Update();
        public abstract void Render();

        static BaseHUDItem()
        {
            _shadow = TextureGenerator.Generate(new Color(0f, 0f, 0f, 0.6f));
        }
    }
}
