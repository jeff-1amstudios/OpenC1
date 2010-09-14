using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using NFSEngine;

namespace PlatformEngine
{
    public class SkyBox
    {

        Texture2D[] _textures = new Texture2D[6];
        Effect _effect;

        public Texture2D[] Textures
        {
            get { return _textures; }
            set { _textures = value; }
        }
        
        VertexBuffer _vertices;
        IndexBuffer _indices;
        VertexDeclaration _vertexDeclaration;

        Vector3 _cameraPosition;
        Matrix _viewMatrix;
        Matrix _projectionMatrix;
        Matrix _worldMatrix;

        public float HeightOffset { get; set; }

        public SkyBox(float repetionsX)
        {
            _worldMatrix = Matrix.Identity;
            LoadResources(repetionsX);
        }

        public Vector3 CameraPosition
        {
            get { return _cameraPosition; }
            set
            {
                _cameraPosition = value;
                _worldMatrix = Matrix.CreateTranslation(_cameraPosition);
            }
        }

        public Matrix ViewMatrix
        {
            set { _viewMatrix = value; }
            get { return _viewMatrix; }
        }

        public Matrix ProjectionMatrix
        {
            set { _projectionMatrix = value; }
            get { return _projectionMatrix; }
        }

        
        public void LoadResources(float repetionsX)
        {
            _effect = Engine.ContentManager.Load<Effect>("Content\\Skybox\\skybox");
            _vertexDeclaration = new VertexDeclaration(Engine.Device, VertexPositionTexture.VertexElements);
            _vertices = new VertexBuffer(Engine.Device, typeof(VertexPositionTexture), 4 * 6, BufferUsage.WriteOnly);
            VertexPositionTexture[] data = new VertexPositionTexture[4 * 6];

            Vector3 vExtents = new Vector3(200, 125, 200);
            //back
            data[0].Position = new Vector3(vExtents.X, -vExtents.Y, -vExtents.Z);
            data[0].TextureCoordinate.X = repetionsX; data[0].TextureCoordinate.Y = 1.0f;
            data[1].Position = new Vector3(vExtents.X, vExtents.Y, -vExtents.Z);
            data[1].TextureCoordinate.X = repetionsX; data[1].TextureCoordinate.Y = 0.0f;
            data[2].Position = new Vector3(-vExtents.X, vExtents.Y, -vExtents.Z);
            data[2].TextureCoordinate.X = 0.0f; data[2].TextureCoordinate.Y = 0.0f;
            data[3].Position = new Vector3(-vExtents.X, -vExtents.Y, -vExtents.Z);
            data[3].TextureCoordinate.X = 0.0f; data[3].TextureCoordinate.Y = 1.0f;

            //front
            data[4].Position = new Vector3(-vExtents.X, -vExtents.Y, vExtents.Z);
            data[4].TextureCoordinate.X = repetionsX; data[4].TextureCoordinate.Y = 1.0f;
            data[5].Position = new Vector3(-vExtents.X, vExtents.Y, vExtents.Z);
            data[5].TextureCoordinate.X = repetionsX; data[5].TextureCoordinate.Y = 0.0f;
            data[6].Position = new Vector3(vExtents.X, vExtents.Y, vExtents.Z);
            data[6].TextureCoordinate.X = 0.0f; data[6].TextureCoordinate.Y = 0.0f;
            data[7].Position = new Vector3(vExtents.X, -vExtents.Y, vExtents.Z);
            data[7].TextureCoordinate.X = 0.0f; data[7].TextureCoordinate.Y = 1.0f;

            //bottom
            data[8].Position = new Vector3(-vExtents.X, -vExtents.Y, -vExtents.Z);
            data[8].TextureCoordinate.X = repetionsX; data[8].TextureCoordinate.Y = 0.0f;
            data[9].Position = new Vector3(-vExtents.X, -vExtents.Y, vExtents.Z);
            data[9].TextureCoordinate.X = repetionsX; data[9].TextureCoordinate.Y = 1.0f;
            data[10].Position = new Vector3(vExtents.X, -vExtents.Y, vExtents.Z);
            data[10].TextureCoordinate.X = 0.0f; data[10].TextureCoordinate.Y = 1.0f;
            data[11].Position = new Vector3(vExtents.X, -vExtents.Y, -vExtents.Z);
            data[11].TextureCoordinate.X = 0.0f; data[11].TextureCoordinate.Y = 0.0f;

            //top
            data[12].Position = new Vector3(vExtents.X, vExtents.Y, -vExtents.Z);
            data[12].TextureCoordinate.X = 0.0f; data[12].TextureCoordinate.Y = 0.0f;
            data[13].Position = new Vector3(vExtents.X, vExtents.Y, vExtents.Z);
            data[13].TextureCoordinate.X = 0.0f; data[13].TextureCoordinate.Y = 1.0f;
            data[14].Position = new Vector3(-vExtents.X, vExtents.Y, vExtents.Z);
            data[14].TextureCoordinate.X = repetionsX; data[14].TextureCoordinate.Y = 1.0f;
            data[15].Position = new Vector3(-vExtents.X, vExtents.Y, -vExtents.Z);
            data[15].TextureCoordinate.X = repetionsX; data[15].TextureCoordinate.Y = 0.0f;

            //left
            data[16].Position = new Vector3(-vExtents.X, vExtents.Y, -vExtents.Z);
            data[16].TextureCoordinate.X = repetionsX; data[16].TextureCoordinate.Y = 0.0f;
            data[17].Position = new Vector3(-vExtents.X, vExtents.Y, vExtents.Z);
            data[17].TextureCoordinate.X = 0.0f; data[17].TextureCoordinate.Y = 0.0f;
            data[18].Position = new Vector3(-vExtents.X, -vExtents.Y, vExtents.Z);
            data[18].TextureCoordinate.X = 0.0f; data[18].TextureCoordinate.Y = 1.0f;
            data[19].Position = new Vector3(-vExtents.X, -vExtents.Y, -vExtents.Z);
            data[19].TextureCoordinate.X = repetionsX; data[19].TextureCoordinate.Y = 1.0f;

            //right
            data[20].Position = new Vector3(vExtents.X, -vExtents.Y, -vExtents.Z);
            data[20].TextureCoordinate.X = 0.0f; data[20].TextureCoordinate.Y = 1.0f;
            data[21].Position = new Vector3(vExtents.X, -vExtents.Y, vExtents.Z);
            data[21].TextureCoordinate.X = repetionsX; data[21].TextureCoordinate.Y = 1.0f;
            data[22].Position = new Vector3(vExtents.X, vExtents.Y, vExtents.Z);
            data[22].TextureCoordinate.X = repetionsX; data[22].TextureCoordinate.Y = 0.0f;
            data[23].Position = new Vector3(vExtents.X, vExtents.Y, -vExtents.Z);
            data[23].TextureCoordinate.X = 0.0f; data[23].TextureCoordinate.Y = 0.0f;

            _vertices.SetData<VertexPositionTexture>(data);


            _indices = new IndexBuffer(Engine.Device, typeof(short), 6 * 6, BufferUsage.WriteOnly);

            short[] ib = new short[6 * 6];

            for (int x = 0; x < 6; x++)
            {
                ib[x * 6 + 0] = (short)(x * 4 + 0);
                ib[x * 6 + 2] = (short)(x * 4 + 1);
                ib[x * 6 + 1] = (short)(x * 4 + 2);

                ib[x * 6 + 3] = (short)(x * 4 + 2);
                ib[x * 6 + 5] = (short)(x * 4 + 3);
                ib[x * 6 + 4] = (short)(x * 4 + 0);
            }

            _indices.SetData<short>(ib);
        }


