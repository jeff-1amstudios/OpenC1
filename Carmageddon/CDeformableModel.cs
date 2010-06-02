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
        Vector3[] _repairPoisitons;
        Vector3[] _originalPositions;
        VertexPositionNormalTexture[] _localVertices;

        public Actor _actor;

        DynamicVertexBuffer _vertexBuffer;
        IndexBuffer _indexBuffer;

        bool _repairing;
        float _repairingFactor;
        bool _changed;
        float _lastCrushTime = 0;

        public CarFile _carFile;

        public override void Resolve(List<ushort> indices, List<VertexPositionNormalTexture> vertices, List<Vector2> vertexTextureMap, List<Vector3> vertexPositions)
        {
            List<int> indices2 = new List<int>();

            foreach (Polygon poly in Polygons)
            {
                poly.NbrPrims = 1;
                indices2.Add(poly.Vertex1); indices2.Add(poly.Vertex2); indices2.Add(poly.Vertex3);
            }


            _originalPositions = new Vector3[VertexCount];
            _repairPoisitons = new Vector3[VertexCount];
            vertexPositions.CopyTo(VertexBaseIndex, _originalPositions, 0, VertexCount);

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

            _localVertices = new VertexPositionNormalTexture[VertexCount];
            for (int i = 0; i < VertexCount; i++)
            {
                Vector3 normal = Polygons[i / 3].Normal;
                if (TextureMapCount > 0)
                    _localVertices[i] = new VertexPositionNormalTexture(vertexPositions[i + VertexBaseIndex], normal, vertexTextureMap[i + VertexBaseIndex]);
                else
                    _localVertices[i] = new VertexPositionNormalTexture(vertexPositions[i + VertexBaseIndex], normal, Vector2.Zero);

                _originalPositions[i] = _localVertices[i].Position;
            }

            int size = VertexPositionNormalTexture.SizeInBytes * _localVertices.Length;
            _vertexBuffer = new DynamicVertexBuffer(Engine.Device, size, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(_localVertices);

            _indexBuffer = new IndexBuffer(Engine.Device, typeof(int), indices2.Count, BufferUsage.WriteOnly);
            _indexBuffer.SetData(indices2.ToArray());

            for (int i = 0; i < indices2.Count / 3; i++)
            {
                Vector3 firstvec = _localVertices[indices2[i * 3 + 1]].Position - _localVertices[indices2[i * 3]].Position;
                Vector3 secondvec = _localVertices[indices2[i * 3]].Position - _localVertices[indices2[i * 3 + 2]].Position;
                Vector3 normal = Vector3.Cross(firstvec, secondvec);
                normal.Normalize();
                VertexPositionNormalTexture vpnt = _localVertices[indices2[i * 3]];
                vpnt.Normal += normal;
                vpnt = _localVertices[indices2[i * 3 + 1]];
                vpnt.Normal += normal;
                vpnt = _localVertices[indices2[i * 3 + 2]];
                vpnt.Normal += normal;
            }
            for (int i = 0; i < vertices.Count; i++)
                vertices[i].Normal.Normalize();

        }

        private Vector3 KeepCrushPositionInBounds(Vector3 pos, BoundingBox box)
        {
            if (pos.Z < box.Min.Z)
                pos.Z = box.Min.Z;
            else if (pos.Z > box.Max.Z)
                pos.Z = box.Max.Z;

            if (pos.X < box.Min.X)
                pos.X = box.Min.X;
            else if (pos.X > box.Max.X)
                pos.X = box.Max.X;

            if (pos.Y < box.Min.Y)
                pos.Y = box.Min.Y;
            else if (pos.Y > box.Max.Y)
                pos.Y = box.Max.Y;

            return pos;
        }

        public void OnContact(Vector3 contactPoint, float force2, Vector3 normal)
        {

            Vector3 force = force2 * normal;
            //GameConsole.WriteEvent("nrml: " + normal.ToShortString());
            //GameConsole.WriteEvent("force: " + force.ToShortString());

            force = Vector3.Transform(force, _actor.GlobalOrientation);

            force *= 0.000000006f; // 0.000000009f * _carFile.CrushSections[1].DamageMultiplier; //scale it down to a managable number
            float forceSize = force.Length();
            
            //if (forceSize < 0.007f)
              //  return;

            if (forceSize > 0.04f)
                force *= 0.04f / forceSize; //cap max force so things dont get loco

            if (forceSize < 0.004f)
                return;

            if (_lastCrushTime + 0.1f > Engine.TotalSeconds) return;
            _lastCrushTime = Engine.TotalSeconds;

                        
            force.X = Math.Abs(force.X);
            force.Y = Math.Abs(force.Y);
            force.Z = Math.Abs(force.Z);


            if (float.IsNaN(Vector3.Normalize(force).X)) return;

            int hitpoints = 0;

            foreach (CrushData data in _carFile.CrushSections[1].Data)
            {
                Vector3 crushPoint = Vector3.Transform(_originalPositions[data.RefVertex], GameVariables.ScaleMatrix * _actor.GlobalPose);
                //Engine.DebugRenderer.AddCube(Matrix.CreateTranslation(crushPoint), Color.Yellow);

                if (Vector3.Distance(crushPoint, contactPoint) < 0.6f)
                {

                    hitpoints++;
                    Vector3 curPos = _localVertices[data.RefVertex].Position;

                    Vector3 directedForce = new Vector3();
                    directedForce.X = curPos.X < 0 ? force.X : -force.X;
                    directedForce.Y = curPos.Y < 0 ? force.Y : -force.Y;
                    directedForce.Z = curPos.Z < 0 ? force.Z : -force.Z;
                    Vector3 oldPos = _localVertices[data.RefVertex].Position;
                    curPos = KeepCrushPositionInBounds(curPos + directedForce, data.Box);
                    float distanceMoved = Vector3.Distance(oldPos, curPos);
                    _localVertices[data.RefVertex].Position = curPos;

                    //GameConsole.WriteEvent("moved: " + distanceMoved);

                    Vector3 rnd = Engine.Random.Next(data.MinScale, data.MaxScale);
                    if (Engine.Random.Next() % 2 == 0) rnd.Y *= 1;

                    Vector3 normalforce = Vector3.Normalize(force);


                    foreach (CrushPoint point in data.Points)
                    {
                        if (float.IsNaN(normalforce.X) || float.IsNaN(normalforce.Y) || float.IsNaN(normalforce.Z))
                        {
                            break;
                        }

                        force = normalforce * distanceMoved * MathHelper.Lerp(0.8f, 0.5f, point.DistanceFromParent);
                        curPos = _localVertices[point.VertexIndex].Position;
                        directedForce.X = curPos.X < 0 ? force.X : -force.X;
                        directedForce.Y = curPos.Y < 0 ? force.Y : -force.Y;
                        directedForce.Z = curPos.Z < 0 ? force.Z : -force.Z;
                        rnd.Y = (_localVertices[data.RefVertex].Position.Y - curPos.Y) * 80;
                        rnd.Y *= curPos.Y > 0 ? 1 : -1;
                        

                        Vector3 forceToUse = directedForce * Vector3.Lerp(Vector3.One, rnd * 1.5f, point.DistanceFromParent); //as we get further away from parent, more random

                        _localVertices[point.VertexIndex].Position = curPos + forceToUse;
                    }
                    _changed = true;
                }
            }

            if (_changed)
            {
                bool tidied = false;
                foreach (CrushData data in _carFile.CrushSections[1].Data)
                {
                    Vector3 oldpos = _localVertices[data.RefVertex].Position;
                    _localVertices[data.RefVertex].Position = KeepCrushPositionInBounds(_localVertices[data.RefVertex].Position, data.Box);
                    if (oldpos != _localVertices[data.RefVertex].Position) tidied = true;
                }

                if (tidied) GameConsole.WriteEvent("tidied up");

                for (int i = 0; i < _localVertices.Length; i++)
                {
                    if (float.IsNaN(_localVertices[i].Position.X) || float.IsNaN(_localVertices[i].Position.Y) || float.IsNaN(_localVertices[i].Position.Z))
                    {
                    }
                }
            }

            if (hitpoints > 0) GameConsole.WriteEvent("f: " + Math.Round(forceSize, 4) + ", pts: " + hitpoints);
        }

        int _nbr = 0;
        public override void Render(CMaterial actorMaterial)
        {
            if (_actor == null) return;

            for (int i = _nbr; i < _carFile.CrushSections[1].Data.Count; i++)
            {
                CrushData data = _carFile.CrushSections[1].Data[i];
                //if (data.RefVertex != 170) continue;

                //Vector3 crushPoint = Vector3.Transform(_originalPositions[data.RefVertex], GameVariables.ScaleMatrix * _actor.GlobalPose);

                //Engine.DebugRenderer.AddWireframeCube(
                //    Matrix.CreateScale(0.09f)
                //    * Matrix.CreateTranslation(crushPoint)
                //    , Color.White);

                //Engine.DebugRenderer.AddAxis(
                //    Matrix.CreateTranslation(Vector3.Transform(data.Box.Max, GameVariables.ScaleMatrix * _actor.GlobalPose))
                //    , 10);

                //Engine.DebugRenderer.AddAxis(
                //    Matrix.CreateTranslation(Vector3.Transform(data.Box.Min, GameVariables.ScaleMatrix * _actor.GlobalPose))
                //    , 10);

                //Engine.DebugRenderer.AddAxis(
                //  Matrix.CreateTranslation(Vector3.Transform(data.V2, GameVariables.ScaleMatrix * _actor.GlobalPose))
                //, 10);

                //foreach (CrushPoint point in data.Points)
                //{
                //    Engine.DebugRenderer.AddWireframeCube(
                //    Matrix.CreateScale(0.05f)
                //    * Matrix.CreateTranslation(Vector3.Transform(_originalPositions[point.VertexIndex], GameVariables.ScaleMatrix * _actor.GlobalPose))

                //    , Color.Yellow);
                //}
                //break;
            }

            if (Engine.Input.WasPressed(Keys.Back))
            {
                if (!_repairing)
                {
                    _repairing = true;
                    _repairingFactor = 0;
                    for (int i = 0; i < _localVertices.Length; i++)
                    {
                        _repairPoisitons[i] = _localVertices[i].Position;
                    }

                    MessageRenderer.Instance.PostMessage("Repair Cost: 0", 2);
                }
            }

            if (Engine.Input.WasPressed(Keys.T))
            {
                for (int i = 0; i < _carFile.CrushSections[1].Data.Count; i++)
                {
                    CrushData data = _carFile.CrushSections[1].Data[i];
                    _localVertices[data.RefVertex].Position = data.Box.Max;
                }
                _changed = true;
            }

            if (_repairing)
            {
                if (_repairingFactor > 1) _repairingFactor = 1;
                for (int i = 0; i < _localVertices.Length; i++)
                {
                    _localVertices[i].Position = Vector3.Lerp(_repairPoisitons[i], _originalPositions[i], _repairingFactor);
                }
                _repairingFactor += Engine.ElapsedSeconds;
                _changed = true;

                if (_repairingFactor >= 1) _repairing = false;
            }

            if (_changed)
            {
                _vertexBuffer.SetData(_localVertices);
                _changed = false;
            }


            GraphicsDevice device = Engine.Device;

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
        }
    }
}