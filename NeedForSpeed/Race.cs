using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.Parsers;
using Carmageddon.Physics;
using StillDesign.PhysX;
using Carmageddon.Parsers.Grooves;
using Carmageddon.Parsers.Funks;

namespace Carmageddon
{
    class Race
    {
        Actor _trackActor;
        DatFile _models;
        ActFile _actors;
        ResourceCache _resourceCache;
        public Texture2D HorizonTexture;
        public RaceTimeController RaceTime = new RaceTimeController();

        public static Race Current;

        public RaceFile RaceFile { get; private set; }

        public Race(string filename)
        {
            RaceFile = new RaceFile(filename);

            _resourceCache = new ResourceCache();

            foreach (string matFileName in RaceFile.MaterialFiles)
            {
                MatFile matFile = new MatFile(@"C:\Games\carma1\data\material\" + matFileName);
                _resourceCache.Add(matFile);
            }

            foreach (string pixFileName in RaceFile.PixFiles)
            {
                PixFile pixFile = new PixFile(@"C:\Games\carma1\data\pixelmap\" + pixFileName);
                _resourceCache.Add(pixFile);
            }

            _resourceCache.Add(new MatFile(@"C:\Games\carma1\data\material\" + "drkcurb.mat"));

            _resourceCache.ResolveMaterials();

            _models = new DatFile(@"C:\Games\carma1\data\models\" + RaceFile.ModelFile);

            _actors = new ActFile(@"C:\Games\carma1\data\actors\" + RaceFile.ActorFile, _models);
            _actors.ResolveHierarchy(false, RaceFile.Grooves);
            _actors.ResolveMaterials(_resourceCache);
            _models.Resolve(_resourceCache);

            // link the actors and grooves
            foreach (BaseGroove g in RaceFile.Grooves)
                g.SetActor(_actors.GetByName(g.ActorName));

            // link the funks and materials
            foreach (BaseFunk f in RaceFile.Funks)
            {
                if (f is FramesFunk) ((FramesFunk)f).Resolve(_resourceCache);
                CMaterial cm = _resourceCache.GetMaterial(f.MaterialName);
                cm.Funk = f;
                f.Material = cm;
            }
            
            if (RaceFile.SkyboxTexture != "none")
            {
                PixFile horizonPix = new PixFile(@"C:\Games\carma1\data\pixelmap\" + RaceFile.SkyboxTexture);
                HorizonTexture = horizonPix.PixMaps[0].Texture;
            }
            GameVariables.DepthCueMode = RaceFile.DepthCueMode;

            _trackActor = Physics.TrackProcessor.GenerateTrackActor(_actors, _models);
            Physics.TrackProcessor.GenerateNonCars(_actors, RaceFile.NonCars, _trackActor);

            Current = this;
        }

        public void SetupPhysx(VehicleChassis vehicle)
        {
            PhysX.Instance.Scene.SetGroupCollisionFlag(10, 1, true);
            PhysX.Instance.Scene.SetActorGroupPairFlags(10, 1, ContactPairFlag.Forces | ContactPairFlag.OnTouch | ContactPairFlag.OnStartTouch);
        }

        public void StartCountdown()
        {
            RaceTime.StartCountdown();
        }

        public void Start()
        {

        }

        public void Update()
        {
            RaceTime.Update();

            foreach (BaseGroove groove in RaceFile.Grooves)
            {
                groove.Update();
            }
            foreach (BaseFunk funk in RaceFile.Funks)
            {
                funk.Update();
            }   
        }

        public void Render()
        {
            _models.SetupRender();
            _actors.Render(_models, Matrix.Identity);
            RaceTime.Render();
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
