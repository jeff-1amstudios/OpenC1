using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace NeedForSpeed.Parsers
{
    enum PolygonShape
    {
        Unknown = 0,
        Triangle = 0x83,
        Quad = 0x84,
        UnTexturedQuad = 0x8C
    }

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
        private PolygonShape _shape;
        private PolygonType _type;
        List<Vector3> _vertices = new List<Vector3>();
        List<Vector2> _textureCoords = new List<Vector2>();
        private int _vertexBufferIndex;
        private Texture2D _texture;

        public string PartName { get; set; }

        public PolygonShape Shape
        {
            get { return _shape; }
            set { _shape = value; }
        }

        internal PolygonType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string TextureName
        {
            get { return _textureName; }
            set { _textureName = value; }
        }

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

        public Texture2D Texture
        {
            get { return _texture; }
        }

        public int VertexCount
        {
            get
            {
                if (_shape == PolygonShape.Triangle)
                    return 3;
                else
                    return 6;
            }
        }

        public int VertexBufferIndex
        {
            get { return _vertexBufferIndex; }
            set { _vertexBufferIndex = value; }
        }

        public Polygon(PolygonShape type, string part)
        {
            _shape = type;
            PartName = part;
        }

        public void ResolveTexture(BitmapEntry texture)
        {
            _texture = texture.Texture;
            if (_shape == PolygonShape.UnTexturedQuad)
                return;

            for (int i =0; i < _textureCoords.Count; i++)
            {
                Vector2 coord = _textureCoords[i];
                coord.X /= texture.Texture.Width;
                coord.Y /= texture.Texture.Height;
                _textureCoords[i] = coord;
            }
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
