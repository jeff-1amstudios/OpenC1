using System;
using System.Collections.Generic;
using System.Text;
using OpenC1.Parsers;
using Microsoft.Xna.Framework;
using OneAmEngine;

namespace OpenC1
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

        public static float GetSignedAngleBetweenVectors(Vector3 from, Vector3 to, bool ignoreY)
        {
            if (ignoreY) from.Y = to.Y = 0;
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

        public static float GetUnsignedAngleBetweenVectors(Vector3 from, Vector3 to, bool ignoreY)
        {
            if (ignoreY) from.Y = to.Y = 0;
            from.Normalize();
            to.Normalize();
            return (float)Math.Acos(Vector3.Dot(from, to));
        }

        public static float UnsignedAngleBetweenTwoV3(Vector3 v1, Vector3 v2)
        {
            //v1.Normalize();
            //v2.Normalize();
            double Angle = (float)Math.Acos(Vector3.Dot(v1, v2));
            return (float)Angle;
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
            Vector3 lineDir = line2 - line1;
            lineDir.Normalize();

            float d = Vector3.Distance(line1, line2);

            Vector3 v1 = pos - line1;
            float t = Vector3.Dot(lineDir, v1);

            if (t <= 0)
                return line1;
            if (t >= d)
                return line2;

            Vector3 v3 = lineDir * t;

            Vector3 closestPoint = line1 + v3;


            //float xDelta = line2.X - line1.X;
            //float yDelta = line2.Z - line1.Z;

            //float u = ((pos.X - line1.X) * xDelta + (pos.Z - line1.Z) * yDelta) / (xDelta * xDelta + yDelta * yDelta);

            //Vector3 closestPoint;
            //if (u < 0)
            //{
            //    closestPoint = line1;
            //}
            //else if (u > 1)
            //{
            //    closestPoint = line2;
            //}
            //else
            //{
            //    closestPoint = new Vector3(line1.X + u * xDelta, 0, line1.Z + u * yDelta);
            //}
            return closestPoint;
        }

        public static bool HasTimePassed(float seconds, float eventTime)
        {
            return eventTime + seconds < Engine.TotalSeconds;
        }
    }
}

