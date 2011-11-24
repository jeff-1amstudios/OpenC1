using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using OpenC1.Parsers.Funks;

using OpenC1.Parsers;
using System.IO;
using OneAmEngine;

namespace OpenC1
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

        public CMaterial()
        {
        }

        public CMaterial(string name, int paletteIndex)
        {
            Name = name;
            SimpMatPixelIndex = paletteIndex;
        }

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
                PixFile pix = new PixFile(PixName);
                if (pix.Exists)
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
