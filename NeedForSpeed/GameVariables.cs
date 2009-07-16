using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework;

namespace Carmageddon
{
    static class GameVariables
    {
        public static PaletteFile Palette { get; set; }
        public static Vector3 Scale = new Vector3(22);
        public static int NbrSectionsRendered = 0;
        public static int NbrSectionsChecked = 0;
        public static int DrawDistance = 2500;
        public static string DepthCueMode = "dark";
    }
}
