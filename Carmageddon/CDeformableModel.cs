using System;
using System.Collections.Generic;
using System.Text;
using StillDesign.PhysX;
using Microsoft.Xna.Framework;
using System.IO;
using Carmageddon.Physics;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.Parsers;
using PlatformEngine;

namespace Carmageddon
{
    class CDeformableModel : CModel
    {
        Cloth _deformableBody;

        public Cloth DeformableBody
        {
            get { return _deformableBody; }
        }

        public override void Resolve(List<ushort> indices, List<VertexPositionNormalTexture> vertices, List<Vector2> vertexTextureMap, List<Vector3> vertexPositions)
        {
            List<int> indices2 = new List<int>();
            foreach (Polygon poly in Polygons)
            {
                poly.NbrPrims = 1;
                indices2.Add(poly.Vertex1); indices2.Add(poly.Vertex2); indices2.Add(poly.Vertex3);
            }

            ClothMeshDescription desc = new ClothMeshDescription();
            desc.AllocateVertices<Vector3>(VertexCount);
            desc.AllocateTriangles<int>(indices2.Count / 3);
            desc.VertexCount = VertexCount;
            desc.TriangleCount = indices2.Count / 3;
            
            Vector3[] modelVerts = new Vector3[VertexCount];
            vertexPositions.CopyTo(VertexBaseIndex, modelVerts, 0, VertexCount);

            for (int i=0; i<modelVerts.Length; i++)
            {
                modelVerts[i] = Vector3.Transform(modelVerts[i], GameVariables.ScaleMatrix);
            }

            desc.TriangleStream.SetData(indices2.ToArray());
            desc.VerticesStream.SetData(modelVerts);
            

            //desc.Flags = MeshFlag.Indices16Bit;

            MemoryStream ms = new MemoryStream();
            Cooking.InitializeCooking();
            Cooking.CookClothMesh(desc, ms);
            Cooking.CloseCooking();
            ms.Position = 0;
            ClothMesh mesh = PhysX.Instance.Core.CreateClothMesh(ms);

            ClothDescription clothDesc = new ClothDescription()
            {
                ClothMesh = mesh,
                Flags = ClothFlag.Visualization | ClothFlag.Bending
            };

            /*clothDesc.BendingStiffness = 0.1f,
                DampingCoefficient = 0.9f,
                StretchingStiffness=0.1f,*/
            //clothDesc.SleepLinearVelocity = 50;
            clothDesc.BendingStiffness = 1f;
            clothDesc.DampingCoefficient = 1;
            clothDesc.StretchingStiffness = 1;

            clothDesc.MeshData.AllocatePositions<Vector3>(VertexCount);
            clothDesc.MeshData.AllocateIndices<int>(indices2.Count);
            clothDesc.MeshData.AllocateNormals<Vector3>(VertexCount);

            clothDesc.MeshData.MaximumIndices = indices2.Count;
            clothDesc.MeshData.MaximumVertices = VertexCount;
            clothDesc.MeshData.NumberOfIndices = indices2.Count;
            clothDesc.MeshData.NumberOfVertices = VertexCount;

            _deformableBody = PhysX.Instance.Scene.CreateCloth(clothDesc);
            
        }

        public override void Render(CMaterial actorMaterial)
        {
            return;

            GraphicsDevice device = Engine.Device;

            Matrix world = GameVariables.CurrentEffect.World;
            GameVariables.CurrentEffect.World = Matrix.Identity;
            GameVariables.CurrentEffect.CommitChanges();
            
            
            int n = _deformableBody.MeshData.NumberOfVertices.Value;

            var positions = _deformableBody.MeshData.PositionsStream.GetData<Vector3>();
            var normals = _deformableBody.MeshData.NormalsStream.GetData<Vector3>();
            var indicies = _deformableBody.MeshData.IndicesStream.GetData<int>();

            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[n];

            for (int x = 0; x < n; x++)
            {
                vertices[x].Normal = normals[x];
                vertices[x].Position = positions[x];
            }
            //device.RenderState.FillMode = FillMode.WireFrame;
            VertexBuffer verts = device.Vertices[0].VertexBuffer;
            
            //GameVariables.CurrentEffect.CurrentTechnique.Passes[0].End();
            //GameVariables.CurrentEffect.CurrentTechnique.Passes[0].Begin();
            
            device.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, vertices, 0, n, indicies, 0, indicies.Length / 3);
            device.Vertices[0].SetSource(verts, 0, VertexPositionNormalTexture.SizeInBytes);

            //GameVariables.CurrentEffect.CurrentTechnique.Passes[0].End();
            //GameVariables.CurrentEffect.CurrentTechnique.Passes[0].Begin();

            GameVariables.CurrentEffect.World = world;
            GameVariables.CurrentEffect.CommitChanges();

        }
    }
}
