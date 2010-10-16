using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace OneAmEngine
{
    public interface ICamera
    {
        Matrix View { get; }
        Matrix Projection { get; }
        float DrawDistance { set; }
        Vector3 Position { get; set; }
        Vector3 Orientation { get; set; }

        void Update();
    }
}
