using System;
using System.Collections.Generic;

using System.Text;
using Carmageddon.Parsers;

namespace Carmageddon
{
    class ResourceCache
    {
        List<PixMap> _pixMaps = new List<PixMap>();
        List<Material> _materials = new List<Material>();

        public void Add(PixFile pixFile)
        {
            foreach (PixMap pixMap in pixFile.PixMaps)
                _pixMaps.Add(pixMap);
        }

        public void Add(MatFile matFile)
        {
            foreach (Material material in matFile.Materials)
                _materials.Add(material);
        }

        public PixMap GetPixelMap(string name)
        {
            return _pixMaps.Find(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public Material GetMaterial(string name)
        {
            return _materials.Find(m => m.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
