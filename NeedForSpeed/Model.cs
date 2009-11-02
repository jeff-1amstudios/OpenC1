using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.Parsers;
using PlatformEngine;

namespace Carmageddon
{
    class Model
    {
        public string Name { get; set; }
        public List<string> MaterialNames { get; set; }
        public List<Polygon> Polygons { get; set; }
        public int VertexCount { get; set; }
        public int VertexBaseIndex { get; set; }
        public int IndexBufferStart { get; set; }

        public void Render(Texture2D texture)
        {
            GraphicsDevice device = Engine.Instance.Device;
            Texture2D currentTexture = null;
            int baseVert = VertexBaseIndex;
            int indexBufferStart = IndexBufferStart;

            for (int i = 0; i < Polygons.Count; i++)
            {
                Polygon poly = Polygons[i];
                if (poly.Skip) continue;

                if (GameVariables.CullingDisabled != poly.DoubleSided)
                {
                    device.RenderState.CullMode = (poly.DoubleSided ? CullMode.None : CullMode.CullClockwiseFace);
                    GameVariables.CullingDisabled = poly.DoubleSided;
                }

                if (poly.Texture != null)
                {
                    if (currentTexture != poly.Texture)
                    {
                        device.Textures[0] = poly.Texture;
                        currentTexture = poly.Texture;
                    }
                }
                else
                {
                    if (currentTexture != texture)
                    {
                        device.Textures[0] = texture;
                        currentTexture = texture;
                    }
                }
                GameVariables.NbrDrawCalls++;
                Engine.Instance.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseVert, 0, 3*poly.NbrPrims, indexBufferStart, poly.NbrPrims);
                indexBufferStart += poly.NbrPrims * 3;
            }
        }
    }
}
