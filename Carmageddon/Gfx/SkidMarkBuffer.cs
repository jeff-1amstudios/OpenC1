using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using OpenC1.Physics;
using OpenC1.Parsers;
using OneAmEngine;

namespace OpenC1.Gfx
{

    class CurrentSkid
    {
        public VehicleWheel Wheel;
        public float StartTime;
        public Vector3 StartPosition, EndPosition;
        public bool IsActive = true;
    }

    class SkidMarkBuffer
    {
        const float SKID_TIME = 0.2f;
        private int _maxSkids;

        VertexPositionTexture[] _vertices;
        DynamicVertexBuffer _buffer;
        VertexDeclaration _vertexDeclaration;

        int _firstNewVert;
        int _firstFreeVert;
        bool _usingBloodTexture;
        Texture2D _texture;
        static Texture2D _defaultTexture, _bloodTexture;
        Vehicle _vehicle;
        Texture2D[] _skids;
        int _skidPtr;

        private List<CurrentSkid> _currentSkids = new List<CurrentSkid>();


        public SkidMarkBuffer(Vehicle vehicle, int maxSkids)
        {
            _maxSkids = maxSkids;
            _vehicle = vehicle;
            _vertices = new VertexPositionTexture[_maxSkids * 6];
            _skids = new Texture2D[_maxSkids];

            _vertexDeclaration = new VertexDeclaration(Engine.Device, VertexPositionTexture.VertexElements);

            // Create a dynamic vertex buffer.
            int size = VertexPositionTexture.SizeInBytes * _vertices.Length;

            _buffer = new DynamicVertexBuffer(Engine.Device, size, BufferUsage.WriteOnly);

            if (_defaultTexture == null)
            {
                MatFile matfile = new MatFile("skidmark.mat");
                matfile.Materials[0].ResolveTexture();
                _defaultTexture = matfile.Materials[0].Texture;
                matfile = new MatFile("gibsmear.mat");
                matfile.Materials[0].ResolveTexture();
                _bloodTexture = matfile.Materials[0].Texture;
            }
            _texture = _defaultTexture;
        }

        public void SetTexture(Texture2D texture)
        {
            if (_usingBloodTexture) return;

            if (_texture != texture)
            {
                _texture = texture;
                //_firstFreeParticle = _firstNewParticle = 0;
                _usingBloodTexture = texture == _bloodTexture;
            }
        }

        private void Update()
        {
            if (!Helpers.HasTimePassed(0.9f, _vehicle.LastRunOverPedTime))
            {
                foreach (var wheel in _vehicle.Chassis.Wheels)
                {
                    if (!wheel.IsRear) RegisterSkid(wheel, wheel.ContactPoint);
                }
                SetTexture(_bloodTexture);
            }
            else
            {
                if (_usingBloodTexture)
                {
                    _usingBloodTexture = false;
                    SetTexture(_defaultTexture);
                }

                foreach (var wheel in _vehicle.Chassis.Wheels)
                {
                    if (wheel.IsSkiddingLat || wheel.IsSkiddingLng)
                    {
                        RegisterSkid(wheel, wheel.ContactPoint);
                    }
                }
            }

            for (int i = 0; i < _currentSkids.Count; i++)
            {
                CurrentSkid skid = _currentSkids[i];
                if (!skid.IsActive)
                {
                    _currentSkids.RemoveAt(i);
                    i--;
                    AddToBuffer(skid);
                }
                else if (Helpers.HasTimePassed(SKID_TIME, _currentSkids[i].StartTime))
                {
                    AddToBuffer(skid);
                    skid.StartPosition = skid.EndPosition;
                    skid.StartTime = Engine.TotalSeconds;
                }
            }

            int nbrTempSkids = 0;
            for (int i = 0; i < _currentSkids.Count; i++)
            {
                if (_currentSkids[i].IsActive && !Helpers.HasTimePassed(SKID_TIME, _currentSkids[i].StartTime))
                {
                    //temp
                    _currentSkids[i].EndPosition = _currentSkids[i].Wheel.ContactPoint;
                    AddToBuffer(_currentSkids[i]);
                    nbrTempSkids++;
                }
            }

            AddNewParticlesToVertexBuffer();

            _firstFreeVert -= 6 * nbrTempSkids;
            if (_firstFreeVert < 0)
                _firstFreeVert = _vertices.Length - (-_firstFreeVert);
            _firstNewVert = _firstFreeVert;
            _skidPtr -= 1 * nbrTempSkids;
            if (_skidPtr < 0)
                _skidPtr = _maxSkids - (-_skidPtr);
        }


