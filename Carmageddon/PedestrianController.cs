using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.Parsers;
using PlatformEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Carmageddon
{
    class PedestrianController
    {
        static List<PedestrianBehaviour> _behaviours;
        static List<PixMap> _pixMaps = new List<PixMap>();

        List<Pedestrian> _peds;

        VertexPositionTexture[] _vertices;
        VertexBuffer _vertexBuffer;
        VertexDeclaration _vertexDeclaration;
        
        public PedestrianController(List<Pedestrian> peds)
        {
            if (_behaviours == null)
            {
                Initialize();
            }

            _peds = peds;

            foreach (Pedestrian ped in _peds)
            {   //match up behaviour to ped instance
                ped.Behaviour = _behaviours.Find(a => a.RefNumber == ped.RefNumber);
                ped.Initialize();
            }
        }

        private void Initialize()
        {
            _behaviours = new PedestriansFile()._pedestrians;

            List<string> loadedFiles = new List<string>();
            foreach (PedestrianBehaviour behaviour in _behaviours)
            {
                if (!loadedFiles.Contains(behaviour.PixFile))
                {
                    PixFile pixFile = new PixFile(GameVars.BasePath + "data\\pixelmap\\" + behaviour.PixFile);
                    _pixMaps.AddRange(pixFile.PixMaps);
                    loadedFiles.Add(behaviour.PixFile);
                }
            }

            foreach (PedestrianBehaviour behaviour in _behaviours)
            {
                foreach (PedestrianSequence seq in behaviour.Sequences)
                {
                    foreach (PedestrianFrame frame in seq.InitialFrames)
                    {
                        PixMap pix = _pixMaps.Find(a => a.Name.Equals(frame.PixName, StringComparison.InvariantCultureIgnoreCase));
                        if (pix != null) frame.Texture = pix.Texture;
                    }
                    foreach (PedestrianFrame frame in seq.LoopingFrames)
                    {
                        PixMap pix = _pixMaps.Find(a => a.Name.Equals(frame.PixName, StringComparison.InvariantCultureIgnoreCase));
                        if (pix != null) frame.Texture = pix.Texture;
                    }
                }
            }

            CreateGeometry();
        }

        public void Update()
        {
            Vector3 playerPos = Race.Current.PlayerVehicle.Position;

            foreach (Pedestrian ped in _peds)
            {
                if (Vector3.Distance(playerPos, ped.Position) < 10)
                    ped.SetAction(ped.Behaviour.Running);
                ped.Update();
            }
        }

        public void Render()
        {
            Engine.Device.Vertices[0].SetSource(_vertexBuffer, 0, VertexPositionTexture.SizeInBytes);
            Engine.Device.VertexDeclaration = _vertexDeclaration;
            GameVars.CurrentEffect.LightingEnabled = false;
            Engine.Device.RenderState.CullMode = CullMode.None;

            GameVars.CurrentEffect.CurrentTechnique.Passes[0].Begin();

            foreach (Pedestrian ped in _peds)
            {
                ped.Render();
            }

            GameVars.CurrentEffect.CurrentTechnique.Passes[0].End();
            GameVars.CurrentEffect.LightingEnabled = true;
            Engine.Device.RenderState.FillMode = FillMode.Solid;
        }

        private void CreateGeometry()
        {
            Vector3 topLeftFront = new Vector3(-0.5f, 1.0f, 0.5f);
            Vector3 bottomLeftFront = new Vector3(-0.5f, 0.0f, 0.5f);
            Vector3 topRightFront = new Vector3(0.5f, 1.0f, 0.5f);
            Vector3 bottomRightFront = new Vector3(0.5f, 0.0f, 0.5f);

            Vector2 textureTopLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureTopRight = new Vector2(1.0f, 0.0f);
            Vector2 textureBottomLeft = new Vector2(0.0f, 1.0f);
            Vector2 textureBottomRight = new Vector2(1.0f, 1.0f);

            _vertices = new VertexPositionTexture[4];
            _vertices[0] = new VertexPositionTexture(topLeftFront, textureTopLeft);
            _vertices[1] = new VertexPositionTexture(bottomLeftFront, textureBottomLeft);
            _vertices[2] = new VertexPositionTexture(topRightFront, textureTopRight);
            _vertices[3] = new VertexPositionTexture(bottomRightFront, textureBottomRight);

            _vertexBuffer = new VertexBuffer(Engine.Device,
                                                 VertexPositionTexture.SizeInBytes * _vertices.Length,
                                                 BufferUsage.WriteOnly);

            _vertexBuffer.SetData<VertexPositionTexture>(_vertices);
            _vertexDeclaration = new VertexDeclaration(Engine.Device, VertexPositionTexture.VertexElements);
        }

    }
}
