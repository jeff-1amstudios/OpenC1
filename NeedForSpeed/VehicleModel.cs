using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.Parsers;
using Carmageddon.Physics;

namespace Carmageddon
{
    class VehicleModel
    {
        DatFile _models;
        ActFile _actors;
        ResourceCache _resourceCache;
        CrushSection _crushSection;
        public PhysicalProperties Properties { get; private set; }
        private List<WheelActor> _wheelActors = new List<WheelActor>();
        public PhysicsVehicle PhysicalVehicle { get; set; }

        public VehicleModel(string filename)
        {
            CarFile carFile = new CarFile(filename);

            _resourceCache = new ResourceCache();
            foreach (string pixFileName in carFile.PixFiles)
            {
                PixFile pixFile = new PixFile(@"C:\Games\carma1\data\pixelmap\" + pixFileName);
                _resourceCache.Add(pixFile);
            }

            foreach (string matFileName in carFile.MaterialFiles)
            {
                MatFile matFile = new MatFile(@"C:\Games\carma1\data\material\" + matFileName);
                _resourceCache.Add(matFile);
            }

            _models = new DatFile(@"C:\Games\carma1\data\models\" + carFile.ModelFile);

            _actors = new ActFile(@"C:\Games\carma1\data\actors\" + carFile.ActorFile, _models, false);
            _actors.ResolveMaterials(_resourceCache);
            _models.Resolve(_resourceCache);

            _crushSection = carFile.CrushSections[1];

            Properties = carFile.PhysicalProperties;

            Vector3 tireWidth = new Vector3(0.00f, 0, 0) * GameVariables.Scale;
            Properties.WheelPositions.Add(_actors.GetByName("FLPIVOT").Matrix.Translation - tireWidth);
            Properties.WheelPositions.Add(_actors.GetByName("FRPIVOT").Matrix.Translation + tireWidth);
            Properties.WheelPositions.Add(_actors.GetByName("RLWHEEL").Matrix.Translation-tireWidth);
            Properties.WheelPositions.Add(_actors.GetByName("RRWHEEL").Matrix.Translation+tireWidth);
            

            _wheelActors.Add(new WheelActor(_actors.GetByName("FLWHEEL")));
            _wheelActors.Add(new WheelActor(_actors.GetByName("FRWHEEL")));
            _wheelActors.Add(new WheelActor(_actors.GetByName("RLWHEEL")));
            _wheelActors.Add(new WheelActor(_actors.GetByName("RRWHEEL")));

        }

        public void Update(GameTime gameTime)
        {
        }


        public void Render()
        {
            Engine.Instance.Device.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
            Engine.Instance.Device.SamplerStates[0].AddressV = TextureAddressMode.Clamp;

            Engine.Instance.CurrentEffect = _models.SetupRender();

            Vector3[] points = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                StillDesign.PhysX.RaycastHit hit = PhysicalVehicle.Body.Scene.RaycastClosestShape(
                    new StillDesign.PhysX.Ray(PhysicalVehicle.Wheels[i].GlobalPosition * new Vector3(1, 0, 1), Vector3.Down), StillDesign.PhysX.ShapesType.Static);

                points[i] = hit.WorldImpact;
            }

            ModelShadow.Render(points);
            _models.DoneRender();
            
            _actors.Render(_models, PhysicalVehicle.Body.GlobalPose);

            //Engine.Instance.GraphicsUtils.AddAxis(PhysicalVehicle.Body.CenterOfMassGlobalPose, 10);

            Engine.Instance.CurrentEffect = _models.SetupRender();

            Engine.Instance.CurrentEffect.CurrentTechnique.Passes[0].Begin();

            for (int i = 0; i < 4; i++)
            {
                Engine.Instance.CurrentEffect.World = GetWorldMatrixForWheel(i);
                _actors.RenderSingle(_wheelActors[i].Actor);
            }

            Engine.Instance.CurrentEffect.CurrentTechnique.Passes[0].End();

            _models.DoneRender();
            
            Engine.Instance.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            Engine.Instance.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            return;

            for (int i = 0; i < _crushSection.Data.Count; i++)
            {
                CrushData d = _crushSection.Data[i];
                //Vector3 center = ((d.V2 + d.V1) / 2) + new Vector3(0, 1.0f, 0);
                //Vector3 size = d.V2 - d.V1;
                float dx = MathHelper.Distance(d.V1.X, d.V2.X);
                float dy = MathHelper.Distance(d.V1.Y, d.V2.Y);
                float dz = MathHelper.Distance(d.V1.Z, d.V2.Z);
                //dx=dy=dz=0.03f;
                Vector3 ride = new Vector3(0, 0.11f, 0);
                //Engine.Instance.GraphicsUtils.AddLine(d.V1 + ride, d.V2 + ride, Color.Yellow);
                int baseIdx = _models.GetModels()[0].VertexBaseIndex;
                foreach (CrushPoint pt in d.Points)
                {
                    Vector3 pos = _models._vertices[baseIdx + pt.VertexIndex].Position + ride;
                    Engine.Instance.GraphicsUtils.AddCube(Matrix.CreateScale(0.01f) * Matrix.CreateTranslation(pos), Color.Yellow);
                }
                //Engine.Instance.GraphicsUtils.AddWireframeCube(
                  //  Matrix.CreateScale(new Vector3(dx,dy,dz)) * Matrix.CreateTranslation(d.V1+new Vector3(0, 0.11f, 0)), Color.Yellow);
            }
            //Engine.Instance.GraphicsUtils.AddWireframeCube(Matrix.CreateScale(0.03f) * Matrix.CreateTranslation(0.050065f, 0.011696f + 0.11f, 0.383752f), Color.Yellow);
            //_models.Crush(_crushSection);
        }

        private Matrix GetWorldMatrixForWheel(int wheelIndex)
        {
            StillDesign.PhysX.WheelShape wheel = PhysicalVehicle.Wheels[wheelIndex];
            StillDesign.PhysX.WheelContactData wcd = wheel.GetContactData();
            
            float suspensionLength = 0;

            if (wcd.ContactShape == null)
            {
                suspensionLength = wheel.SuspensionTravel;
            }
            else
            {
                suspensionLength = wcd.ContactPosition - wheel.Radius;
            }

            _wheelActors[wheelIndex].UpdateAxleSpeed(wheel.AxleSpeed);

            Matrix world = wheel.GlobalPose;
            Matrix translation = Matrix.CreateTranslation(0.0f, -suspensionLength, 0.0f);
            return _wheelActors[wheelIndex].RotationMatrix * Matrix.CreateRotationY(wheel.SteeringAngle) * translation * world;
        }
        

        internal void Crush()
        {
            _models.Crush(_crushSection);
        }
    }
}
