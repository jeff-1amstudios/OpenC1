using System;
using System.Collections.Generic;

using System.Text;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework;

namespace Carmageddon
{
    class Helpers
    {
        public static byte[] GetBytesForImage(byte[] pixels, int width, int height, IPalette palette)
        {
            int overhang = 0;// (4 - ((width * 4) % 4));
            int stride = (width * 4) + overhang;

            byte[] imgData = new byte[stride * height];
            int curPosition = 0;
            for (int i = 0; i < height; i++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte pixel = pixels[width * i + x];

                    if (pixel > 0)
                    {
                        byte[] rgb = palette.GetRGBBytesForPixel(pixel);
                        imgData[curPosition] = rgb[2];
                        imgData[curPosition + 1] = rgb[1];
                        imgData[curPosition + 2] = rgb[0];
                        imgData[curPosition + 3] = 0xFF;
                    }
                    curPosition += 4;
                }
                curPosition += overhang;
            }
            return imgData;
        }

        public static float GetSignedAngleBetweenVectors(Vector3 from, Vector3 to)
        {

            from.Y = to.Y = 0;
            from.Normalize();
            to.Normalize();
            Vector3 toRight = Vector3.Cross(to, Vector3.Up);
            toRight.Normalize();

            float forwardDot = Vector3.Dot(from, to);
            float rightDot = Vector3.Dot(from, toRight);

            // Keep dot in range to prevent rounding errors
            forwardDot = MathHelper.Clamp(forwardDot, -1.0f, 1.0f);

            double angleBetween = Math.Acos(forwardDot);

            if (rightDot < 0.0f)
                angleBetween *= -1.0f;

            return (float)angleBetween;
        }

        public static float GetUnsignedAngleBetweenVectors(Vector3 from, Vector3 to)
        {
            from.Y = to.Y = 0;
            from.Normalize();
            to.Normalize();

            Vector2 a = new Vector2(from.X, from.Z);
            a.Normalize();
            Vector2 b = new Vector2(to.X, to.Z);
            b.Normalize();
            return (float)Math.Acos(Vector2.Dot(a, b));
        }

        public static float UnsignedAngleBetweenTwoV3(Vector3 v1, Vector3 v2)
        {
            //v1.Normalize();
            //v2.Normalize();
            double Angle = (float)Math.Acos(Vector3.Dot(v1, v2));
            return (float)Angle;
        }

        public static Vector3 RotateAroundPoint(Vector3 point, Vector3 originPoint, Vector3 rotationAxis, float radiansToRotate)
        {
            Vector3 diffVect = point - originPoint;

            Vector3 rotatedVect = Vector3.Transform(diffVect, Matrix.CreateFromAxisAngle(rotationAxis, radiansToRotate));

            rotatedVect += originPoint;

            return rotatedVect;
        }

        /// <summary>
        /// 2d only
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Vector3 GetClosestPointOnLine(Vector3 line1, Vector3 line2, Vector3 pos)
        {
            float xDelta = line2.X - line1.X;
            float yDelta = line2.Z - line1.Z;

            float u = ((pos.X - line1.X) * xDelta + (pos.Z - line1.Z) * yDelta) / (xDelta * xDelta + yDelta * yDelta);

            Vector3 closestPoint;
            if (u < 0)
            {
                closestPoint = line1;
            }
            else if (u > 1)
            {
                closestPoint = line2;
            }
            else
            {
                closestPoint = new Vector3(line1.X + u * xDelta, 0, line1.Z + u * yDelta);
            }
            return closestPoint;
        }
    }
}

