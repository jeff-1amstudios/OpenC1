using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.Parsers;
using PlatformEngine;
using Microsoft.Xna.Framework;

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

        private static float offs = 0;

        public void Render(CMaterial actorMaterial)
        {
            GraphicsDevice device = Engine.Instance.Device;
            CMaterial currentMaterial = null;
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

                if (poly.Material != null)
                {
                    if (currentMaterial != poly.Material)
                    {
                        device.Textures[0] = poly.Material.Texture;
                        currentMaterial = poly.Material;
                    }
                }
                else if (actorMaterial != null)
                {
                    if (currentMaterial != actorMaterial)
                    {
                        device.Textures[0] = actorMaterial.Texture;
                        currentMaterial = actorMaterial;
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
                Engine.Instance.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseVert, 0, 3*poly.NbrPrims, indexBufferStart, poly.NbrPrims);
                indexBufferStart += poly.NbrPrims * 3;

                if (currentMaterial != null && currentMaterial.Funk != null)
                {
                    currentMaterial.Funk.AfterRender();
                }
            }
        }
    }
}
