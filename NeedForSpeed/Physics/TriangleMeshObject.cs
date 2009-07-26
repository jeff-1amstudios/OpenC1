using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using JigLibX.Geometry;
using JigLibX.Physics;
using JigLibX.Collision;
using Carmageddon.Parsers;
using System.Diagnostics;

namespace Carmageddon.Physics
{
    class TriangleMeshObject : PhysicObject
    {
        TriangleMesh triangleMesh;

        public TriangleMeshObject(Game game, ActFile actors)
            : base(game, null)
        {
            body = new Body();
            collision = new CollisionSkin(null);

            triangleMesh = new TriangleMesh();
            
            List<Vector3> vertexList = new List<Vector3>();
            List<TriangleVertexIndices> indexList = new List<TriangleVertexIndices>();

            foreach (Actor actor in actors.GetAllActors())
            {
                if (actor.Model == null) continue;
                //if (actor.BoundingBox.Max.X == 0) continue;

                foreach (Polygon poly in actor.Model.Polygons)
                {
                    int v0, v1, v2;

                    Vector3 transformedVec = Vector3.Transform(poly.Vertices[0], actor.Matrix);
                    v0 = vertexList.FindIndex(v => v == transformedVec);
                    if (v0 == -1)
                    {
                        vertexList.Add(transformedVec);
                        v0 = vertexList.Count - 1;
                    }
                    transformedVec = Vector3.Transform(poly.Vertices[1], actor.Matrix);
                    v1 = vertexList.FindIndex(v => v == transformedVec);
                    if (v1 == -1)
                    {
                        vertexList.Add(transformedVec);
                        v1 = vertexList.Count - 1;
                    }
                    transformedVec = Vector3.Transform(poly.Vertices[2], actor.Matrix);
                    v2 = vertexList.FindIndex(v => v == transformedVec);
                    if (v2 == -1)
                    {
                        vertexList.Add(transformedVec);
                        v2 = vertexList.Count - 1;
                    }
                    indexList.Add(new TriangleVertexIndices(v0, v1, v2));
                }
            }

            triangleMesh.CreateMesh(vertexList, indexList, 4, 1.0f);
            collision.AddPrimitive(triangleMesh, new MaterialProperties(0.8f, 0.7f, 0.6f));
            PhysicsSystem.CurrentPhysicsSystem.CollisionSystem.AddCollisionSkin(collision);
        }

        public override void ApplyEffects(BasicEffect effect)
        {
            effect.DiffuseColor = Vector3.One * 0.8f;
        }
    }
}
