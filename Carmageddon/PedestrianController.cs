using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using OneAmEngine;

namespace Carmageddon
{
    class PedestrianController
    {
        static List<PedestrianBehaviour> _behaviours;
        static List<PixMap> _pixMaps = new List<PixMap>();

        List<Pedestrian> _peds;
        public int Count;

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

                if (ped.RefNumber < 100)
                    Count++;
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
                    PixFile pixFile = new PixFile(behaviour.PixFile);
                    _pixMaps.AddRange(pixFile.PixMaps);
                    loadedFiles.Add(behaviour.PixFile);
                }
            }

            foreach (PedestrianBehaviour behaviour in _behaviours)
            {
                foreach (PedestrianSequence seq in behaviour.Sequences)
                {
                    var allFrames = new List<PedestrianFrame>();
                    allFrames.AddRange(seq.InitialFrames);
                    allFrames.AddRange(seq.LoopingFrames);

                    foreach (PedestrianFrame frame in allFrames)
                    {
                        PixMap pix = _pixMaps.Find(a => a.Name.Equals(frame.PixName, StringComparison.InvariantCultureIgnoreCase));
                        if (pix == null)
                        {
                            PixFile pixFile = new PixFile(frame.PixName);
                            _pixMaps.AddRange(pixFile.PixMaps);
                            pix = pixFile.PixMaps.Find(a => a.Name.Equals(frame.PixName, StringComparison.InvariantCultureIgnoreCase));
                        }
                        if (pix != null)
                            frame.Texture = pix.Texture;
                    }
                }
            }

            CreateGeometry();
        }

        public void Update()
        {
            Vector3 playerPos = Race.Current.PlayerVehicle.Position;

            for (int i = 0; i < _peds.Count; i++)
            {
                Pedestrian ped = _peds[i];
                if (ped.RefNumber >= 100)
                {
                    // remove used powerups & mines
                    if (ped._stopUpdating)
                    {
                        _peds.Remove(ped);
                        i--;
                        continue;
                    }
                }
                ped.DistanceFromPlayer = Vector3.Distance(playerPos, ped.Position);
                if (ped.DistanceFromPlayer < 100)
                {
                    if (ped.DistanceFromPlayer < 25)
                    {
                        if (Race.Current.RaceTime.IsStarted)
                        {
                            ped.SetRunning(true);
                        }
                    }
                    ped.Update();
                }
            }
        }

        public void Render()
        {
            Engine.Device.Vertices[0].SetSource(_vertexBuffer, 0, VertexPositionTexture.SizeInBytes);
            Engine.Device.VertexDeclaration = _vertexDeclaration;
            GameVars.CurrentEffect.LightingEnabled = false;
            Engine.Device.RenderState.CullMode = CullMode.None;
            Engine.Device.RenderState.ReferenceAlpha = 100;

            GameVars.CurrentEffect.CurrentTechnique.Passes[0].Begin();

            foreach (Pedestrian ped in _peds)
            {
                if (ped.DistanceFromPlayer < 150)
                    ped.Render();
            }

            GameVars.CurrentEffect.CurrentTechnique.Passes[0].End();
            GameVars.CurrentEffect.LightingEnabled = true;
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
