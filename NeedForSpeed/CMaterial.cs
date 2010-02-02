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
        public int BasePixel { get; set; }
        public Texture2D Texture;
        public BaseFunk Funk;

        public void ResolveTexture(List<PixMap> pixmaps)
        {
            if (PixName == null)
            {
                //simp mat
                Texture = TextureGenerator.Generate(GameVariables.Palette.GetRGBColorForPixel(BasePixel));
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
                Texture = TextureGenerator.Generate(GameVariables.Palette.GetRGBColorForPixel(BasePixel));
            }
            else
            {
                PixFile pix;
                //if (File.Exists(GameVariables.BasePath + "Data\\Reg\\Pixelmap\\" + PixName))
                //    pix = new PixFile(GameVariables.BasePath + "Data\\Reg\\Pixelmap\\" + PixName);
                //else
                    pix = new PixFile(GameVariables.BasePath + "Data\\Pixelmap\\" + PixName);
                Texture = pix.PixMaps[0].Texture;
            }
        }
    }
}
