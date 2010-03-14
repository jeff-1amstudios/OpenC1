using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Carmageddon
{
    enum PathType
    {
        General = 0,
        Race = 1,
        CheatOnly = 1000
    }

    class OpponentPath
    {
        public int Number;
        public OpponentPathNode Start, End;
        public float MinSpeedAtEnd, MaxSpeedAtEnd;
        public float Width;
        public PathType Type;
        public bool UserSet = false;
    }
}
