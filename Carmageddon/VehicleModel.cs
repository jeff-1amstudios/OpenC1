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

namespace Carmageddon
{
    class VehicleModel
    {
        DatFile _models;
        ActFile _actors;
        CrushSection _crushSection;
        public CarFile Config;
        public VehicleChassis Chassis { get; set; }
        private List<BaseGroove> _grooves = new List<BaseGroove>();
        List<ISound> _engineSounds = new List<ISound>();
        ISound _engineSound;
        
        ParticleEmitter _vehicleBitsEmitter;
        public Stack<SpecialVolume> CurrentSpecialVolume = new Stack<SpecialVolume>();

        public VehicleModel(string filename, bool userDriven)
        {
            Config = new CarFile(filename);

            foreach (string pixFileName in Config.PixFiles)
            {
                PixFile pixFile = new PixFile(@"C:\Games\carma1\data\pixelmap\" + pixFileName);
                ResourceCache.Add(pixFile);
            }

            foreach (string matFileName in Config.MaterialFiles)
            {
                MatFile matFile = new MatFile(@"C:\Games\carma1\data\material\" + matFileName);
                ResourceCache.Add(matFile);
            }

            foreach (string matFileName in Config.CrashMaterialFiles)
            {
                MatFile matFile = new MatFile(@"C:\Games\carma1\data\material\" + matFileName);
                ResourceCache.Add(matFile);
            }

            ResourceCache.ResolveMaterials();

            foreach (BaseGroove g in Config.Grooves)
                if (!g.IsWheelActor) _grooves.Add(g);

            _models = new DatFile(@"C:\Games\carma1\data\models\" + Config.ModelFile);

            _actors = new ActFile(@"C:\Games\carma1\data\actors\" + Config.ActorFile, _models);
            _actors.ResolveHierarchy(true, _grooves);
            _actors.ResolveMaterials();
            _models.Resolve();

            if (Config.WindscreenMaterial != "none")
            {
                Config.Funks.Add(new WindscreenFunk(Config.WindscreenMaterial, this));
            }

            foreach (BaseGroove g in _grooves)
            {
                g.SetActor(_actors.GetByName(g.ActorName));
            }

            // link the funks and materials
            foreach (BaseFunk f in Config.Funks)
            {
                f.Resolve();
            }

            _crushSection = Config.CrushSections[1];


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
                //if (actor != null)
                //{
                CWheelActor ca = new CWheelActor(actor, false, true);
                ca.Position = actor.Matrix.Translation + (ca.IsLeft ? -1 * tireWidth : tireWidth);
                Config.WheelActors.Add(ca);
                //}
                //else
                //{
                //GameConsole.WriteEvent("Actor not found: " + g.ActorName);
                //}
            }
            foreach (int id in Config.EngineSoundIds)
                _engineSounds.Add(SoundCache.CreateInstance(id));
            _engineSound = _engineSounds[0];

            if (_engineSound != null)
            {
                _engineSound.Play(true);
                _engineSound.Volume -= 1000;
            }

            CMaterial crashMat = ResourceCache.GetMaterial(Config.CrashMaterialFiles[0]);
            _vehicleBitsEmitter = new ParticleEmitter(new VehicleBitsParticleSystem(crashMat), 6, Vector3.Zero);

            if (userDriven)
                ContactReport.Instance.PlayerWorldCollision += ContactReport_PlayerWorldCollision;
        }

        public int EngineSoundIndex
        {
            set
            {
                if (_engineSound != _engineSounds[value])
                {
                    _engineSound.Stop();
                    _engineSound = _engineSounds[value];
                    _engineSound.Play(true);
                }
            }
        }

        public Vector3 GetBodyBottom()
        {
            
            Vector3 pos = Chassis.Body.GlobalPosition;
            pos.Y = pos.Y + Chassis.Wheels[0].Shape.LocalPosition.Y - Config.DrivenWheelRadius;
            return pos;
        }

        public void SetupChassis(Vector3 position, float direction)
        {
            Matrix pose = GridPlacer.GetGridPosition(position, direction);
            Chassis = new Carmageddon.Physics.VehicleChassis(Carmageddon.Physics.PhysX.Instance.Scene, pose, 1, this);
        }

        void ContactReport_PlayerWorldCollision(float force, Vector3 position, Vector3 normal)
        {
            //GameConsole.WriteEvent(force.ToString());

            if (force > 200 /* 750000*/)
            {
                if (force > 1000)
                {
                    _vehicleBitsEmitter.DumpParticles(position);
                }
                SoundCache.PlayCrash();
                return;
            }
            
            float product = Math.Abs(Vector3.Dot(Chassis.Body.GlobalPose.Forward, normal));
            if (product < 0.3f)
            {
                SoundCache.PlayScrape();
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

            if (_engineSound != null)
            {
                _engineSound.Frequency = 8000 + (int)(Chassis.Motor.Rpm * 2500);
                //_engineSound.Position = Chassis.Body.GlobalPosition;
                //_engineSound.Velocity = Chassis.Body.LinearVelocity;
            }

            if (CurrentSpecialVolume.Count > 0)
                CurrentSpecialVolume.Peek().Update(this);

            Chassis.Update();

            foreach (VehicleWheel wheel in Chassis.Wheels)
            {
                if (wheel.ShouldPlaySkidSound) // wheel.IsSkiddingLat || wheel.IsSkiddingLng)
                {
                    SoundCache.PlaySkid();
                    break;
                }
            }
        }


        public void Render()
        {
            //_vehicleBitsEmitter.ParticleSystem.Render();
                       

            ModelShadow.Render(Config.BoundingBox, Chassis);
            
            GameVariables.SkidMarkBuffer.Render();


            _models.SetupRender();
            _actors.Render(_models, Chassis.Body.GlobalPose);
            
            GameVariables.CurrentEffect.CurrentTechnique.Passes[0].Begin();

            for (int i = 0; i < Config.WheelActors.Count; i++)
            {
                GameVariables.CurrentEffect.World = Chassis.Wheels[i].GetRenderMatrix();
                _actors.RenderSingle(Config.WheelActors[i].Actor);
            }

            //Engine.DebugRenderer.AddAxis(Chassis.Body.CenterOfMassGlobalPose, 5);

            GameVariables.CurrentEffect.CurrentTechnique.Passes[0].End();

            return;

            //for (int i = 0; i < _crushSection.Data.Count; i++)
            //{
            //    CrushData d = _crushSection.Data[i];
            //    //Vector3 center = ((d.V2 + d.V1) / 2) + new Vector3(0, 1.0f, 0);
            //    //Vector3 size = d.V2 - d.V1;
            //    float dx = MathHelper.Distance(d.V1.X, d.V2.X);
            //    float dy = MathHelper.Distance(d.V1.Y, d.V2.Y);
            //    float dz = MathHelper.Distance(d.V1.Z, d.V2.Z);
            //    //dx=dy=dz=0.03f;
            //    Vector3 ride = new Vector3(0, 0.11f, 0);
            //    //Engine.GraphicsUtils.AddLine(d.V1 + ride, d.V2 + ride, Color.Yellow);
            //    int baseIdx = _models.GetModels()[0].VertexBaseIndex;
            //    foreach (CrushPoint pt in d.Points)
            //    {
            //        Vector3 pos2 = _models._vertices[baseIdx + pt.VertexIndex].Position + ride;
            //        Engine.DebugRenderer.AddCube(Matrix.CreateScale(0.01f) * Matrix.CreateTranslation(pos2), Color.Yellow);
            //    }
            //    //Engine.GraphicsUtils.AddWireframeCube(
            //      //  Matrix.CreateScale(new Vector3(dx,dy,dz)) * Matrix.CreateTranslation(d.V1+new Vector3(0, 0.11f, 0)), Color.Yellow);
            //}
            //Engine.GraphicsUtils.AddWireframeCube(Matrix.CreateScale(0.03f) * Matrix.CreateTranslation(0.050065f, 0.011696f + 0.11f, 0.383752f), Color.Yellow);
            //_models.Crush(_crushSection);
        }

        

        internal void Crush()
        {
            _models.Crush(_crushSection);
        }

        public void Reset()
        {
            Chassis.Reset();
        }
    }
}
