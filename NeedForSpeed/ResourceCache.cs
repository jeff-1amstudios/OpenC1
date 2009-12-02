using System;
using System.Collections.Generic;

using System.Text;
using Carmageddon.Parsers;
using NFSEngine;

namespace Carmageddon
{
    class ResourceCache
    {
        List<PixMap> _pixMaps = new List<PixMap>();
        List<CMaterial> _materials = new List<CMaterial>();

        public void Add(PixFile pixFile)
        {
            foreach (PixMap pixMap in pixFile.PixMaps)
                _pixMaps.Add(pixMap);
        }

        public void Add(MatFile matFile)
        {
            foreach (CMaterial material in matFile.Materials)
                _materials.Add(material);
        }

        public PixMap GetPixelMap(string name)
        {
            return _pixMaps.Find(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public CMaterial GetMaterial(string name)
        {
            return _materials.Find(m => m.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        // load textures for materials
        public void ResolveMaterials()
        {
            foreach (CMaterial material in _materials)
            {
                if (material.PixName == null)
                {
                    //simp mat
                    material.Texture = TextureGenerator.Generate(GameVariables.Palette.GetRGBColorForPixel(material.BaseColor));
                }
                else
                {
                    PixMap pixmap = GetPixelMap(material.PixName);
                    if (pixmap != null)
                        material.Texture = pixmap.Texture;
                }
            }
        }
    }
}