        public void Render()
        {
            Update();

            GraphicsDevice device = Engine.Device;

            device.Vertices[0].SetSource(_buffer, 0, VertexPositionTexture.SizeInBytes);
            device.VertexDeclaration = _vertexDeclaration;

            GameVars.CurrentEffect.CurrentTechnique.Passes[0].Begin();

            device.RenderState.DepthBias = -0.00002f;
            CullMode oldCullMode = Engine.Device.RenderState.CullMode;
            device.RenderState.CullMode = CullMode.None;

            int nbrCalls = 0;
            for (int i = 0; i < _maxSkids; )
            {
                if (_skids[i] == null) break;

                int j = 1;
                for (; i+j < _maxSkids; j++)
                    if (_skids[i] != _skids[i+j])
                        break;
                device.Textures[0] = _skids[i];
                device.DrawPrimitives(PrimitiveType.TriangleList, i * 6, j * 2);
                nbrCalls++;
                
                i += j;
            }

            GameConsole.WriteLine("skids", nbrCalls);

            GameVars.CurrentEffect.CurrentTechnique.Passes[0].End();

            device.RenderState.CullMode = oldCullMode;

            //de-activate all skids before next update
            foreach (CurrentSkid skid in _currentSkids)
                skid.IsActive = false;

            device.RenderState.DepthBias = 0;
        }


        void AddNewParticlesToVertexBuffer()
        {
            int stride = VertexPositionTexture.SizeInBytes;


            if (_firstNewVert < _firstFreeVert)
            {
                // If the new particles are all in one consecutive range,
                // we can upload them all in a single call.
                _buffer.SetData(_firstNewVert * stride, _vertices,
                                     _firstNewVert,
                                     _firstFreeVert - _firstNewVert,
                                     stride, SetDataOptions.NoOverwrite);
               
            }
            else
            {
                // If the new particle range wraps past the end of the queue
                // back to the start, we must split them over two upload calls.
                _buffer.SetData(_firstNewVert * stride, _vertices,
                                     _firstNewVert,
                                     _vertices.Length - _firstNewVert,
                                     stride, SetDataOptions.NoOverwrite);

                

                if (_firstFreeVert > 0)
                {
                    _buffer.SetData(0, _vertices,
                                         0, _firstFreeVert,
                                         stride, SetDataOptions.NoOverwrite);
                    
                }
            }

            // Move the particles we just uploaded from the new to the active queue.
            _firstNewVert = _firstFreeVert;
        }


        #region Public Methods

        public void RegisterSkid(VehicleWheel wheel, Vector3 pos)
        {
            if (pos == Vector3.Zero)
                return;

            CurrentSkid skid = null;
            foreach (CurrentSkid cskid in _currentSkids)
            {
                if (cskid.Wheel == wheel)
                {
                    skid = cskid; break;
                }
            }
            if (skid == null)
            {
                skid = new CurrentSkid { Wheel = wheel, StartPosition = pos, EndPosition = pos, StartTime = Engine.TotalSeconds };
                _currentSkids.Add(skid);
            }
            else
            {
                skid.IsActive = true;
                skid.EndPosition = pos;
            }
        }

        private void AddToBuffer(CurrentSkid skid)
        {
            int p1, p2;

            float thickness = 0.13f;
            float length = _vehicle.Chassis.Speed * 0.045f;

            Vector3 direction = skid.EndPosition - skid.StartPosition;
            direction.Normalize();

            Vector3 normal = Vector3.Cross(direction, Vector3.UnitY);
            normal.Normalize();

            _vertices[_firstFreeVert].Position = skid.StartPosition - normal * thickness;
            _vertices[_firstFreeVert].TextureCoordinate = new Vector2(0, 1);

            _firstFreeVert++;
            p1 = _firstFreeVert;
            _vertices[_firstFreeVert].Position = skid.EndPosition - normal * thickness;
            _vertices[_firstFreeVert].TextureCoordinate = new Vector2(length, 1);

            _firstFreeVert++;
            p2 = _firstFreeVert;
            _vertices[_firstFreeVert].Position = skid.StartPosition + normal * thickness;
            _vertices[_firstFreeVert].TextureCoordinate = Vector2.Zero;

            _firstFreeVert++;
            _vertices[_firstFreeVert] = _vertices[p2];
            _firstFreeVert++;
            _vertices[_firstFreeVert] = _vertices[p1];
            _firstFreeVert++;
            _vertices[_firstFreeVert].Position = skid.EndPosition + normal * thickness;
            _vertices[_firstFreeVert].TextureCoordinate = new Vector2(length, 0);
                        
            _firstFreeVert = (_firstFreeVert + 1) % _vertices.Length;
             
            _skids[_skidPtr] = _texture ?? _defaultTexture;
            _skidPtr = (_skidPtr + 1) % _maxSkids;
        }

        
        #endregion

        internal void Reset()
        {
            _currentSkids.Clear();
        }
    }
}
