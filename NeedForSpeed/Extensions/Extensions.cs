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

        public static Vector3 GetSize(this BoundingBox bb)
        {
            float w = Math.Abs(bb.Max.X - bb.Min.X);
            float h = Math.Abs(bb.Max.Y - bb.Min.Y);
            float l = Math.Abs(bb.Max.Z - bb.Min.Z);
            return new Vector3(w, h, l);
        }

        public static Vector2 Abs(this Vector2 vec)
        {
            return new Vector2(Math.Abs(vec.X), Math.Abs(vec.Y));
        }

        public static BoundingBox GetBoundingBox(this Matrix matrix)
        {
            Vector3 topleft = new Vector3(-1, -1, -1);
            Vector3 botRight = new Vector3(1, 1, 1);
            topleft = Vector3.Transform(topleft, matrix);
            botRight = Vector3.Transform(botRight, matrix);

            return new BoundingBox(topleft, botRight);
        }
    }
}
