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
            List<Carmageddon.CActor> actorsList = actors.All();

            for (int i = 0; i < actorsList.Count; i++)
            {
                CActor actor = actorsList[i];
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
                        Vector3 transformedVec = Vector3.Transform(models._vertexPositions[actor.Model.VertexBaseIndex + poly.Vertex1], actor.Matrix);
                        verts[index] = transformedVec;
                    }
                    index = baseIndex + poly.Vertex2;
                    indices.Add((ushort)index);
                    if (verts[index] == Vector3.Zero)
                    {
                        Vector3 transformedVec = Vector3.Transform(models._vertexPositions[actor.Model.VertexBaseIndex + poly.Vertex2], actor.Matrix);
                        verts[index] = transformedVec;
                    }
                    index = baseIndex + poly.Vertex3;
                    indices.Add((ushort)index);
                    if (verts[index] == Vector3.Zero)
                    {
                        Vector3 transformedVec = Vector3.Transform(models._vertexPositions[actor.Model.VertexBaseIndex + poly.Vertex3], actor.Matrix);
                        verts[index] = transformedVec;
                    }

                    if (Char.IsDigit(materialName[0]))
                    {
                        ushort matModiferId = (ushort)(ushort.Parse(materialName.Substring(0, 1))+1);
                        if (matModiferId >= file.MaterialModifiers.Count) matModiferId = 0;
                        
                        materialIndices.Add(matModiferId);
                    }
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

            foreach (Checkpoint checkpoint in file.Checkpoints)
            {
                ActorDescription actorDesc = new ActorDescription();
                
                BoxShapeDescription box = new BoxShapeDescription(checkpoint.BBox.GetSize());
                box.Flags = ShapeFlag.TriggerOnEnter | ShapeFlag.Visualization;
                actorDesc.Shapes.Add(box);
                Actor actor = PhysX.Instance.Scene.CreateActor(actorDesc);
                actor.GlobalPosition = checkpoint.BBox.GetCenter();
                actor.UserData = checkpoint;
            }

            StillDesign.PhysX.Actor environment = PhysX.Instance.Scene.CreateActor(actorDescription);
            environment.Group = 10;
            environment.Shapes[0].SetFlag(ShapeFlag.Visualization, false);

            
            CreateDefaultWaterSpecVols(file, actorsList, models);

            
            for (int i = 1; i < file.SpecialVolumes.Count; i++)
            {
                SpecialVolume vol = file.SpecialVolumes[i];

                Vector3 scale = new Vector3();
                Vector3 trans = new Vector3();
                Quaternion q = new Quaternion();
                Matrix matrix = vol.Matrix;
                bool success = matrix.Decompose(out scale, out q, out trans);

                ActorDescription actorDesc = new ActorDescription();
                BoxShapeDescription box = new BoxShapeDescription(scale);

                if (success)
                {
                    box.LocalRotation = Matrix.CreateFromQuaternion(q);
                }
                else
                {
                    //if the matrix cannot be decomposed, like part of the long tunnel in coasta...
                    // get the rotation by calculating some points and working out rotation from them
                    Vector3 v1 = Vector3.Transform(new Vector3(-1, -1, 1), matrix);
                    Vector3 v2 = Vector3.Transform(new Vector3(-1, 1, -1), matrix);
                    Vector3 forwards = v2 - v1;
                    forwards.Normalize();
                    box.LocalRotation = Matrix.CreateWorld(Vector3.Zero, forwards, Vector3.Up);
                }

                box.Flags = ShapeFlag.TriggerOnEnter | ShapeFlag.TriggerOnLeave | ShapeFlag.Visualization;
                actorDesc.Shapes.Add(box);
                Actor actor = PhysX.Instance.Scene.CreateActor(actorDesc);

                actor.GlobalPosition = vol.Matrix.Translation;
                actor.UserData = vol;

                //if (vol.Gravity < 1)
                //{
                //    ForceFieldDescription ffdesc = new ForceFieldDescription();
                //    ffdesc.Constant = new Vector3(0, 11000, 0);

                //    ForceField ff = PhysX.Instance.Scene.CreateForceField(ffdesc);

                //    BoxForceFieldShapeDescription ffshape = new BoxForceFieldShapeDescription();
                //    ForceFieldShape ffshape2 = ff.CreateShape(ffshape);
                //    ffshape2.Pose = vol.Matrix;
                //}
                
            }

            return environment;
        }

        private static void CreateDefaultWaterSpecVols(RaceFile file, List<CActor> actors, DatFile modelsFile)
        {

            for (int i = 0; i < actors.Count; i++)
            {
                CActor actor = actors[i];
                if (actor.Model == null) continue;

                CModel model = actor.Model;
                bool foundWater = false;
                Vector3 min = new Vector3(9999), max = new Vector3(-9999);
                List<Vector3> waterVerts = new List<Vector3>();
                foreach (Polygon poly in model.Polygons)
                {
                    string materialName = model.MaterialNames[poly.MaterialIndex];
                    //this is a non-solid material
                    if (materialName.StartsWith("!"))
                    {
                        foundWater = true;
                        waterVerts.Add(Vector3.Transform(modelsFile._vertexPositions[model.VertexBaseIndex + poly.Vertex1], actor.Matrix));
                        waterVerts.Add(Vector3.Transform(modelsFile._vertexPositions[model.VertexBaseIndex + poly.Vertex2], actor.Matrix));
                        waterVerts.Add(Vector3.Transform(modelsFile._vertexPositions[model.VertexBaseIndex + poly.Vertex3], actor.Matrix));
                    }
                }

                if (foundWater)
                {
                    //add a bottom
                    waterVerts.Add(new Vector3(waterVerts[0].X, waterVerts[0].Y - 6 * GameVariables.Scale.Y, waterVerts[0].Z));

                    BoundingBox bb = BoundingBox.CreateFromPoints(waterVerts);
                    
                    Matrix m = Matrix.CreateScale(bb.GetSize()) * Matrix.CreateTranslation(bb.GetCenter());
                    
                    SpecialVolume vol = file.SpecialVolumes[0].Copy(); //copy default water
                    vol.Matrix = m;
                    file.SpecialVolumes.Add(vol);
                }
            }
        }

        
        public static List<CActor> GenerateNonCars(ActFile actors, List<NoncarFile> nonCars)
        {
            List<CActor> nonCarActors = new List<CActor>();
            List<CActor> actorsList = actors.All();

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
                        boxDesc.Size = nonCar.BoundingBox.GetSize();
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
                            jointDesc.MaxForce = nonCar.BendAngleBeforeSnapping * nonCar.Mass * nonCar.TorqueRequiredToMove * 1.8f;
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
