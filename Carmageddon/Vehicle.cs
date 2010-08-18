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
        float _damage = 0;
        public ParticleEmitter DamageSmokeEmitter;
        Vector3 _damagePosition;
        PixmapBillboard _flames;


        public Vehicle(string filename, IDriver driver)
        {
            Driver = driver;
            Driver.Vehicle = this;

            LoadModel(filename);

            _crushSection = Config.CrushSections[1];

            CMaterial crashMat = ResourceCache.GetMaterial(Config.CrashMaterialFiles[0]);
            _vehicleBitsEmitter = new ParticleEmitter(new VehicleBitsParticleSystem(crashMat), 6, Vector3.Zero);

            Audio.Play();

            DamageSmokeEmitter = new ParticleEmitter(new DamageSmokeParticleSystem(Color.Gray), 5, Vector3.Zero);
            DamageSmokeEmitter.Enabled = false;

            _flames = new PixmapBillboard(new Vector2(0.7f, 0.25f), GameVariables.BasePath + "data\\pixelmap\\flames.pix");            
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
            ActFile actFile = new ActFile(GameVariables.BasePath + "data\\actors\\" + Config.ActorFile, modelFile.Models);

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

        public void OnCollision(float force, Vector3 position, Vector3 normal, ContactPairFlag events)
        {
            float product = Math.Abs(Vector3.Dot(Chassis.Actor.GlobalPose.Forward, normal));

            if (Chassis.LastSpeeds.GetMax() > 7)
            {
                if (force > 1500)
                {
                    _vehicleBitsEmitter.DumpParticles(position);
                }
                if (force > 400)
                {
                    GameVariables.SparksEmitter.DumpParticles(position, 6);
                }

                if (Driver is PlayerDriver)
                {   
                    if (product < 0.3f)
                    {
                        SoundCache.PlayScrape(this);
                    }
                    else if (force > 50)
                        SoundCache.PlayCrash(this, force);
                }
            }
            _deformableModel.OnContact(position, force, normal);

            // if this is a CPU driven car, only damage if the player has something to do with it.  Stops cars killing themselves
            if (Driver is CpuDriver && ((CpuDriver)Driver).LastPlayerTouchTime + 0.3f > Engine.TotalSeconds)
            {
                Damage(force);
            }
            else if (Driver is PlayerDriver)
            {
                Damage(force);
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
            
            Vector3 pos = Vector3.Transform(_damagePosition, GameVariables.ScaleMatrix * Chassis.Actor.GlobalPose);
            DamageSmokeEmitter.Update(pos);
        }


        public void Render()
        {
            //Engine.DebugRenderer.AddAxis(Chassis.Actor.CenterOfMassGlobalPose, 1);
            //return;
            ModelShadow.Render(Config.BoundingBox, Chassis);
            SkidMarkBuffer.Render();

            //Vector3 pos2 = Chassis.Actor.GlobalPosition;
            Vector3 pos2 = Vector3.Transform(new Vector3(0,1,0), Chassis.Actor.GlobalOrientation);
            Matrix pose = Matrix.CreateFromQuaternion(Chassis.Actor.GlobalOrientationQuat) * Matrix.CreateTranslation(Chassis.Actor.GlobalPosition) * Matrix.CreateTranslation(pos2);
            Engine.DebugRenderer.AddAxis(pose, 5);
            //pos2 *= 4;
            //pose = Matrix.CreateFromQuaternion(Chassis.Actor.GlobalOrientationQuat) * Matrix.CreateTranslation(Chassis.Actor.GlobalPosition) * Matrix.CreateTranslation(pos2);
            //Engine.DebugRenderer.AddAxis(pose, 5);
            _actors.Render(pose, null);

            GameVariables.CurrentEffect.CurrentTechnique.Passes[0].Begin();            

            for (int i = 0; i < Config.WheelActors.Count; i++)
            {
                GameVariables.CurrentEffect.World = Chassis.Wheels[i].GetRenderMatrix();
                _actors.RenderSingle(Config.WheelActors[i].Actor);
            }

            if (_damage > 50)
            {
                Vector3 pos = Vector3.Transform(_damagePosition, GameVariables.ScaleMatrix * Chassis.Actor.GlobalPose);
                _flames.Render(pos);
            }

            GameVariables.CurrentEffect.CurrentTechnique.Passes[0].End();
            //Engine.DebugRenderer.AddAxis(Chassis.Actor.CenterOfMassGlobalPose, 5);
        }

        public Vector3 Position
        {
            get { return Chassis.Actor.GlobalPosition; }
        }

        public void Recover(Matrix pose)
        {
            if (pose != Matrix.Identity)
            {
                pose.Up = Vector3.Up;
                Chassis.Actor.GlobalPose = pose;
            }
            Chassis.Reset();
        }

        public Vector3 GetBodyBottom()
        {
            return Position + new Vector3(0, Chassis.Wheels[0].Shape.LocalPosition.Y - Config.DrivenWheelRadius, 0);
        }

        private void Damage(float force)
        {
            if (force < 170000) return;

            float olddamage = _damage;
            float damage = force * Config.CrushSections[1].DamageMultiplier * 0.000005f;
            _damage += damage;
            Chassis.Motor.Damage = _damage;
            GameConsole.WriteEvent("Damage " + force + ", "  + _damage);

            if (_damage > 15 && olddamage < 15)
            {
                DamageSmokeEmitter.Enabled = true;
                DamageSmokeEmitter.ParticleSystem = new DamageSmokeParticleSystem(Color.White);
                DamageSmokeEmitter.ParticlesPerSecond = 8;
                _damagePosition = _deformableModel.GetMostDamagedPosition();
            }
            else if (_damage > 40 && olddamage < 40)
            {
                _damagePosition = _deformableModel.GetMostDamagedPosition();
                DamageSmokeEmitter.ParticleSystem = new DamageSmokeParticleSystem(Color.Gray);
                DamageSmokeEmitter.ParticlesPerSecond = 15;
            }
            else if (_damage > 70 && olddamage < 70)
            {
                _damagePosition = _deformableModel.GetMostDamagedPosition();
                DamageSmokeEmitter.ParticleSystem = new DamageSmokeParticleSystem(Color.Black);
                DamageSmokeEmitter.ParticlesPerSecond = 20;
                if (Driver is CpuDriver)
                {
                    Race.Current.OnCarKilled(this);
                    return;
                }
            }

            if (Driver is CpuDriver)
            {
                Race.Current.OnPlayerCpuCarHit(damage);
            }
        }

        internal void Repair()
        {
            if (_damage > 0)
            {
                MessageRenderer.Instance.PostHeaderMessage("Repair Cost: " + (int)_damage * 20, 2);
                SoundCache.Play(SoundIds.Repair, this, false);
                _deformableModel.Repair();
                _damage = Chassis.Motor.Damage = 0;
                DamageSmokeEmitter.Enabled = false;
            }
        }
    }
}
