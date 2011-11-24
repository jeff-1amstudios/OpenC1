using System;
using System.Collections.Generic;
using System.Text;
using OpenC1.Parsers;
using Microsoft.Xna.Framework;
using OneAmEngine;
using OpenC1.Gfx;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace OpenC1
{
	enum EmulationMode
	{
		Demo,
		Full,
		SplatPackDemo,
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
        public static bool CullingOff { get; set; }
        public static Color FogColor;
        public static string BasePath;
        public static BasicEffect2 CurrentEffect;
        public static ParticleEmitter SparksEmitter;
        public static string SelectedCarFileName;
        public static RaceInfo SelectedRaceInfo;
        public static Texture2D SelectedRaceScene;
        public static EmulationMode Emulation;
        public static int SkillLevel = 1;
        public static bool FullScreen;
        public static bool DisableFog;

		public static void DetectEmulationMode()
		{
			if (File.Exists(GameVars.BasePath + "RACES\\CASTLE.TXT") || File.Exists(GameVars.BasePath + "RACES\\TINSEL.TXT"))
			{
				if (!File.Exists(GameVars.BasePath + "NETRACES.TXT"))
					GameVars.Emulation = EmulationMode.SplatPackDemo;
				else
					GameVars.Emulation = EmulationMode.SplatPack;
			}
			else
			{
				if (!File.Exists(GameVars.BasePath + "NETRACES.TXT"))
					GameVars.Emulation = EmulationMode.Demo;
				else
					GameVars.Emulation = EmulationMode.Full;
			}
		}
        
    }
}
