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
        CActorHierarchy _actors;
        List<NonCar> _nonCars;
        public RaceTimeController RaceTime = new RaceTimeController();
        SkyBox _skybox;
        public int NextCheckpoint = 0, CurrentLap, NbrDeadOpponents;
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
                MatFile matFile = new MatFile(GameVariables.BasePath + @"data\material\" + matFileName);
                ResourceCache.Add(matFile);
            }

            foreach (string pixFileName in ConfigFile.PixFiles)
            {
                PixFile pixFile = new PixFile(GameVariables.BasePath + @"data\pixelmap\" + pixFileName);
                ResourceCache.Add(pixFile);
            }

            ResourceCache.Add(new MatFile(GameVariables.BasePath + @"data\material\" + "drkcurb.mat"));

            ResourceCache.ResolveMaterials();

            DatFile modelFile = new DatFile(GameVariables.BasePath + @"data\models\" + ConfigFile.ModelFile);

            ActFile actFile = new ActFile(GameVariables.BasePath + @"data\actors\" + ConfigFile.ActorFile, modelFile.Models);
            _actors = actFile.Hierarchy;
            _actors.ResolveTransforms(false, ConfigFile.Grooves);

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

            Physics.TrackProcessor.GenerateTrackActor(ConfigFile, _actors);
            _nonCars = Physics.TrackProcessor.GenerateNonCars(_actors, ConfigFile.NonCars);            

            Opponents.Add(new Opponent("tassle.txt", ConfigFile.GridPosition, ConfigFile.GridDirection));
            Opponents.Add(new Opponent("ivan.txt", ConfigFile.GridPosition, ConfigFile.GridDirection));
            Opponents.Add(new Opponent("screwie.txt", ConfigFile.GridPosition, ConfigFile.GridDirection));
            Opponents.Add(new Opponent("kutter.txt", ConfigFile.GridPosition, ConfigFile.GridDirection));
            Opponents.Add(new Opponent("dump.txt", ConfigFile.GridPosition, ConfigFile.GridDirection));

            foreach (Opponent o in Opponents) Drivers.Add(o.Driver);
                        
            OpponentController.Nodes = ConfigFile.OpponentPathNodes;

            PlayerVehicle = new Vehicle(GameVariables.BasePath + @"data\cars\" + playerVehicleFile, new PlayerDriver());
            PlayerVehicle.PlaceOnGrid(ConfigFile.GridPosition, ConfigFile.GridDirection);
            Drivers.Add(PlayerVehicle.Driver);

            Race.Current = this;

            PhysX.Instance.Scene.SetActorGroupPairFlags(PhysXConsts.TrackId, PhysXConsts.VehicleId, ContactPairFlag.Forces | ContactPairFlag.OnStartTouch | ContactPairFlag.OnTouch);
            PhysX.Instance.Scene.SetActorGroupPairFlags(PhysXConsts.VehicleId, PhysXConsts.NonCarId, ContactPairFlag.Forces | ContactPairFlag.OnStartTouch | ContactPairFlag.OnTouch);
            PhysX.Instance.Scene.SetActorGroupPairFlags(PhysXConsts.TrackId, PhysXConsts.NonCarId, ContactPairFlag.OnTouch);
            PhysX.Instance.Scene.SetActorGroupPairFlags(PhysXConsts.VehicleId, PhysXConsts.VehicleId, ContactPairFlag.Forces | ContactPairFlag.OnTouch | ContactPairFlag.OnStartTouch);
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

            foreach (NonCar nonCar in _nonCars)
            {
                if (nonCar.CActor.PhysXActor != null && !nonCar.CActor.PhysXActor.IsSleeping && nonCar.CActor.PhysXActor.LinearVelocity.Length() > 1)
                {
                    _actors.RecalculateActorParent(nonCar.CActor);
                }
                if (nonCar.Hit)
                {
                    nonCar.OnHit();
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

            BoundingFrustum frustum = new BoundingFrustum(Engine.Camera.View * Engine.Camera.Projection);

             _actors.Render(Matrix.Identity, frustum);

             foreach (Opponent opponent in Opponents)
             {
                 if (frustum.Intersects(opponent.GetBoundingSphere()))
                 {
                     opponent.Driver.InPlayersView = true;
                     opponent.Vehicle.Render();
                 }
                 else
                 {
                     opponent.Driver.InPlayersView = true;
                 }
             }
            
            RaceTime.Render();
            MessageRenderer.Instance.Render();
        }

        public void OnCheckpointHit(Checkpoint checkpoint)
        {
            if (checkpoint.Number == NextCheckpoint)
            {
                SoundCache.Play(SoundIds.Checkpoint, PlayerVehicle, false);
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
                SoundCache.Play(SoundIds.WrongCheckpoint, PlayerVehicle, false);
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
                exitedVolume.Exit(vehicle);
            }
            else if (nextVol.Id != vol.Id)
            {
                nextVol.Enter(vehicle);
                exitedVolume.Exit(vehicle);
            }
        }

        internal void OnCarKilled(Vehicle vehicle)
        {
            foreach (Opponent opponent in Opponents)
            {
                if (opponent.Vehicle == vehicle)
                {
                    opponent.Kill();
                    NbrDeadOpponents++;
                    break;
                }
            }

            if (NbrDeadOpponents == Opponents.Count)
            {
                MessageRenderer.Instance.PostMessage("Every opponent wasted", 30);
            }
            else
            {
                MessageRenderer.Instance.PostMessagePix("destroy.pix", 10, 0.7f, 0.003f, 1.4f);
            }
        }
    }
}
