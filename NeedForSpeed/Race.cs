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
using NFSEngine;

namespace Carmageddon
{
    class Race
    {
        DatFile _models;
        ActFile _actors;
        List<CActor> _nonCars;
        ResourceCache _resourceCache;
        public Texture2D HorizonTexture;
        public RaceTimeController RaceTime = new RaceTimeController();
        bool _started;

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

            Physics.TrackProcessor.GenerateTrackActor(_actors, _models);
            _nonCars = Physics.TrackProcessor.GenerateNonCars(_actors, RaceFile.NonCars);

            Current = this;
        }

        public void SetupPhysx(VehicleChassis vehicle)
        {
            //PhysX.Instance.Scene.SetGroupCollisionFlag(10, 1, true);
            //PhysX.Instance.Scene.SetGroupCollisionFlag(10, 1, true);
            PhysX.Instance.Scene.SetActorGroupPairFlags(10, 1, ContactPairFlag.Forces | ContactPairFlag.OnTouch);
            PhysX.Instance.Scene.SetActorGroupPairFlags(11, 1, ContactPairFlag.Forces | ContactPairFlag.OnTouch);
            PhysX.Instance.Scene.SetActorGroupPairFlags(10, 11, ContactPairFlag.OnTouch);
        }


        public void Start()
        {
            _started = true;
        }

        public void Update()
        {
            RaceTime.Update();

            if (!RaceTime.IsStarted)
            {
                if ((int)RaceTime.TotalTime == 2 && !RaceTime.CountingDown)
                    RaceTime.StartCountdown();

                float height = 55 -(RaceTime.CountdownTime * 35f);
                if (height > 2)
                    ((FixedChaseCamera)Engine.Instance.Camera).HeightOverride = height;
            }

            foreach (BaseGroove groove in RaceFile.Grooves)
            {
                groove.Update();
            }
            foreach (BaseFunk funk in RaceFile.Funks)
            {
                funk.Update();
            }

            foreach (CActor nonCar in _nonCars)
            {
                if (!nonCar.PhysXActor.IsSleeping && nonCar.PhysXActor.LinearVelocity.Length() > 1)
                {
                    _actors.RecalculateActorParent(nonCar);
                }
            }

            MessageRenderer.Instance.Update();
        }

        public void Render()
        {
            _models.SetupRender();
            _actors.Render(_models, Matrix.Identity);
            RaceTime.Render();
            MessageRenderer.Instance.Render();
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
