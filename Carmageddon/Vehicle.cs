using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.Parsers;
using Carmageddon.Physics;
using Carmageddon.Gfx;
using Carmageddon.Parsers.Grooves;
using NFSEngine.Audio;
using Carmageddon.Parsers.Funks;
using Particle3DSample;
using NFSEngine;
using System.IO;
using StillDesign.PhysX;
using Microsoft.Xna.Framework.Input;

namespace Carmageddon
{
    class Vehicle
    {
        CActorHierarchy _actors;
        CrushSection _crushSection;
        List<BaseGroove> _grooves;
        ParticleEmitter _vehicleBitsEmitter;

        public CarFile Config;
        public VehicleChassis Chassis { get; set; }
        public SkidMarkBuffer SkidMarkBuffer = new SkidMarkBuffer(200);
        public Stack<SpecialVolume> CurrentSpecialVolume = new Stack<SpecialVolume>();
        public IDriver Driver { get; private set; }
        public VehicleAudio Audio;
        CDeformableModel _deformableModel;

        public Vehicle(string filename, IDriver driver)
        {
            Driver = driver;
            Driver.Vehicle = this;

            LoadModel(filename);
            
            _crushSection = Config.CrushSections[1];
            
            CMaterial crashMat = ResourceCache.GetMaterial(Config.CrashMaterialFiles[0]);
            _vehicleBitsEmitter = new ParticleEmitter(new VehicleBitsParticleSystem(crashMat), 6, Vector3.Zero);

            Audio.Play();                  
        }

        private void LoadModel(string filename)
        {
            Config = new CarFile(filename);

            foreach (string pixFileName in Config.PixFiles)
            {
                PixFile pixFile = new PixFile(GameVariables.BasePath + "data\\pixelmap\\" + pixFileName);
                ResourceCache.Add(pixFile);
            }

            foreach (string matFileName in Config.MaterialFiles)
            {
                MatFile matFile = new MatFile(GameVariables.BasePath + "data\\material\\" + matFileName);
                ResourceCache.Add(matFile);
            }

            foreach (string matFileName in Config.CrashMaterialFiles)
            {
                MatFile matFile = new MatFile(GameVariables.BasePath + "data\\material\\" + matFileName);
                ResourceCache.Add(matFile);
            }

            ResourceCache.ResolveMaterials();

            _grooves = new List<BaseGroove>();
            foreach (BaseGroove g in Config.Grooves)
                if (!g.IsWheelActor) _grooves.Add(g);

            DatFile modelFile = new DatFile(GameVariables.BasePath + "data\\models\\" + Config.ModelFile, new List<string> { Config.ModelFile });
            ActFile actFile = new ActFile(GameVariables.BasePath +  "data\\actors\\" + Config.ActorFile, modelFile.Models);
            
            _actors = actFile.Hierarchy;
            _actors.ResolveTransforms(true, _grooves);

            if (Config.WindscreenMaterial != "none")
                Config.Funks.Add(new WindscreenFunk(Config.WindscreenMaterial, this));
            
            foreach (BaseGroove g in _grooves)
                g.SetActor(_actors.GetByName(g.ActorName));
            
            // link the funks and materials
            foreach (BaseFunk f in Config.Funks) 
                f.Resolve();

            Vector3 tireWidth = new Vector3(0.034f, 0, 0) * GameVariables.Scale;

            foreach (int id in Config.DrivenWheelRefs)
            {
                BaseGroove g = Config.Grooves.Find(a => a.Id == id);
                if (g == null) continue;
                CActor actor = _actors.GetByName(g.ActorName);
                CWheelActor ca = new CWheelActor(actor, true, false);
                ca.Position = actor.Matrix.Translation + (ca.IsLeft ? -1 * tireWidth : tireWidth);
                Config.WheelActors.Add(ca);
            }
            foreach (int id in Config.NonDrivenWheelRefs)
            {
                BaseGroove g = Config.Grooves.Find(a => a.Id == id);
                CActor actor = _actors.GetByName(g.ActorName);
                CWheelActor ca = new CWheelActor(actor, false, true);
                ca.Position = actor.Matrix.Translation + (ca.IsLeft ? -1 * tireWidth : tireWidth);
                Config.WheelActors.Add(ca);
            }

            Audio = new VehicleAudio(this);

            CActor actor2 = _actors.GetByName(Path.GetFileNameWithoutExtension(Config.ModelFile));
            _deformableModel = (CDeformableModel)actor2.Model;

            Chassis = new VehicleChassis(this, actor2);
        }

        
        public void PlaceOnGrid(Vector3 position, float direction)
        {
            Matrix pose = GridPlacer.GetGridPosition(position, direction);
            Chassis.Actor.GlobalPose = pose;
            
            
        }

        public void ContactReport_Collision(Vector3 force, Vector3 position, Vector3 normal)
        {
            float forceSize = force.Length();

            if (Chassis.Speed > 7 || Chassis.LastSpeed > 7)
            {
                if (forceSize > 400)
                {
                    if (forceSize > 1500)
                    {
                        _vehicleBitsEmitter.DumpParticles(position);
                    }
                    if (forceSize > 400)
                    {
                        GameVariables.SparksEmitter.DumpParticles(position, 6);
                    }
                }

                if (Driver is PlayerDriver)
                {
                    float product = Math.Abs(Vector3.Dot(Chassis.Actor.GlobalPose.Forward, normal));
                    if (product < 0.3f)
                    {
                        SoundCache.PlayScrape(this);
                    }
                    else if (forceSize > 200)
                        SoundCache.PlayCrash(this, forceSize);
                }

                _deformableModel.OnContact(position, force, normal);
            }
        }

        public void Update()
        {
           
            foreach (BaseGroove groove in _grooves)
            {
                groove.Update();
            }

            foreach (BaseFunk funk in Config.Funks)
            {
                funk.Update();
            }

            Audio.Update();

            if (CurrentSpecialVolume.Count > 0)
                CurrentSpecialVolume.Peek().Update(this);

            Chassis.Update();

            float maxlat = 0;
            foreach (VehicleWheel wheel in Chassis.Wheels)
            {
                if (Math.Abs(wheel.LatSlip) > maxlat) maxlat = Math.Abs(wheel.LatSlip);
                if (wheel.ShouldPlaySkidSound)
                {
                    SoundCache.PlaySkid(this, wheel.LatSlip);
                    break;
                }
            }
            GameConsole.WriteLine("maxlat", maxlat);
        }


        public void Render()
        {
            ModelShadow.Render(Config.BoundingBox, Chassis);
            SkidMarkBuffer.Render();

            _actors.Render(Chassis.Actor.GlobalPose, null);
            
            GameVariables.CurrentEffect.CurrentTechnique.Passes[0].Begin();

            for (int i = 0; i < Config.WheelActors.Count; i++)
            {
                GameVariables.CurrentEffect.World = Chassis.Wheels[i].GetRenderMatrix();
                _actors.RenderSingle(Config.WheelActors[i].Actor);
            }

            //Engine.DebugRenderer.AddAxis(Chassis.Actor.CenterOfMassGlobalPose, 5);

            GameVariables.CurrentEffect.CurrentTechnique.Passes[0].End();
        }

        public Vector3 Position
        {
            get { return Chassis.Actor.GlobalPosition; }
        }

        public void Reset()
        {
            Chassis.Reset();
        }

        public Vector3 GetBodyBottom()
        {
            return Position + new Vector3(0, Chassis.Wheels[0].Shape.LocalPosition.Y - Config.DrivenWheelRadius, 0);
        }
    }
}
