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
        public static void WriteLine(object o, int row)
        {
            Engine.Instance.GraphicsUtils.AddText(new Vector2(20, row * 18 + 18), o.ToString(), Justify.MIDDLE_LEFT, Color.White);
        }

        public static void WriteLine(Vector3 vec, int row)
        {
            vec.X = (int)vec.X;
            vec.Y = (int)vec.Y;
            vec.Z = (int)vec.Z;
            WriteLine(vec.ToString(), row);
        }


    }
}
