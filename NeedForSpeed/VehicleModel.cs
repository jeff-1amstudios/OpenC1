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

namespace Carmageddon
{
    class VehicleModel
    {
        DatFile _models;
        ActFile _actors;
        ResourceCache _resourceCache;
        CrushSection _crushSection;
        public CarFile CarFile;
        public VehicleChassis Chassis { get; set; }
        private List<BaseGroove> _grooves;
        ISound _engineSound;


        public VehicleModel(string filename)
        {
            CarFile = new CarFile(filename);

            _resourceCache = new ResourceCache();
            foreach (string pixFileName in CarFile.PixFiles)
            {
                PixFile pixFile = new PixFile(@"C:\Games\carma1\data\pixelmap\" + pixFileName);
                _resourceCache.Add(pixFile);
            }

            foreach (string matFileName in CarFile.MaterialFiles)
            {
                MatFile matFile = new MatFile(@"C:\Games\carma1\data\material\" + matFileName);
                _resourceCache.Add(matFile);
            }

            _resourceCache.ResolveMaterials();

            _grooves = new List<BaseGroove>();
            foreach (BaseGroove g in CarFile.Grooves)
                if (!g.IsWheelActor) _grooves.Add(g);

            _models = new DatFile(@"C:\Games\carma1\data\models\" + CarFile.ModelFile);

            _actors = new ActFile(@"C:\Games\carma1\data\actors\" + CarFile.ActorFile, _models);
            _actors.ResolveHierarchy(true, _grooves);
            _actors.ResolveMaterials(_resourceCache);
            _models.Resolve(_resourceCache);

            foreach (BaseGroove g in _grooves)
            {
                g.SetActor(_actors.GetByName(g.ActorName));
            }

            // link the funks and materials
            foreach (BaseFunk f in CarFile.Funks)
            {
                if (f is FramesFunk) ((FramesFunk)f).Resolve(_resourceCache);
                CMaterial cm = _resourceCache.GetMaterial(f.MaterialName);
                cm.Funk = f;
                f.Material = cm;
            }

            _crushSection = CarFile.CrushSections[1];

            
            Vector3 tireWidth = new Vector3(0.034f, 0, 0) * GameVariables.Scale;

            foreach (int id in CarFile.DrivenWheelRefs)
            {
                BaseGroove g = CarFile.Grooves.Find(a => a.Id == id);
                if (g == null) continue;
                CActor actor = _actors.GetByName(g.ActorName);
                CWheelActor ca = new CWheelActor(actor, true, false);
                ca.Position = actor.Matrix.Translation + (ca.IsLeft ? -1 * tireWidth : tireWidth);
                CarFile.WheelActors.Add(ca);
            }
            foreach (int id in CarFile.NonDrivenWheelRefs)
            {
                BaseGroove g = CarFile.Grooves.Find(a => a.Id == id);
                CActor actor = _actors.GetByName(g.ActorName);
                CWheelActor ca = new CWheelActor(actor, false, true);
                ca.Position = actor.Matrix.Translation + (ca.IsLeft ? -1*tireWidth : tireWidth);
                CarFile.WheelActors.Add(ca);
            }

            _engineSound = SoundCache.CreateInstance(CarFile.EngineNoiseId);
            _engineSound.Play(true);

        }

        public void Update()
        {
            TyreSmokeParticleSystem.Instance.Update();
            TyreSmokeParticleSystem.Instance.SetCamera(Engine.Instance.Camera);

            SparksParticleSystem.Instance.SetCamera(Engine.Instance.Camera);
            SparksParticleSystem.Instance.Update();

            foreach (BaseGroove groove in _grooves)
            {
                groove.Update();
            }

            foreach (BaseFunk funk in CarFile.Funks)
            {
                funk.Update();
            }

            if (_engineSound != null)
            {
                _engineSound.Frequency = 8000 + (int)(Chassis.Motor.Rpm * 2500);
                //_engineSound.Position = Chassis.Body.GlobalPosition;
                //_engineSound.Velocity = Chassis.Body.LinearVelocity;
            }

            Chassis.Update();
        }


        public void Render()
        {
            Engine.Instance.Device.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
            Engine.Instance.Device.SamplerStates[0].AddressV = TextureAddressMode.Clamp;

            Vector3[] points = new Vector3[4];

            BoundingBox bb = CarFile.BoundingBox;
            Matrix pose = Chassis.Body.GlobalPose;
            float shadowWidth = 0.0f;
            Vector3 pos = new Vector3(bb.Min.X - shadowWidth, 0, bb.Min.Z);
            points[0] = Vector3.Transform(pos, pose);
            pos = new Vector3(bb.Max.X + shadowWidth, 0, bb.Min.Z);
            points[1] = Vector3.Transform(pos, pose);
            pos = new Vector3(bb.Min.X - shadowWidth, 0, bb.Max.Z);
            points[2] = Vector3.Transform(pos, pose);
            pos = new Vector3(bb.Max.X + shadowWidth, 0, bb.Max.Z);
            points[3] = Vector3.Transform(pos, pose);

            StillDesign.PhysX.Scene scene = Chassis.Body.Scene;
            Vector3 offset = new Vector3(0, 0.1f, 0);
            for (int i = 0; i < 4; i++)
            {
                StillDesign.PhysX.RaycastHit hit = scene.RaycastClosestShape(
                    new StillDesign.PhysX.Ray(points[i], Vector3.Down), StillDesign.PhysX.ShapesType.Static);
                points[i] = hit.WorldImpact+offset;
            }

            ModelShadow.Render(points);

            _models.SetupRender();
            
            _actors.Render(_models, Chassis.Body.GlobalPose);

            //Engine.Instance.DebugRenderer.AddAxis(Chassis.Body.CenterOfMassGlobalPose, 10);

            
            GameVariables.CurrentEffect.CurrentTechnique.Passes[0].Begin();

            for (int i = 0; i < CarFile.WheelActors.Count; i++)
            {
                GameVariables.CurrentEffect.World = Chassis.Wheels[i].GetRenderMatrix();
                _actors.RenderSingle(CarFile.WheelActors[i].Actor);
            }

            GameVariables.CurrentEffect.CurrentTechnique.Passes[0].End();

            TyreSmokeParticleSystem.Instance.Render();
            SparksParticleSystem.Instance.Render();

            Engine.Instance.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            Engine.Instance.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

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
            //    //Engine.Instance.GraphicsUtils.AddLine(d.V1 + ride, d.V2 + ride, Color.Yellow);
            //    int baseIdx = _models.GetModels()[0].VertexBaseIndex;
            //    foreach (CrushPoint pt in d.Points)
            //    {
            //        Vector3 pos2 = _models._vertices[baseIdx + pt.VertexIndex].Position + ride;
            //        Engine.Instance.DebugRenderer.AddCube(Matrix.CreateScale(0.01f) * Matrix.CreateTranslation(pos2), Color.Yellow);
            //    }
            //    //Engine.Instance.GraphicsUtils.AddWireframeCube(
            //      //  Matrix.CreateScale(new Vector3(dx,dy,dz)) * Matrix.CreateTranslation(d.V1+new Vector3(0, 0.11f, 0)), Color.Yellow);
            //}
            //Engine.Instance.GraphicsUtils.AddWireframeCube(Matrix.CreateScale(0.03f) * Matrix.CreateTranslation(0.050065f, 0.011696f + 0.11f, 0.383752f), Color.Yellow);
            //_models.Crush(_crushSection);
        }

        

        internal void Crush()
        {
            _models.Crush(_crushSection);
        }
    }
}
