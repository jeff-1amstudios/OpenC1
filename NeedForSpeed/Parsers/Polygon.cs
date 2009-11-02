using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Carmageddon.Parsers
{

    class Polygon
    {
        public UInt16 Vertex1 { get; private set; }
        public UInt16 Vertex2 { get; private set; }
        public UInt16 Vertex3 { get; private set; }
        public Vector3 Normal { get; private set; }

        public int MaterialIndex { get; set; }
        public bool DoubleSided { get; set; }
        public Texture2D Texture {get; set; }
        public bool Skip { get; set; }
        public int NbrPrims { get; set; }

        public Polygon(UInt16 v1, UInt16 v2, UInt16 v3)
        {
            Vertex1 = v1;
            Vertex2 = v2;
            Vertex3 = v3;
        }

        public void CalculateNormal(List<Vector3> vertices)
        {
            Vector3 ab = vertices[Vertex1] - vertices[Vertex3];
            Vector3 ac = vertices[Vertex1] - vertices[Vertex2];
            Normal = Vector3.Cross(ab, ac);
            Normal.Normalize();
        }
    }
}
