using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;


namespace Carmageddon.Track
{
    class SkyboxGenerator
    {
        public static SkyBox Generate(Texture2D horizon, float repetionsX, string depthCueMode)
        {
            if (horizon == null)
            {
                Color c;
                if (depthCueMode == "dark")
                    c = new Color(0, 0, 0);
                else if (depthCueMode == "fog" || depthCueMode == "none")
                    c = Color.WhiteSmoke;
                else
                    throw new NotImplementedException();
                horizon = new Texture2D(Engine.Instance.Device, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
                horizon.SetData<Color>(new Color[] { c });  //top left pixel
            }

            Color[] pixels = new Color[horizon.Width * horizon.Height];
            horizon.GetData<Color>(pixels);

            Texture2D topTexture = new Texture2D(Engine.Instance.Device, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            topTexture.SetData<Color>(new Color[] { pixels[0] });  //top left pixel

            Texture2D bottomTexture = new Texture2D(Engine.Instance.Device, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            bottomTexture.SetData<Color>(new Color[] { pixels[pixels.Length - 1] }); //bottom right pixel

            Texture2D sideTexture = new Texture2D(Engine.Instance.Device, horizon.Width, horizon.Height, 1, TextureUsage.None, SurfaceFormat.Color);
            int ptr = 0;
            Color[] flippedPixels = new Color[pixels.Length];
            for (int h = 0; h < horizon.Height; h++)
            {
                for (int w = 0; w < horizon.Width; w++)
                {
                    flippedPixels[ptr] = pixels[h * horizon.Width + (horizon.Width - w) - 1];
                    ptr++;
                }
            }
            sideTexture.SetData<Color>(flippedPixels);

            SkyBox skyBox = new SkyBox(repetionsX);
            skyBox.Textures[0] = horizon;
            skyBox.Textures[1] = horizon;
            skyBox.Textures[2] = bottomTexture;
            skyBox.Textures[3] = topTexture;
            skyBox.Textures[4] = sideTexture;
            skyBox.Textures[5] = sideTexture;
            return skyBox;
        }
    }
}
