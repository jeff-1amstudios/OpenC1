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
using Carmageddon.Gfx;

namespace Carmageddon
{
    class Race
    {
        DatFile _models;
        ActFile _actors;
        List<CActor> _nonCars;
        ResourceCache _resourceCache;
        public RaceTimeController RaceTime = new RaceTimeController();
        SkyBox _skybox;
        public int NextCheckpoint = 0, CurrentLap;

        public static Race Current;

        public RaceFile ConfigFile { get; private set; }

        public Race(string filename)
        {
            ConfigFile = new RaceFile(filename);

            _resourceCache = new ResourceCache();

            foreach (string matFileName in ConfigFile.MaterialFiles)
            {
                MatFile matFile = new MatFile(@"C:\Games\carma1\data\material\" + matFileName);
                _resourceCache.Add(matFile);
            }

            foreach (string pixFileName in ConfigFile.PixFiles)
            {
                PixFile pixFile = new PixFile(@"C:\Games\carma1\data\pixelmap\" + pixFileName);
                _resourceCache.Add(pixFile);
            }

            _resourceCache.Add(new MatFile(@"C:\Games\carma1\data\material\" + "drkcurb.mat"));

            _resourceCache.ResolveMaterials();

            _models = new DatFile(@"C:\Games\carma1\data\models\" + ConfigFile.ModelFile);

            _actors = new ActFile(@"C:\Games\carma1\data\actors\" + ConfigFile.ActorFile, _models);
            _actors.ResolveHierarchy(false, ConfigFile.Grooves);
            _actors.ResolveMaterials(_resourceCache);
            _models.Resolve(_resourceCache);

            // link the actors and grooves
            foreach (BaseGroove g in ConfigFile.Grooves)
                g.SetActor(_actors.GetByName(g.ActorName));

            // link the funks and materials
            foreach (BaseFunk f in ConfigFile.Funks)
            {
                if (f is FramesFunk) ((FramesFunk)f).Resolve(_resourceCache);
                CMaterial cm = _resourceCache.GetMaterial(f.MaterialName);
                cm.Funk = f;
                f.Material = cm;
            }
            
            if (ConfigFile.SkyboxTexture != "none")
            {
                PixFile horizonPix = new PixFile(GameVariables.BasePath + "data\\pixelmap\\" + ConfigFile.SkyboxTexture);
                _skybox = SkyboxGenerator.Generate(horizonPix.PixMaps[0].Texture, ConfigFile.SkyboxRepetitionsX - 3f, ConfigFile.DepthCueMode);
                _skybox.HeightOffset = -220 + ConfigFile.SkyboxPositionY * 1.5f;
            }

            Physics.TrackProcessor.GenerateTrackActor(ConfigFile, _actors, _models);
            _nonCars = Physics.TrackProcessor.GenerateNonCars(_actors, ConfigFile.NonCars);

            
            GameVariables.SkidMarkBuffer = new Carmageddon.Gfx.SkidMarkBuffer(200);

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


        public void Update()
        {
            RaceTime.Update();

            if (!RaceTime.IsStarted)
            {

                if ((int)RaceTime.TotalTime == 2 && !RaceTime.CountingDown)
                    RaceTime.StartCountdown();
                if (Engine.Instance.Camera is FixedChaseCamera)
                {
                    float height = 55 - (RaceTime.CountdownTime * 35f);
                    ((FixedChaseCamera)Engine.Instance.Camera).HeightOverride = Math.Max(2, height);
                }
            }

            foreach (BaseGroove groove in ConfigFile.Grooves)
            {
                groove.Update();
            }
            foreach (BaseFunk funk in ConfigFile.Funks)
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
            if (_skybox != null) _skybox.Draw();

            _models.SetupRender();
            _actors.Render(_models, Matrix.Identity);

            foreach (SpecialVolume vol in ConfigFile.SpecialVolumes)
            {
                //Engine.Instance.DebugRenderer.AddSquareGrid(vol.Matrix, 20, Color.Yellow); // Matrix.CreateScale(vol.BoundingBox.GetSize()) * Matrix.CreateTranslation(vol.BoundingBox.GetCenter()), Color.Yellow);
            }

            RaceTime.Render();
            MessageRenderer.Instance.Render();
        }

        public void OnCheckpointHit(Checkpoint checkpoint)
        {
            if (checkpoint.Number == NextCheckpoint)
            {
                SoundCache.Play(SoundIds.Checkpoint);
                NextCheckpoint++;
                if (NextCheckpoint == ConfigFile.Checkpoints.Count)
                {
                    CurrentLap++;
                    NextCheckpoint = 0;
                }
                MessageRenderer.Instance.PostMessagePix("checkpnt.pix", 10, 0.7f, 0.003f, 1.4f);
            }
            else
            {   
                SoundCache.Play(SoundIds.WrongCheckpoint);
            }
        }

        public void OnVehicleEnterSpecVol(SpecialVolume volume, VehicleModel vehicle)
        {
            int currentVolumeId = vehicle.CurrentSpecialVolume.Count == 0 ? -1 : vehicle.CurrentSpecialVolume.Peek().Id;
            GameConsole.WriteEvent("Enter specvol - " + vehicle.CurrentSpecialVolume.Count);

            if (currentVolumeId != volume.Id)
            {                
                vehicle.EngineSoundIndex = volume.EngineSoundIndex;
                if (volume.EntrySoundId > 0)
                    SoundCache.Play(volume.EntrySoundId);
            }
            vehicle.CurrentSpecialVolume.Push(volume);
        }

        public void OnVehicleExitSpecVol(SpecialVolume volume, VehicleModel vehicle)
        {
            SpecialVolume vol = vehicle.CurrentSpecialVolume.Pop();
            SpecialVolume nextVol = vehicle.CurrentSpecialVolume.Count == 0 ? null : vehicle.CurrentSpecialVolume.Peek();
            GameConsole.WriteEvent("Exit specvol - " + vehicle.CurrentSpecialVolume.Count);

            if (nextVol == null)
            {
                vehicle.EngineSoundIndex = 0;
                if (volume.ExitSoundId > 0)
                    SoundCache.Play(volume.ExitSoundId);
            }
            else if (nextVol.Id != vol.Id)
            {
                vehicle.EngineSoundIndex = nextVol.EngineSoundIndex;
                if (volume.ExitSoundId > 0)
                    SoundCache.Play(volume.ExitSoundId);
            }
        }
    }
}
