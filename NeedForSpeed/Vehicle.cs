using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.Parsers;

namespace Carmageddon
{
    class Vehicle
    {

        DatFile _models;
        ActFile _actors;
        ResourceCache _resourceCache;
        CrushSection _crushSection;

        public Vehicle(string filename)
        {
            CarFile car = new CarFile(filename);

            _resourceCache = new ResourceCache();
            foreach (string pixFileName in car.PixFiles)
            {
                PixFile pixFile = new PixFile(@"C:\Games\carma1\data\pixelmap\" + pixFileName);
                _resourceCache.Add(pixFile);
            }

            foreach (string matFileName in car.MaterialFiles)
            {
                MatFile matFile = new MatFile(@"C:\Games\carma1\data\material\" + matFileName);
                _resourceCache.Add(matFile);
            }

            _models = new DatFile(@"C:\Games\carma1\data\models\" + car.ModelFile);

            _actors = new ActFile(@"C:\Games\carma1\data\actors\" + car.ActorFile, _models);
            _actors.ResolveMaterials(_resourceCache);
            _models.Resolve(_resourceCache);

            _crushSection = car.CrushSections[1];
        }

        public void Update(GameTime gameTime)
        {
        }

        public void Render()
        {
            Engine.Instance.Device.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
            Engine.Instance.Device.SamplerStates[0].AddressV = TextureAddressMode.Clamp;
            //_models.SetupRender();
            BasicEffect effect = _models.SetupRender();
            _models.GetModels()[0].Render(null);
            _actors.Render(_models);

            Engine.Instance.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            Engine.Instance.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            for (int i = 0; i < _crushSection.Data.Count; i++)
            {
                CrushData d = _crushSection.Data[i];
                //Vector3 center = ((d.V2 + d.V1) / 2) + new Vector3(0, 1.0f, 0);
                //Vector3 size = d.V2 - d.V1;
                float dx = MathHelper.Distance(d.V1.X, d.V2.X);
                float dy = MathHelper.Distance(d.V1.Y, d.V2.Y);
                float dz = MathHelper.Distance(d.V1.Z, d.V2.Z);
                //dx=dy=dz=0.03f;
                Vector3 ride = new Vector3(0, 0.11f, 0);
                //Engine.Instance.GraphicsUtils.AddLine(d.V1 + ride, d.V2 + ride, Color.Yellow);
                int baseIdx = _models.GetModels()[0].VertexBaseIndex;
                foreach (CrushPoint pt in d.Points)
                {
                    Vector3 pos = _models._vertices[baseIdx + pt.VertexIndex].Position + ride;
                    Engine.Instance.GraphicsUtils.AddSolidShape(ShapeType.Cube, Matrix.CreateScale(0.01f) * Matrix.CreateTranslation(pos), Color.Yellow, null);
                }
                //Engine.Instance.GraphicsUtils.AddWireframeCube(
                  //  Matrix.CreateScale(new Vector3(dx,dy,dz)) * Matrix.CreateTranslation(d.V1+new Vector3(0, 0.11f, 0)), Color.Yellow);
            }
            //Engine.Instance.GraphicsUtils.AddWireframeCube(Matrix.CreateScale(0.03f) * Matrix.CreateTranslation(0.050065f, 0.011696f + 0.11f, 0.383752f), Color.Yellow);
            //_models.Crush(_crushSection);
        }

        internal void Crush()
        {
            _models.Crush(_crushSection);
        }
    }
}
