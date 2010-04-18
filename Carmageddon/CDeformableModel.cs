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
        List<short> _localIndices = new List<short>();
        List<VertexPositionNormalTexture> _localVerts = new List<VertexPositionNormalTexture>();

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

            for (int i = 0; i < modelVerts.Length; i++)
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
                Flags = ClothFlag.Visualization | ClothFlag.Bending | ClothFlag.SelfCollision
            };

            /*clothDesc.BendingStiffness = 0.1f,
                DampingCoefficient = 0.9f,
                StretchingStiffness=0.1f,*/
            //clothDesc.SleepLinearVelocity = 50;

            //clothDesc.BendingStiffness = 1f;
            //clothDesc.DampingCoefficient = 1;
            //clothDesc.StretchingStiffness = 1;

            clothDesc.MeshData.AllocatePositions<Vector3>(VertexCount);
            clothDesc.MeshData.AllocateIndices<int>(indices2.Count);

            clothDesc.MeshData.MaximumIndices = indices2.Count;
            clothDesc.MeshData.MaximumVertices = VertexCount;
            clothDesc.MeshData.NumberOfIndices = indices2.Count;
            clothDesc.MeshData.NumberOfVertices = VertexCount;

            _deformableBody = PhysX.Instance.Scene.CreateCloth(clothDesc);

            Polygon currentPoly = null;

            foreach (Polygon poly in Polygons)
            {
                poly.NbrPrims = 1;
                _localIndices.Add((short)poly.Vertex1); _localIndices.Add((short)poly.Vertex2); _localIndices.Add((short)poly.Vertex3);


                if (poly.MaterialIndex >= 0 && MaterialNames != null)
                {
                    CMaterial material = ResourceCache.GetMaterial(MaterialNames[poly.MaterialIndex]);

                    if (material != null)
                    {
                        poly.DoubleSided = material.DoubleSided;
                        poly.Material = material;
                    }

                    if (currentPoly != null && poly.MaterialIndex == currentPoly.MaterialIndex)
                    {
                        poly.Skip = true;
                        currentPoly.NbrPrims = currentPoly.NbrPrims + 1;
                    }
                    else
                    {
                        //currentPoly = poly;
                    }
                }
            }

            for (int i = 0; i < VertexCount; i++)
            {
                Vector3 normal = Polygons[i / 3].Normal;
                if (TextureMapCount > 0)
                    _localVerts.Add(new VertexPositionNormalTexture(vertexPositions[i + VertexBaseIndex], normal, vertexTextureMap[i + VertexBaseIndex]));
                else
                    _localVerts.Add(new VertexPositionNormalTexture(vertexPositions[i + VertexBaseIndex], normal, Vector2.Zero));
            }

            //for (int i = 0; i < _localIndices.Count / 3; i++)
            //{
            //    Vector3 firstvec = _localVerts[indices[i * 3 + 1]].Position - _localVerts[indices[i * 3]].Position;
            //    Vector3 secondvec = _localVerts[indices[i * 3]].Position - _localVerts[indices[i * 3 + 2]].Position;
            //    Vector3 normal = Vector3.Cross(firstvec, secondvec);
            //    normal.Normalize();
            //    VertexPositionNormalTexture vpnt = vertices[indices[i * 3]];
            //    vpnt.Normal += normal;
            //    vpnt = vertices[indices[i * 3 + 1]];
            //    vpnt.Normal += normal;
            //    vpnt = vertices[indices[i * 3 + 2]];
            //    vpnt.Normal += normal;
            //}
            //for (int i = 0; i < vertices.Count; i++)
            //    vertices[i].Normal.Normalize();


        }

        

        public override void Render(CMaterial actorMaterial)
        {
            Render2();

            return;

            GraphicsDevice device = Engine.Device;

            Matrix world = GameVariables.CurrentEffect.World;
            GameVariables.CurrentEffect.World = Matrix.Identity;
            GameVariables.CurrentEffect.CommitChanges();


            var pos = _deformableBody.GetVertexPosition(0);
            
            int n = _deformableBody.MeshData.NumberOfVertices.Value;

            

            var positions = _deformableBody.MeshData.PositionsStream.GetData<Vector3>();
            var indicies = _deformableBody.MeshData.IndicesStream.GetData<int>();

            VertexPositionTexture[] vertices = new VertexPositionTexture[n];

            for (int x = 0; x < n; x++)
            {
                vertices[x].Position = positions[x];
            }
            //device.RenderState.FillMode = FillMode.WireFrame;
            VertexBuffer verts = device.Vertices[0].VertexBuffer;
            
            //GameVariables.CurrentEffect.CurrentTechnique.Passes[0].End();
            //GameVariables.CurrentEffect.CurrentTechnique.Passes[0].Begin();
            
            device.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, vertices, 0, n, indicies, 0, indicies.Length / 3);
            device.Vertices[0].SetSource(verts, 0, VertexPositionNormalTexture.SizeInBytes);

            //GameVariables.CurrentEffect.CurrentTechnique.Passes[0].End();
            //GameVariables.CurrentEffect.CurrentTechnique.Passes[0].Begin();

            GameVariables.CurrentEffect.World = world;
            GameVariables.CurrentEffect.CommitChanges();

        }

        private void Render2()
        {
            GraphicsDevice device = Engine.Device;

            CMaterial currentMaterial = null;
            int baseVert = 0;
            int indexBufferStart = 0;

            for (int i = 0; i < Polygons.Count; i++)
            {
                Polygon poly = Polygons[i];
                if (poly.Skip) continue;

                if (GameVariables.CullingDisabled != poly.DoubleSided)
                {
                    device.RenderState.CullMode = (poly.DoubleSided ? CullMode.None : CullMode.CullClockwiseFace);
                    GameVariables.CullingDisabled = poly.DoubleSided;
                }


                if (poly.Material != null)
                {
                    if (currentMaterial != poly.Material)
                    {
                        device.Textures[0] = poly.Material.Texture;
                        currentMaterial = poly.Material;
                    }
                }
                else
                {
                    device.Textures[0] = null; currentMaterial = null;
                }

                if (currentMaterial != null && currentMaterial.Funk != null)
                {
                    currentMaterial.Funk.BeforeRender();
                }
                GameVariables.NbrDrawCalls++;

                Engine.Device.DrawUserIndexedPrimitives < VertexPositionNormalTexture>(PrimitiveType.TriangleList, _localVerts.ToArray(), 0, 3 * poly.NbrPrims, _localIndices.ToArray(), indexBufferStart, poly.NbrPrims);
                
                indexBufferStart += poly.NbrPrims * 3;

                if (currentMaterial != null && currentMaterial.Funk != null)
                {
                    currentMaterial.Funk.AfterRender();
                }
            }
        }
    }
}
