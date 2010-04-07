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

            Chassis = new VehicleChassis(this, actor2);
        }

        
        public void PlaceOnGrid(Vector3 position, float direction)
        {
            Matrix pose = GridPlacer.GetGridPosition(position, direction);
            Chassis.Actor.GlobalPose = pose;

            var actorDesc = new ActorDescription();
            actorDesc.BodyDescription = new BodyDescription(0.0001f);
            actorDesc.Shapes.Add(new BoxShapeDescription(new Vector3(0.1f)));
            actorDesc.GlobalPose = Matrix.CreateTranslation(Chassis.Actor.GlobalPosition);
            Actor dummy = PhysX.Instance.Scene.CreateActor(actorDesc);

            CActor bodycactor = _actors.GetByName(Path.GetFileNameWithoutExtension(Config.ModelFile));
            Cloth cloth = ((CDeformableModel)bodycactor.Model).DeformableBody;
            cloth.AttachToCore(dummy, 1, 0.2f);
            FixedJointDescription jointdesc = new FixedJointDescription() { Actor1 = Chassis.Actor, Actor2 = dummy, };
            jointdesc.SetGlobalAxis(new Vector3(0.0f, 1.0f, 0.0f));
            FixedJoint joint = PhysX.Instance.Scene.CreateJoint<FixedJoint>(jointdesc);
        }

        public void ContactReport_Collision(float force, Vector3 position, Vector3 normal)
        {
            if (Chassis.Speed > 7 || Chassis.LastSpeed > 7)
            {
                if (force > 200 /* 750000*/)
                {
                    if (force > 1000)
                    {
                        _vehicleBitsEmitter.DumpParticles(position);
                    }
                    if (force > 200)
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
                    else if (force > 200)
                        SoundCache.PlayCrash(this);
                }
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

            foreach (VehicleWheel wheel in Chassis.Wheels)
            {
                if (wheel.ShouldPlaySkidSound)
                {
                    SoundCache.PlaySkid(this);
                    break;
                }
            }
        }


        public void Render()
        {
            ModelShadow.Render(Config.BoundingBox, Chassis);
            SkidMarkBuffer.Render();

            return;
            _actors.Render(Chassis.Actor.GlobalPose, null);
            
            GameVariables.CurrentEffect.CurrentTechnique.Passes[0].Begin();

            for (int i = 0; i < Config.WheelActors.Count; i++)
            {
                GameVariables.CurrentEffect.World = Chassis.Wheels[i].GetRenderMatrix();
                _actors.RenderSingle(Config.WheelActors[i].Actor);
            }

            Engine.DebugRenderer.AddAxis(Chassis.Actor.CenterOfMassGlobalPose, 5);

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
            _actors.Models.Crush(_crushSection);
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
