using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ExtensionAttribute : Attribute
    {
        public ExtensionAttribute() { }
    }
}

namespace Carmageddon
{
    public static class Extensions
    {

        public static Vector3 GetCenter(this BoundingBox bb)
        {
            return 0.5f * (bb.Min + bb.Max);
        }

        public static Vector2 Abs(this Vector2 vec)
        {
            return new Vector2(Math.Abs(vec.X), Math.Abs(vec.Y));
        }
    }
}
