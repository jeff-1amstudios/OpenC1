using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework;
using OneAmEngine;
using Carmageddon.Gfx;
using Microsoft.Xna.Framework.Graphics;

namespace Carmageddon
{
    enum EmulationMode
    {
        Demo,
        Full,
        SplatPack
    }

    static class GameVars
    {
        public static PaletteFile Palette { get; set; }
        public static int DrawDistance;
        public static Vector3 Scale = new Vector3(6, 6, 6);
        public static Matrix ScaleMatrix = Matrix.CreateScale(Scale);
        public static int NbrSectionsRendered = 0;
        public static int NbrSectionsChecked = 0;
        public static int NbrDrawCalls = 0;
        public static bool CullingDisabled { get; set; }
        public static Color FogColor;
        public static string BasePath;
        public static BasicEffect2 CurrentEffect;
        public static ParticleEmitter SparksEmitter;
        public static bool LightingEnabled = true;
        public static string SelectedCarFileName;
        public static RaceInfo SelectedRaceInfo;
        public static Texture2D SelectedRaceScene;
        public static EmulationMode Emulation;
        public static int SkillLevel = 1;
        public static bool FullScreen;
        public static bool DisableFog;
        
    }
}
