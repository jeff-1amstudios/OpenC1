using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;
using Microsoft.Xna.Framework;

namespace Carmageddon
{
    class ModelShadow
    {
        static VertexDeclaration _vertexDeclaration;

        static ModelShadow()
        {
            _vertexDeclaration = new VertexDeclaration(Engine.Instance.Device, VertexPositionColor.VertexElements);
        }

        public static void Render(Vector3[] points)
        {
            Color shadowColor = new Color(10, 10, 10, 100);
            VertexPositionColor[] verts = new VertexPositionColor[points.Length];
            int i2 = 0;
            for (int i = points.Length-1; i >= 0; i--)
            {
                verts[i2++] = new VertexPositionColor(points[i], shadowColor);
            }

            GraphicsDevice device = Engine.Instance.Device;
            Engine.Instance.Device.RenderState.CullMode = CullMode.None;
            GameVariables.CullingDisabled = true;
            
            GameVariables.CurrentEffect.World = Matrix.Identity;
            GameVariables.CurrentEffect.TextureEnabled = false;
            GameVariables.CurrentEffect.VertexColorEnabled = true;
            VertexDeclaration oldVertDecl = device.VertexDeclaration;
            device.VertexDeclaration = _vertexDeclaration;

            device.RenderState.AlphaBlendEnable = true;
            device.RenderState.AlphaBlendOperation = BlendFunction.Add;
            device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            device.RenderState.SourceBlend = Blend.SourceAlpha;
            device.RenderState.DepthBufferWriteEnable = false;
            //device.RenderState.DepthBufferEnable = false;

            GameVariables.CurrentEffect.CurrentTechnique.Passes[0].Begin();
            device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, verts, 0, 2);
            GameVariables.CurrentEffect.CurrentTechnique.Passes[0].End();

            GameVariables.CurrentEffect.TextureEnabled = true;
            GameVariables.CurrentEffect.VertexColorEnabled = false;
            device.RenderState.AlphaBlendEnable = false;
            //device.RenderState.DepthBufferEnable = true;
            device.RenderState.DepthBufferWriteEnable = true;
            device.VertexDeclaration = oldVertDecl;
        }
    }
}
