using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;

namespace NFSEngine
{
    public static class TextureGenerator
    {
        public static Texture2D Generate(Color color)
        {
            Texture2D tex = new Texture2D(Engine.Instance.Device, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            tex.SetData<Color>(new Color[] { color });
            return tex;
        }
    }
}
