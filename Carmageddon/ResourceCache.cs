using System;
using System.Collections.Generic;
using Carmageddon.Parsers;

namespace Carmageddon
{
    static class ResourceCache
    {
        static List<PixMap> _pixMaps = new List<PixMap>();
        static List<CMaterial> _materials = new List<CMaterial>();
        static List<FliFile> _fliFiles = new List<FliFile>();

        public static void Add(PixFile pixFile)
        {
            foreach (PixMap pixMap in pixFile.PixMaps)
                _pixMaps.Add(pixMap);
        }

        public static void Add(MatFile matFile)
        {
            foreach (CMaterial material in matFile.Materials)
                _materials.Add(material);
        }

        public static PixMap GetPixelMap(string name)
        {
            return _pixMaps.Find(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public static CMaterial GetMaterial(string name)
        {
            return _materials.Find(m => m.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        // load textures for materials
        public static void ResolveMaterials()
        {
            foreach (CMaterial material in _materials)
            {
                material.ResolveTexture(_pixMaps);
            }
        }

        public static FliFile GetFliFile(string filename)
        {
            FliFile fli = _fliFiles.Find(a=>a.Filename == filename);
            if (fli != null) return fli;
            fli = new FliFile(filename);
            _fliFiles.Add(fli);
            return fli;
        }

        public static void Clear()
        {
            _pixMaps.Clear();
            _materials.Clear();
        }
    }
}