using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace OneAmEngine
{
    public interface IPlayer
    {
        Vector3 Position {get; set; }
        Matrix Orientation { get; set; }
        Vector3 Velocity { get; set; }
        void Update();
    }
}
