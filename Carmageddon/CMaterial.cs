using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;
using Carmageddon.Parsers.Funks;
using NFSEngine;
using Carmageddon.Parsers;
using System.IO;

namespace Carmageddon
{
    class CMaterial
    {
        public string Name { get; set; }
        public string PixName { get; set; }
        public bool DoubleSided { get; set; }
        public int SimpMatPixelIndex { get; set; }
        public int SimpMatGradientCount { get; set; }
        public Texture2D Texture;
        public BaseFunk Funk;

        public void ResolveTexture(List<PixMap> pixmaps)
        {
            if (Texture != null) return;  //weve already resolved this material

            if (PixName == null)
            {
                //simp mat
                if (SimpMatGradientCount >0)
                    GenerateSimpMatGradient();
                else
                    Texture = TextureGenerator.Generate(GameVars.Palette.GetRGBColorForPixel(SimpMatPixelIndex));
            }
            else
            {
                PixMap pixmap = pixmaps.Find(p => p.Name.Equals(PixName, StringComparison.InvariantCultureIgnoreCase));
                if (pixmap != null)
                    Texture = pixmap.Texture;
            }
        }

        public void ResolveTexture()
        {
            if (PixName == null)
            {
                //simp mat
                Texture = TextureGenerator.Generate(GameVars.Palette.GetRGBColorForPixel(SimpMatPixelIndex));
            }
            else
            {
                PixFile pix;
                //if (File.Exists(GameVariables.BasePath + "Data\\Reg\\Pixelmap\\" + PixName))
                //    pix = new PixFile(GameVariables.BasePath + "Data\\Reg\\Pixelmap\\" + PixName);
                //else
                    pix = new PixFile(GameVars.BasePath + "Data\\Pixelmap\\" + PixName);
                Texture = pix.PixMaps[0].Texture;
            }
        }

        private void GenerateSimpMatGradient()
        {
            Texture2D tex = new Texture2D(Engine.Device, 1, SimpMatGradientCount + 1, 1, TextureUsage.None, SurfaceFormat.Color);
            Color[] pixels = new Color[1 * SimpMatGradientCount+1];
            for (int i = 0; i < SimpMatGradientCount+1; i++)
                pixels[i] = GameVars.Palette.GetRGBColorForPixel(SimpMatPixelIndex+i);
            tex.SetData<Color>(pixels);

            Texture = tex;            
        }
    }
}
