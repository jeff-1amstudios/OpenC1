using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace OpenC1
{
    class OpponentPathNode
    {
        public int Number;
        public Vector3 Position;
        public List<OpponentPath> Paths = new List<OpponentPath>();
        public float LastUsedTime;
    }
}
