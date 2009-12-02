using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;
using Carmageddon.Parsers.Funks;

namespace Carmageddon
{
    class CMaterial
    {
        public string Name { get; set; }
        public string PixName { get; set; }
        public bool DoubleSided { get; set; }
        public int BaseColor { get; set; }
        public Texture2D Texture;
        public BaseFunk Funk;
    }
}
