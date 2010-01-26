using System;
using System.Collections.Generic;

using System.Text;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework;
using StillDesign.PhysX;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;

namespace Carmageddon.Physics
{
    class TrackProcessor
    {
        public static Actor GenerateTrackActor(RaceFile file, ActFile actors, DatFile models)
        {
            List<Vector3> verts = new List<Vector3>();
            List<ushort> indices = new List<ushort>();
            List<ushort> materialIndices = new List<ushort>();
            List<Carmageddon.CActor> actorsList = actors.GetAllActors();

            for (int i = 0; i < actorsList.Count; i++)
            {
                Carmageddon.CActor actor = actorsList[i];
                if (actor.Model == null) continue;
                if (actor.Name.StartsWith("&"))
                    continue; //dont-merge with track (non-car, animated etc)

                int baseIndex = verts.Count;
                for (int j = 0; j < actor.Model.VertexCount; j++)
                    verts.Add(Vector3.Zero);

                foreach (Polygon poly in actor.Model.Polygons)
                {
                    string materialName = actor.Model.MaterialNames[poly.MaterialIndex];
                    //this is a non-solid material
                    if (materialName.StartsWith("!"))
                        continue;

                    int index = baseIndex + poly.Vertex1;

                    indices.Add((ushort)index);
                    if (verts[index] == Vector3.Zero)
                    {
                        Vector3 transformedVec = Vector3.Transform(models._vertices[actor.Model.VertexBaseIndex + poly.Vertex1].Position, actor.Matrix);
                        verts[index] = transformedVec;
                    }
                    index = baseIndex + poly.Vertex2;
                    indices.Add((ushort)index);
                    if (verts[index] == Vector3.Zero)
                    {
                        Vector3 transformedVec = Vector3.Transform(models._vertices[actor.Model.VertexBaseIndex + poly.Vertex2].Position, actor.Matrix);
                        verts[index] = transformedVec;
                    }
                    index = baseIndex + poly.Vertex3;
                    indices.Add((ushort)index);
                    if (verts[index] == Vector3.Zero)
                    {
                        Vector3 transformedVec = Vector3.Transform(models._vertices[actor.Model.VertexBaseIndex + poly.Vertex3].Position, actor.Matrix);
                        verts[index] = transformedVec;
                    }

                    if (Char.IsDigit(materialName[0]))
                        materialIndices.Add((ushort)(ushort.Parse(materialName.Substring(0, 1)) + 1));
                    else
                        materialIndices.Add(0);
                }
            }

            TriangleMeshDescription meshDesc = new TriangleMeshDescription();
            meshDesc.TriangleCount = indices.Count / 3;
            meshDesc.VertexCount = verts.Count;
                        
            meshDesc.AllocateVertices<Vector3>(meshDesc.VertexCount);
            meshDesc.AllocateTriangles<ushort>(meshDesc.TriangleCount);
            meshDesc.AllocateMaterialIndices<ushort>(materialIndices.Count);
            
            meshDesc.TriangleStream.SetData(indices.ToArray());
            meshDesc.VerticesStream.SetData(verts.ToArray());
            meshDesc.MaterialIndicesStream.SetData(materialIndices.ToArray());
            meshDesc.Flags = MeshFlag.Indices16Bit;

            MemoryStream s = new MemoryStream();

            Cooking.InitializeCooking();
            Cooking.CookTriangleMesh(meshDesc, s);
            Cooking.CloseCooking();

            s.Position = 0;
            TriangleMesh triangleMesh = PhysX.Instance.Core.CreateTriangleMesh(s);

            TriangleMeshShapeDescription shape = new TriangleMeshShapeDescription()
            {
                TriangleMesh = triangleMesh,
            };

            ActorDescription actorDescription = new ActorDescription()
            {
                GlobalPose = Matrix.CreateTranslation(0, 0, 0),
                Shapes = { shape }
            };


            StillDesign.PhysX.Actor a = PhysX.Instance.Scene.CreateActor(actorDescription);
            a.Group = 10;
            a.Shapes[0].SetFlag(ShapeFlag.Visualization, false);
            return a;
        }

        public static List<CActor> GenerateNonCars(ActFile actors, List<NoncarFile> nonCars)
        {
            List<CActor> nonCarActors = new List<CActor>();
            List<CActor> actorsList = actors.GetAllActors();

            for (int i = 0; i < actorsList.Count; i++)
            {
                Carmageddon.CActor actor = actorsList[i];
                if (actor.Model == null) continue;
                if (actor.Name.StartsWith("&"))
                {
                    if (Char.IsDigit(actor.Name[1]) && Char.IsDigit(actor.Name[2]))
                    {
                        int index = int.Parse(actor.Name.Substring(1, 2));
                        NoncarFile nonCar = nonCars.Find(a => a.IndexNumber == index);

                        if (nonCar == null)
                        {
                            Debug.WriteLine("No noncar matching " + actor.Name);
                            continue;
                        }

                        ActorDescription actorDesc = new ActorDescription();
                        actorDesc.BodyDescription = new BodyDescription() { Mass = nonCar.Mass };

                        BoxShapeDescription boxDesc = new BoxShapeDescription();
                        float w = nonCar.BoundingBox.Max.X - nonCar.BoundingBox.Min.X;
                        float h = nonCar.BoundingBox.Max.Y - nonCar.BoundingBox.Min.Y;
                        float l = nonCar.BoundingBox.Max.Z - nonCar.BoundingBox.Min.Z;
                        boxDesc.Size = new Vector3(w, h, l);
                        boxDesc.LocalPosition = nonCar.BoundingBox.GetCenter();
                        actorDesc.Shapes.Add(boxDesc);

                        foreach (Vector3 extraPoint in nonCar.ExtraBoundingBoxPoints)
                        {
                            boxDesc = new BoxShapeDescription(0.2f, 0.2f, 0.2f);
                            boxDesc.LocalPosition = extraPoint;
                            boxDesc.Mass = 0;
                            actorDesc.Shapes.Add(boxDesc);
                        }

                        Vector3 scaleout, transout;
                        Quaternion b;
                        actor.Matrix.Decompose(out scaleout, out b, out transout);

                        Matrix m =
                            Matrix.CreateFromQuaternion(b) *
                            Matrix.CreateTranslation(transout);

                        StillDesign.PhysX.Actor instance = PhysX.Instance.Scene.CreateActor(actorDesc);
                        instance.GlobalPose = m;
                        instance.SetCenterOfMassOffsetLocalPosition(nonCar.CenterOfMass);
                        instance.Group = 11;
                        instance.UserData = nonCar;
                        
                        if (nonCar.BendAngleBeforeSnapping > 0)
                        {;
                            FixedJointDescription jointDesc = new FixedJointDescription()
                            {
                                Actor1 = instance,
                                Actor2 = null
                            };
                            Vector3 anchorPos = instance.Shapes[0].GlobalPosition;
                            anchorPos.Y = instance.GlobalPosition.Y;
                            jointDesc.SetGlobalAnchor(anchorPos);
                            jointDesc.SetGlobalAxis(new Vector3(0.0f, 1.0f, 0.0f));
                            jointDesc.MaxForce = nonCar.BendAngleBeforeSnapping * 125;
                            FixedJoint joint = (FixedJoint)PhysX.Instance.Scene.CreateJoint(jointDesc);
                            
                            //instance.SolverIterationCount = 128;
                        }
                        instance.Sleep();
                        actor.AttachPhysxActor(instance);
                        nonCarActors.Add(actor);
                    }
                }
            }

            Debug.WriteLine("NonCars: " + nonCarActors.Count);
            return nonCarActors;
        }
    }
}
