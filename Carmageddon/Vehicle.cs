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
        CrushSection _crushSection;        
        ParticleEmitter _vehicleBitsEmitter;

        public CarFile Config;
        public VehicleChassis Chassis { get; set; }
        public SkidMarkBuffer SkidMarkBuffer;
        public Stack<SpecialVolume> CurrentSpecialVolume = new Stack<SpecialVolume>();
        public IDriver Driver { get; private set; }
        public VehicleAudio Audio;
        CDeformableModel _deformableModel;
        float _damage = 0;
        public ParticleEmitter DamageSmokeEmitter;
        Vector3 _damagePosition;
        PixmapBillboard _flames;
        VehicleModel _model;

        public Vehicle(string filename, IDriver driver)
        {
            Driver = driver;
            Driver.Vehicle = this;

            Config = new CarFile(filename);

            if (Config.WindscreenMaterial != "none")
                Config.Funks.Add(new WindscreenFunk(Config.WindscreenMaterial, this));

            _model = new VehicleModel(Config, false);

            Audio = new VehicleAudio(this);
            CActor actor2 = _model.GetActor(Path.GetFileNameWithoutExtension(Config.ModelFile));
            _deformableModel = (CDeformableModel)actor2.Model;

            Chassis = new VehicleChassis(this, actor2);

            _crushSection = Config.CrushSections[1];

            CMaterial crashMat = ResourceCache.GetMaterial(Config.CrashMaterialFiles[0]);
            _vehicleBitsEmitter = new ParticleEmitter(new VehicleBitsParticleSystem(crashMat), 3, Vector3.Zero);
            _vehicleBitsEmitter.DumpsPerSecond = 0.7f;

            Audio.Play();

            DamageSmokeEmitter = new ParticleEmitter(new DamageSmokeParticleSystem(Color.Gray), 5, Vector3.Zero);
            DamageSmokeEmitter.Enabled = false;

            _flames = new PixmapBillboard(new Vector2(0.7f, 0.25f), GameVars.BasePath + "data\\pixelmap\\flames.pix");
            SkidMarkBuffer = new SkidMarkBuffer(this, 150);
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
                int particles = Math.Max(6, (int)force / 150000);
                
                if (force > 50000)
                {

                    _vehicleBitsEmitter.DumpParticles(position, particles);
                    GameConsole.WriteEvent("dump particles " + particles);
                }
                //else
                //    _vehicleBitsEmitter.Update(position);

                if (force > 400)
                {
                    GameVars.SparksEmitter.DumpParticles(position, 6);
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
            _model.Update();

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
            
            Vector3 pos = Vector3.Transform(_damagePosition, GameVars.ScaleMatrix * Chassis.Actor.GlobalPose);
            DamageSmokeEmitter.Update(pos);
        }


        public void Render()
        {
            ModelShadow.Render(Config.BoundingBox, Chassis);
            SkidMarkBuffer.Render();
            
            Vector3 pos2 = Vector3.Transform(new Vector3(0, Chassis._heightOffset, 0), Chassis.Actor.GlobalOrientation);
            Matrix pose = Matrix.CreateFromQuaternion(Chassis.Actor.GlobalOrientationQuat) * Matrix.CreateTranslation(Chassis.Actor.GlobalPosition) * Matrix.CreateTranslation(pos2);
            _model.Render(pose);

            GameVars.CurrentEffect.CurrentTechnique.Passes[0].Begin();

            for (int i = 0; i < Config.WheelActors.Count; i++)
            {
                GameVars.CurrentEffect.World = Chassis.Wheels[i].GetRenderMatrix();
                _model.RenderSinglePart(Config.WheelActors[i].Actor);
            }

            if (_damage > 50)
            {
                Vector3 pos = Vector3.Transform(_damagePosition, GameVars.ScaleMatrix * Chassis.Actor.GlobalPose);
                _flames.Render(pos);
            }

            GameVars.CurrentEffect.CurrentTechnique.Passes[0].End();
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

        public void Teleport(Vector3 position)
        {
            SkidMarkBuffer.Reset();
            Chassis.Actor.GlobalPosition = position;
        }
    }
}
