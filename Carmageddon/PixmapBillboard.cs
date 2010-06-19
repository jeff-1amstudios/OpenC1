using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using PlatformEngine;

namespace Carmageddon
{
    class PixmapBillboard
    {
        
        VertexBuffer treeVertexBuffer;
        VertexDeclaration treeVertexDeclaration;
        Effect bbEffect;

        public PixmapBillboard()
        {
            bbEffect = Engine.ContentManager.Load<Effect>("Content/BillboardShader");

            CreateBillboardVerticesFromList(new List<Vector3> { new Vector3(0, 0, 0) });
        }

        private void CreateBillboardVerticesFromList(List<Vector3> treeList)
        {
            VertexPositionTexture[] billboardVertices = new VertexPositionTexture[treeList.Count * 6];
            int i = 0;
            foreach (Vector3 currentV3 in treeList)
            {
                billboardVertices[i++] = new VertexPositionTexture(currentV3, new Vector2(0, 0));
                billboardVertices[i++] = new VertexPositionTexture(currentV3, new Vector2(1, 0));
                billboardVertices[i++] = new VertexPositionTexture(currentV3, new Vector2(1, 1));

                billboardVertices[i++] = new VertexPositionTexture(currentV3, new Vector2(0, 0));
                billboardVertices[i++] = new VertexPositionTexture(currentV3, new Vector2(1, 1));
                billboardVertices[i++] = new VertexPositionTexture(currentV3, new Vector2(0, 1));
            }

            treeVertexBuffer = new VertexBuffer(Engine.Device, billboardVertices.Length * VertexPositionTexture.SizeInBytes, BufferUsage.WriteOnly);
            treeVertexBuffer.SetData(billboardVertices);
            treeVertexDeclaration = new VertexDeclaration(Engine.Device, VertexPositionTexture.VertexElements);
        }

        public void DrawBillboards(Matrix world)
        {
            bbEffect.CurrentTechnique = bbEffect.Techniques["CylBillboard"];
            bbEffect.Parameters["xWorld"].SetValue(Matrix.Identity);
            bbEffect.Parameters["xView"].SetValue(Engine.Camera.View);
            bbEffect.Parameters["xProjection"].SetValue(Engine.Camera.Projection);
            bbEffect.Parameters["xCamPos"].SetValue(Engine.Camera.Position);
            bbEffect.Parameters["xAllowedRotDir"].SetValue(new Vector3(0, 1, 0));
            bbEffect.Parameters["xBillboardTexture"].SetValue(Engine.ContentManager.Load<Texture2D>("Content/damage-smoke"));

            bbEffect.Begin();
            foreach (EffectPass pass in bbEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                Engine.Device.Vertices[0].SetSource(treeVertexBuffer, 0, VertexPositionTexture.SizeInBytes);
                Engine.Device.VertexDeclaration = treeVertexDeclaration;
                int noVertices = treeVertexBuffer.SizeInBytes / VertexPositionTexture.SizeInBytes;
                int noTriangles = noVertices / 3;
                Engine.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, noTriangles);
                pass.End();
            }
            bbEffect.End();
        }
    }


}