        public void Draw()
        {
            if (_vertices == null)
                return;

            CameraPosition = Engine.Camera.Position + new Vector3(0, HeightOffset, 0);
            ProjectionMatrix = Engine.Camera.Projection;
            ViewMatrix = Engine.Camera.View;


            _effect.Begin(SaveStateMode.SaveState);
            _effect.Parameters["worldViewProjection"].SetValue(_worldMatrix * _viewMatrix * _projectionMatrix);

            GraphicsDevice device = Engine.Device;

            bool fogEnabled = device.RenderState.FogEnable;
            device.RenderState.FogEnable = false;

            device.RenderState.DepthBufferWriteEnable = false;
            device.VertexDeclaration = _vertexDeclaration;
            device.Vertices[0].SetSource(_vertices, 0, VertexPositionTexture.SizeInBytes);
            device.Indices = _indices;

            for (int x = 0; x < 6; x++)
            {
                _effect.Parameters["baseTexture"].SetValue(_textures[x]);

                _effect.Techniques[0].Passes[0].Begin();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, x * 4, 4, x * 6, 2);
                _effect.Techniques[0].Passes[0].End();
            }

            _effect.End();

            device.RenderState.DepthBufferWriteEnable = true;
            device.RenderState.FogEnable = fogEnabled;
        }
    }
}
