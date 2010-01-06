using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;
using Microsoft.DirectX;

namespace NFSEngine
{
    public static class TextureGenerator
    {
        public static Texture2D Generate(Color color)
        {
            Texture2D tex = new Texture2D(Engine.Instance.Device, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            Color[] pixels = new Color[1];
            pixels[0] = color;
            tex.SetData<Color>(pixels);
            return tex;
        }

        public static Texture2D Generate(Color color, int x, int y)
        {
            Texture2D tex = new Texture2D(Engine.Instance.Device, x, y, 1, TextureUsage.None, SurfaceFormat.Color);
            Color[] pixels = new Color[x * y];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            tex.SetData<Color>(pixels);
            return tex;
        }
    }
}
