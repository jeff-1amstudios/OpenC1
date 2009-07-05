using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace PlatformEngine
{
    public interface IWorld : IDrawableObject
    {
        float GetHeightAtPoint(Vector3 position);
        void Reset();
    }
}
