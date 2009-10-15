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
            Engine.Instance.GraphicsUtils.AddText(new Vector2(20, (_lines++) * 18 + 500), o.ToString(), Justify.MIDDLE_LEFT, Color.White);
        }

        public static void WriteLine(Vector3 vec)
        {
            vec.X = (float)Math.Round(vec.X,0);
            vec.Y = (float)Math.Round(vec.Y, 0);
            vec.Z = (float)Math.Round(vec.Z, 0);
            WriteLine(vec.ToString());
        }


    }
}
