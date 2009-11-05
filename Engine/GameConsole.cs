using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NFSEngine
{
    public static class GameConsole
    {
        static int _lines = 0;

        public static void Clear()
        {
            _lines = 0;
        }

        public static void WriteLine(object o)
        {
            Engine.Instance.DebugRenderer.AddText(new Vector2(20, (_lines++) * 18 + 500), o.ToString(), Justify.MIDDLE_LEFT, Color.White);
        }

        public static void WriteLine(string s, float o)
        {
            WriteLine(s + ": " + Math.Round(o, 2));
        }

        public static void WriteLine(string s, Vector3 vec)
        {
            WriteLine(s + ": " + vec.X.ToString("0.00, ") + vec.Y.ToString("0.00, ") + vec.Z.ToString("0.00"));
        }
    }
}
