using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.Parsers;
using Carmageddon.Physics;

namespace Carmageddon
{
    class Race
    {
        DatFile _models;
        ActFile _actors;
        ResourceCache _resourceCache;
        public Texture2D HorizonTexture;

        public RaceFile RaceFile { get; private set; }

        public Race(string filename)
        {
            RaceFile = new RaceFile(filename);

            _resourceCache = new ResourceCache();

            foreach (string pixFileName in RaceFile.PixFiles)
            {
                PixFile pixFile = new PixFile(@"C:\Games\carma1\data\pixelmap\" + pixFileName);
                _resourceCache.Add(pixFile);
            }

            foreach (string matFileName in RaceFile.MaterialFiles)
            {
                MatFile matFile = new MatFile(@"C:\Games\carma1\data\material\" + matFileName);
                _resourceCache.Add(matFile);
            }
            _resourceCache.Add(new MatFile(@"C:\Games\carma1\data\material\" + "drkcurb.mat"));

            _models = new DatFile(@"C:\Games\carma1\data\models\" + RaceFile.ModelFile);

            _actors = new ActFile(@"C:\Games\carma1\data\actors\" + RaceFile.ActorFile, _models, true);
            _actors.ResolveMaterials(_resourceCache);
            _models.Resolve(_resourceCache);
            
            if (RaceFile.SkyboxTexture != "none")
            {
                PixFile horizonPix = new PixFile(@"C:\Games\carma1\data\pixelmap\" + RaceFile.SkyboxTexture);
                HorizonTexture = horizonPix.PixMaps[0].Texture;
            }
            GameVariables.DepthCueMode = RaceFile.DepthCueMode;


            Carmageddon.Physics.TrackMesh.Generate(_actors, _models);
        }

        public void Update(GameTime gameTime)
        {
        }

        public void Render()
        {
            _actors.Render(_models, Matrix.Identity);
        }

        public ActFile GetTrackActors()
        {
            return _actors;
        }

        public DatFile GetTrackModels()
        {
            return _models;
        }
    }
}
