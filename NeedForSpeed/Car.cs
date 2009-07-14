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
    class Car
    {

        DatFile _models;
        ActFile _actors;
        ResourceCache _resourceCache;

        public Car(string filename)
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
        }

        public void Update(GameTime gameTime)
        {
        }

        public void Render()
        {
            Engine.Instance.Device.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
            Engine.Instance.Device.SamplerStates[0].AddressV = TextureAddressMode.Clamp;

            _actors.Render(Matrix.CreateScale(30), _models);

            Engine.Instance.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            Engine.Instance.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
        }
    }
}
