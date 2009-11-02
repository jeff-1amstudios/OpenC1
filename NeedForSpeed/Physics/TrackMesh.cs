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
            //Vector3[] verts = new Vector3[models._vertices.Count()];
            //for (int i = 0; i < verts.Length; i++)
              //  verts[i] = models._vertices[i].Position;

            List<Vector3> verts = new List<Vector3>();
            List<ushort> indices = new List<ushort>();
            List<Carmageddon.Parsers.Actor> actorsList = actors.GetAllActors();

            int maxactor = 1200;

            for (int i = 0; i < actorsList.Count; i++)
            {
                Carmageddon.Parsers.Actor actor = actorsList[i];
                if (actor.Model == null) continue;

                //verts = new List<Vector3>(actor.Model.VertexCount);
                for (int j = 0; j < actor.Model.VertexCount; j++)
                    verts.Add(Vector3.Zero);

                foreach (Polygon poly in actor.Model.Polygons)
                {
                    int index = actor.Model.VertexBaseIndex + poly.Vertex1;
                    indices.Add((ushort)index);
                    if (verts[index] == Vector3.Zero)
                    {
                        Vector3 transformedVec = Vector3.Transform(models._vertices[index].Position, actor.Matrix);
                        verts[index] = transformedVec;
                    }
                    index = actor.Model.VertexBaseIndex + poly.Vertex2;
                    indices.Add((ushort)index);
                    if (verts[index] == Vector3.Zero)
                    {
                        Vector3 transformedVec = Vector3.Transform(models._vertices[index].Position, actor.Matrix);
                        verts[index] = transformedVec;
                    }
                    index = actor.Model.VertexBaseIndex + poly.Vertex3;
                    indices.Add((ushort)index);
                    if (verts[index] == Vector3.Zero)
                    {
                        Vector3 transformedVec = Vector3.Transform(models._vertices[index].Position, actor.Matrix);
                        verts[index] = transformedVec;
                    }

                }
            }

            //PhysX wants indices as ints not ushorts...
            //int[] indices = new int[models._indicies.Count];
            //for (int i=0; i < models._indicies.Count; i++)
             //   indices[i] = models._indicies[i];

            //verts = new Vector3[]
            //    {
            //        new Vector3( -100, 5, -100 ),
            //        new Vector3( -100, 5, 100 ),
            //        new Vector3( 100, 5, -100 ),
            //        new Vector3( 100, 5, 100 ),
            //    };

            //indices = new int[]
            //    {
            //        0, 1, 2,
            //        1, 3, 2
            //    };

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
