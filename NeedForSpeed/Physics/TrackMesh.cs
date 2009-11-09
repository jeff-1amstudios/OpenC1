using System;
using System.Collections.Generic;

using System.Text;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework;
using StillDesign.PhysX;
using System.IO;

namespace Carmageddon.Physics
{
    class TrackMesh
    {
        public static void Generate(ActFile actors, DatFile models)
        {
            List<Vector3> verts = new List<Vector3>();
            List<ushort> indices = new List<ushort>();
            List<Carmageddon.Parsers.Actor> actorsList = actors.GetAllActors();

            for (int i = 0; i < actorsList.Count; i++)
            {
                Carmageddon.Parsers.Actor actor = actorsList[i];
                if (actor.Model == null) continue;
                if (actor.Name.StartsWith("&")) 
                    continue; //dont-merge with track (non-car, animation etc)

                int baseIndex = verts.Count;
                for (int j = 0; j < actor.Model.VertexCount; j++)
                    verts.Add(Vector3.Zero);

                foreach (Polygon poly in actor.Model.Polygons)
                {
                    //this is a non-solid material
                    if (actor.Model.MaterialNames[poly.MaterialIndex].StartsWith("!"))
                        continue;

                    int index = baseIndex + poly.Vertex1;
                    
                    indices.Add((ushort)index);
                    if (verts[index] == Vector3.Zero)
                    {
                        Vector3 transformedVec = Vector3.Transform(models._vertices[actor.Model.VertexBaseIndex+poly.Vertex1].Position, actor.Matrix);
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

                }
            }

            for (int j = verts.Count - 1; j >= 0; j--)
            {
                //if (verts[j] == Vector3.Zero)
                  //  verts.RemoveAt(j);
            }

            TriangleMeshDescription triangleMeshDesc = new TriangleMeshDescription();
            triangleMeshDesc.TriangleCount = indices.Count / 3;
            triangleMeshDesc.VertexCount = verts.Count;
            
            triangleMeshDesc.AllocateVertices<Vector3>(triangleMeshDesc.VertexCount);
            triangleMeshDesc.AllocateTriangles<ushort>(triangleMeshDesc.TriangleCount);
            
            triangleMeshDesc.TriangleStream.SetData(indices.ToArray());
            triangleMeshDesc.VerticesStream.SetData(verts.ToArray());
            triangleMeshDesc.Flags = MeshFlag.Indices16Bit;            

            MemoryStream s = new MemoryStream();

            Cooking.InitializeCooking();
            Cooking.CookTriangleMesh(triangleMeshDesc, s);
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
                Shapes = { shape },
            };

            StillDesign.PhysX.Actor a = PhysX.Instance.Scene.CreateActor(actorDescription);
            a.Shapes[0].SetFlag(ShapeFlag.Visualization, false);
            
        }
    }
}
