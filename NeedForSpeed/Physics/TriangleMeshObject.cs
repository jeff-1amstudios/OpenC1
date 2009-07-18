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

            int vIndex=0;
            foreach (Actor actor in actors.GetAllActors())
            {
                if (actor.Model == null) continue;

                foreach (Polygon poly in actor.Model.Polygons)
                {
                    foreach (Vector3 vec in poly.Vertices)
                    {
                        vertexList.Add(Vector3.Transform(vec, actor.Matrix));
                    }
                    //{
                        //vertexList.AddRange(poly.Vertices);
                        
                        indexList.Add(new TriangleVertexIndices(vIndex, vIndex + 1, vIndex + 2));
                        vIndex += 3;
                    //}
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
