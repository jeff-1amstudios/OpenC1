using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Microsoft.Xna.Framework;

namespace PlatformEngine
{
    public class HeightmapTerrain
    {
        VertexPositionNormalTexture[] vertices;
        private VertexBuffer vb;
        private IndexBuffer ib;

        private int TILES_X = 64;
        private int TILES_Z = 64;

        private int[,] heightData;

        public HeightmapTerrain()
        {
            heightData = new int[TILES_X, TILES_Z];
            SetUpVertices();
            SetUpIndices();
        }

        private void LoadHeightData(string filename)
        {
            heightData = new int[TILES_X, TILES_Z];

            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader r = new BinaryReader(fs);
            for (int i = 0; i < TILES_Z; i++)
            {
                for (int y = 0; y < TILES_X; y++)
                {
                    int height = (int)(r.ReadByte() / 50);
                    heightData[y, TILES_Z - 1 - i] = height;
                }
            }
            r.Close();

        }

        private void SetUpVertices()
        {
            vertices = new VertexPositionNormalTexture[TILES_X * TILES_Z];
            for (int x = 0; x < TILES_X; x++)
            {
                for (int z = 0; z < TILES_Z; z++)
                {
                    vertices[x + z * TILES_X].Position = new Vector3(x, heightData[x, z], z);
                    vertices[x + z * TILES_X].Normal = Vector3.Up;
                    vertices[x + z * TILES_X].TextureCoordinate = new Vector2((float)x / TILES_X * 10, (float)z / TILES_Z * 10);
                }
            }

            vb = new VertexBuffer(Engine.Instance.Device, VertexPositionNormalTexture.SizeInBytes * TILES_X * TILES_Z, BufferUsage.WriteOnly);
            vb.SetData(vertices);
        }

        private void SetUpIndices()
        {
            int[] indices = new int[(TILES_X - 1) * (TILES_Z - 1) * 6];
            for (int x = 0; x < TILES_X - 1; x++)
            {
                for (int y = 0; y < TILES_Z - 1; y++)
                {
                    indices[(x + y * (TILES_X - 1)) * 6] = (x + 1) + (y + 1) * TILES_X;
                    indices[(x + y * (TILES_X - 1)) * 6 + 1] = (x + 1) + y * TILES_X;
                    indices[(x + y * (TILES_X - 1)) * 6 + 2] = x + y * TILES_X;

                    indices[(x + y * (TILES_X - 1)) * 6 + 3] = (x + 1) + (y + 1) * TILES_X;
                    indices[(x + y * (TILES_X - 1)) * 6 + 4] = x + y * TILES_X;
                    indices[(x + y * (TILES_X - 1)) * 6 + 5] = x + (y + 1) * TILES_X;  
                }
            }

            ib = new IndexBuffer(Engine.Instance.Device, typeof(int), (TILES_X - 1) * (TILES_Z - 1) * 6, BufferUsage.WriteOnly);
            ib.SetData(indices);
        }

        public void Draw()
        {
            Engine.Instance.Device.RenderState.CullMode = CullMode.CullClockwiseFace;
            Matrix worldMatrix = Matrix.CreateTranslation(0, 0, -TILES_Z+1);
            worldMatrix *= Matrix.CreateScale(10, 1, 10);
            BasicEffect effect = new BasicEffect(Engine.Instance.Device, null);
            effect.World = worldMatrix;
            effect.View = Engine.Instance.Camera.View;
            effect.Projection = Engine.Instance.Camera.Projection;
            effect.Texture = Engine.Instance.ContentManager.Load<Texture2D>("Content\\Textures\\grass");
            effect.TextureEnabled = true;
            Engine.Instance.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            Engine.Instance.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                Engine.Instance.Device.Vertices[0].SetSource(vb, 0, VertexPositionNormalTexture.SizeInBytes);
                Engine.Instance.Device.Indices = ib;
                Engine.Instance.Device.VertexDeclaration = new VertexDeclaration(Engine.Instance.Device, VertexPositionNormalTexture.VertexElements);
                Engine.Instance.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, TILES_X * TILES_Z, 0, (TILES_X - 1) * (TILES_Z - 1) * 2);
            }
            effect.End();
        }
    }
}
