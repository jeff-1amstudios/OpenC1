using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;
using Microsoft.Xna.Framework;
using Carmageddon.Physics;

namespace Carmageddon
{
    class ModelShadow
    {
        static VertexDeclaration _vertexDeclaration;

        static ModelShadow()
        {
            _vertexDeclaration = new VertexDeclaration(Engine.Device, VertexPositionColor.VertexElements);
        }

        public static void Render(BoundingBox bb, VehicleChassis chassis)
        {
            Vector3[] points = new Vector3[4];

            Matrix pose = chassis.Actor.GlobalPose;
            float shadowWidth = 0.0f;
            Vector3 pos = new Vector3(bb.Min.X - shadowWidth, 0, bb.Min.Z);
            points[0] = Vector3.Transform(pos, pose);
            pos = new Vector3(bb.Max.X + shadowWidth, 0, bb.Min.Z);
            points[1] = Vector3.Transform(pos, pose);
            pos = new Vector3(bb.Min.X - shadowWidth, 0, bb.Max.Z);
            points[2] = Vector3.Transform(pos, pose);
            pos = new Vector3(bb.Max.X + shadowWidth, 0, bb.Max.Z);
            points[3] = Vector3.Transform(pos, pose);

            StillDesign.PhysX.Scene scene = chassis.Actor.Scene;
            Vector3 offset = new Vector3(0, 0.1f, 0);
            for (int i = 0; i < 4; i++)
            {
                StillDesign.PhysX.RaycastHit hit = scene.RaycastClosestShape(
                    new StillDesign.PhysX.Ray(points[i], Vector3.Down), StillDesign.PhysX.ShapesType.Static);
                points[i] = hit.WorldImpact + offset;
            }

            Color shadowColor = new Color(10, 10, 10, 100);
            VertexPositionColor[] verts = new VertexPositionColor[points.Length];
            int i2 = 0;
            for (int i = points.Length-1; i >= 0; i--)
            {
                verts[i2++] = new VertexPositionColor(points[i], shadowColor);
            }

            GraphicsDevice device = Engine.Device;
            CullMode oldCullMode = Engine.Device.RenderState.CullMode;
            Engine.Device.RenderState.CullMode = CullMode.None;
            
            GameVariables.CurrentEffect.World = Matrix.Identity;
            GameVariables.CurrentEffect.Texture = null;
            GameVariables.CurrentEffect.VertexColorEnabled = true;
            VertexDeclaration oldVertDecl = device.VertexDeclaration;
            device.VertexDeclaration = _vertexDeclaration;
            Engine.Device.RenderState.ReferenceAlpha = 0;

            device.RenderState.AlphaBlendEnable = true;
            device.RenderState.AlphaBlendOperation = BlendFunction.Add;
            device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            device.RenderState.SourceBlend = Blend.SourceAlpha;
            device.RenderState.DepthBufferWriteEnable = false;

            GameVariables.CurrentEffect.CurrentTechnique.Passes[0].Begin();
            device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, verts, 0, 2);
            GameVariables.CurrentEffect.CurrentTechnique.Passes[0].End();

            GameVariables.CurrentEffect.VertexColorEnabled = false;
            device.RenderState.AlphaBlendEnable = false;
            device.RenderState.DepthBufferWriteEnable = true;
            device.VertexDeclaration = oldVertDecl;
            Engine.Device.RenderState.CullMode = oldCullMode;

            Engine.Device.RenderState.ReferenceAlpha = 200;
            
        }
    }
}
