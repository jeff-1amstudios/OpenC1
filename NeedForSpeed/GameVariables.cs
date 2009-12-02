using System;
using System.Collections.Generic;

using System.Text;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework;

namespace Carmageddon
{
    static class GameVariables
    {
        public static PaletteFile Palette { get; set; }
        public static Vector3 Scale = new Vector3(6);
        public static Matrix ScaleMatrix = Matrix.CreateScale(Scale);
        public static int NbrSectionsRendered = 0;
        public static int NbrSectionsChecked = 0;
        public static int NbrDrawCalls = 0;
        public static bool CullingDisabled { get; set; }
        
        public static string DepthCueMode = "dark";
        public static int MarkedVert = 158;
        public static string BasePath = "c:\\games\\carma1\\data";
        public static BasicEffect2 CurrentEffect;
    }
}
