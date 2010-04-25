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
using NFSEngine;
using Microsoft.Xna.Framework.Input;

namespace Carmageddon
{
    class CDeformableModel : CModel
    {
        Cloth _deformableBody;
        Vector3[] _originalPositions;
        VertexPositionNormalTexture[] _original;

        public Actor _actor;
        
        Matrix _old = Matrix.Identity;
        DynamicVertexBuffer _vertexBuffer;
        IndexBuffer _indexBuffer;
        FixedJoint _joint;

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

            _originalPositions = new Vector3[VertexCount];
            vertexPositions.CopyTo(VertexBaseIndex, _originalPositions, 0, VertexCount);

            for (int i = 0; i < _originalPositions.Length; i++)
            {
                _originalPositions[i] = Vector3.Transform(_originalPositions[i], GameVariables.ScaleMatrix);
            }

            desc.TriangleStream.SetData(indices2.ToArray());
            desc.VerticesStream.SetData(_originalPositions);

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
                        currentPoly = poly;
                    }
                }
            }

            _original = new VertexPositionNormalTexture[VertexCount];
            for (int i = 0; i < VertexCount; i++)
            {
                Vector3 normal = Polygons[i / 3].Normal;
                if (TextureMapCount > 0)
                    _original[i] = new VertexPositionNormalTexture(vertexPositions[i + VertexBaseIndex], normal, vertexTextureMap[i + VertexBaseIndex]);
                else
                    _original[i] = new VertexPositionNormalTexture(vertexPositions[i + VertexBaseIndex], normal, Vector2.Zero);

                _originalPositions[i] = _original[i].Position;
            }

            int size = VertexPositionNormalTexture.SizeInBytes * _original.Length;
            _vertexBuffer = new DynamicVertexBuffer(Engine.Device, size, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(_original);

            _indexBuffer = new IndexBuffer(Engine.Device, typeof(int), indices2.Count, BufferUsage.WriteOnly);
            _indexBuffer.SetData(indices2.ToArray());

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
            if (_actor == null) return; 


            //if (_old == Matrix.Identity) _old = _actor.GlobalPose;
            var positions = _deformableBody.MeshData.PositionsStream.GetData<Vector3>();
            bool changed = false;
            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 calc = Vector3.Transform(_original[i].Position, _old);
                float distance = Vector3.Distance(calc, positions[i]);
                //if (distance > 10 && distance < 50)
                //{
                    _original[i].Position = positions[i]; // calc;// += ((calc - positions[i]) * 0.1f);
                    changed = true;
                //}
            } 
            if (changed)
                _vertexBuffer.SetData(_original);

            var pos = _deformableBody.GetVertexPosition(0);

            //Vector3 calc = Vector3.Transform(_vec0, _old);
            //GameConsole.WriteLine("crush", Vector3.Distance(calc, pos));
             
            _old = _actor.Shapes[0].GlobalPose;

            if (Engine.Input.WasPressed(Keys.N))
            {
                for (int i = 0; i < _originalPositions.Length; i++)
                    _original[i].Position = _originalPositions[i];
                
                _deformableBody.MeshData.PositionsStream.SetData(_originalPositions);
                _vertexBuffer.SetData(_original);
            }

            if (Engine.Input.WasPressed(Keys.U))
            {
                if (_joint == null)
                {
                    FixedJointDescription jointdesc = new FixedJointDescription() { Actor1 = _actor, Actor2 = null };
                    jointdesc.SetGlobalAxis(new Vector3(0.0f, 1.0f, 0.0f));
                    _joint = PhysX.Instance.Scene.CreateJoint<FixedJoint>(jointdesc);
                }
                _deformableBody.AddDirectedForceAtPosition(pos, Vector3.Backward * 20, 2);
            }


            GraphicsDevice device = Engine.Device;

            Matrix world = GameVariables.CurrentEffect.World;
            GameVariables.CurrentEffect.World = Matrix.Identity;
            GameVariables.CurrentEffect.CommitChanges();

            VertexBuffer verts = device.Vertices[0].VertexBuffer;

            device.Vertices[0].SetSource(_vertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
            device.Indices = _indexBuffer;

            CMaterial currentMaterial = null;
            int baseVert = 0; // VertexBaseIndex;
            int indexBufferStart = 0; // IndexBufferStart;

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

                Engine.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseVert, 0, 3 * poly.NbrPrims, indexBufferStart, poly.NbrPrims);
                indexBufferStart += poly.NbrPrims * 3;

                if (currentMaterial != null && currentMaterial.Funk != null)
                {
                    currentMaterial.Funk.AfterRender();
                }
            }

            device.Vertices[0].SetSource(verts, 0, VertexPositionNormalTexture.SizeInBytes);
            GameVariables.CurrentEffect.World = world;
            GameVariables.CurrentEffect.CommitChanges();
        }
    }
}
