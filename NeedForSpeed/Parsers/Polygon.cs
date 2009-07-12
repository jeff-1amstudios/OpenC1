using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Carmageddon.Parsers
{

    enum PolygonType
    {
        Normal,
        WheelFrontLeft,
        WheelFrontRight,
        WheelRearRight,
        WheelRearLeft
    }

    class Polygon
    {
        private string _textureName;
        private PolygonType _type;
        List<Vector3> _vertices = new List<Vector3>();
        List<Vector2> _textureCoords = new List<Vector2>();
        private int _vertexBufferIndex;
        private Texture2D _texture;

        public string PartName { get; set; }

        internal PolygonType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string MaterialName {get; set; }

        public List<Vector3> Vertices
        {
            get { return _vertices; }
            set { _vertices = value; }
        }

        public List<Vector2> TextureCoords
        {
            get { return _textureCoords; }
            set { _textureCoords = value; }
        }

        public bool DoubleSided { get; set; }

        public Texture2D Texture {get; set; }

        public int VertexCount
        {
            get
            {
                return 3;
            }
        }

        public int VertexBufferIndex
        {
            get { return _vertexBufferIndex; }
            set { _vertexBufferIndex = value; }
        }
        
        public Polygon(string part)
        {
            PartName = part;
        }

        public List<VertexPositionNormalTexture> GetVertices()
        {
            List<VertexPositionNormalTexture> verts = new List<VertexPositionNormalTexture>();

            Vector3 ab = _vertices[0] - _vertices[2];
            Vector3 ac = _vertices[0] - _vertices[1];
            Vector3 normal = Vector3.Cross(ab, ac);
            normal.Normalize();
            verts.Add(new VertexPositionNormalTexture(_vertices[0], normal, _textureCoords[0]));
            verts.Add(new VertexPositionNormalTexture(_vertices[1], normal, _textureCoords[1]));
            verts.Add(new VertexPositionNormalTexture(_vertices[2], normal, _textureCoords[2]));
            
            return verts;
        }
    }
}
