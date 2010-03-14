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
using Microsoft.Xna.Framework.Input;

namespace Carmageddon
{
    class Race
    {
        DatFile _models;
        ActFile _actors;
        List<CActor> _nonCars;
        public RaceTimeController RaceTime = new RaceTimeController();
        SkyBox _skybox;
        public int NextCheckpoint = 0, CurrentLap;
        public Vehicle PlayerVehicle;
        public List<Opponent> Opponents = new List<Opponent>();
        public List<IDriver> Drivers = new List<IDriver>(); //opponent + player drivers

        public static Race Current;

        public RaceFile ConfigFile { get; private set; }

        public Race(string filename, string playerVehicleFile)
        {
            
            ConfigFile = new RaceFile(filename);

            foreach (string matFileName in ConfigFile.MaterialFiles)
            {
                MatFile matFile = new MatFile(@"C:\Games\carma1\data\material\" + matFileName);
                ResourceCache.Add(matFile);
            }

            foreach (string pixFileName in ConfigFile.PixFiles)
            {
                PixFile pixFile = new PixFile(@"C:\Games\carma1\data\pixelmap\" + pixFileName);
                ResourceCache.Add(pixFile);
            }

            ResourceCache.Add(new MatFile(@"C:\Games\carma1\data\material\" + "drkcurb.mat"));

            ResourceCache.ResolveMaterials();

            _models = new DatFile(@"C:\Games\carma1\data\models\" + ConfigFile.ModelFile);

            _actors = new ActFile(@"C:\Games\carma1\data\actors\" + ConfigFile.ActorFile, _models);
            _actors.ResolveHierarchy(false, ConfigFile.Grooves);
            _actors.ResolveMaterials();
            _models.Resolve();

            // link the actors and grooves
            foreach (BaseGroove g in ConfigFile.Grooves)
                g.SetActor(_actors.GetByName(g.ActorName));

            // link the funks and materials
            foreach (BaseFunk f in ConfigFile.Funks)
            {
                f.Resolve();
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

            PhysX.Instance.Scene.SetActorGroupPairFlags(10, 1, ContactPairFlag.Forces | ContactPairFlag.OnTouch);
            PhysX.Instance.Scene.SetActorGroupPairFlags(11, 1, ContactPairFlag.Forces | ContactPairFlag.OnTouch);
            PhysX.Instance.Scene.SetActorGroupPairFlags(10, 11, ContactPairFlag.OnTouch);


            Opponents.Add(new Opponent("tassle.txt", ConfigFile.GridPosition, ConfigFile.GridDirection));
            Opponents.Add(new Opponent("ivan.txt", ConfigFile.GridPosition, ConfigFile.GridDirection));
            Opponents.Add(new Opponent("screwie.txt", ConfigFile.GridPosition, ConfigFile.GridDirection));
            Opponents.Add(new Opponent("kutter.txt", ConfigFile.GridPosition, ConfigFile.GridDirection));
            Opponents.Add(new Opponent("dump.txt", ConfigFile.GridPosition, ConfigFile.GridDirection));

            foreach (Opponent o in Opponents) Drivers.Add(o.Driver);
                        
            OpponentController.Nodes = ConfigFile.OpponentPathNodes;

            PlayerVehicle = new Vehicle(GameVariables.BasePath + @"data\cars\" + playerVehicleFile, new PlayerDriver());
            PlayerVehicle.SetupPhysics(ConfigFile.GridPosition, ConfigFile.GridDirection);
            Drivers.Add(PlayerVehicle.Driver);

            Race.Current = this;
        }


        public void Update()
        {
            RaceTime.Update();

            if (!RaceTime.IsStarted)
            {
                if ((int)RaceTime.TotalTime == 2 && !RaceTime.CountingDown)
                    RaceTime.StartCountdown();
                if (Engine.Camera is FixedChaseCamera)
                {
                    float height = 55 - (RaceTime.CountdownTime * 35f);
                    ((FixedChaseCamera)Engine.Camera).HeightOverride = Math.Max(0, height);
                }
            }

            foreach (IDriver driver in Drivers)
                driver.Update();
            
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

            PlayerVehicle.Update();

            foreach (Opponent opponent in Opponents)
            {
                opponent.Vehicle.Update();
            }

            MessageRenderer.Instance.Update();
        }

        public void Render()
        {
            if (_skybox != null) _skybox.Draw();

            _models.SetupRender();
            _actors.Render(_models, Matrix.Identity);

            foreach (Opponent opponent in Opponents)
            {
                opponent.Vehicle.Render();
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

        public void OnVehicleEnterSpecVol(SpecialVolume volume, Vehicle vehicle)
        {
            int currentVolumeId = vehicle.CurrentSpecialVolume.Count == 0 ? -1 : vehicle.CurrentSpecialVolume.Peek().Id;
            GameConsole.WriteEvent("Enter specvol - " + vehicle.CurrentSpecialVolume.Count);

            if (currentVolumeId != volume.Id)
            {
                volume.Enter(vehicle);
            }
            vehicle.CurrentSpecialVolume.Push(volume);
        }

        public void OnVehicleExitSpecVol(SpecialVolume exitedVolume, Vehicle vehicle)
        {
            SpecialVolume vol = vehicle.CurrentSpecialVolume.Pop();
            SpecialVolume nextVol = vehicle.CurrentSpecialVolume.Count == 0 ? null : vehicle.CurrentSpecialVolume.Peek();
            GameConsole.WriteEvent("Exit specvol - " + vehicle.CurrentSpecialVolume.Count);

            if (nextVol == null)
            {
                //reset
                exitedVolume.Reset(vehicle);
                exitedVolume.Exit();
            }
            else if (nextVol.Id != vol.Id)
            {
                nextVol.Enter(vehicle);
                exitedVolume.Exit();
            }
        }
    }
}
