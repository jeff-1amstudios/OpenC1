using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Carmageddon.Parsers.Funks
{
    class WindscreenFunk : BaseFunk
    {
        public Vector2 Speed;
        Vector2 _uvOffset;
        TextureAddressMode _lastMode;
        VehicleModel _vehicle;
        Texture2D _origTexture;

        public WindscreenFunk(string materialName, VehicleModel vehicle)
        {
            _vehicle = vehicle;
            MaterialName = materialName;
            Speed = new Vector2(0.3f);
        }

        public override void Resolve()
        {
            base.Resolve();
            _origTexture = Material.Texture;
        }

        public override void BeforeRender()
        {
            _lastMode = Engine.Device.SamplerStates[0].AddressU;
            if (_lastMode != TextureAddressMode.Wrap)
                Engine.Device.SamplerStates[0].AddressU = Engine.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            GameVariables.CurrentEffect.TexCoordsOffset = _uvOffset;
            GameVariables.CurrentEffect.CommitChanges();
        }

        public override void AfterRender()
        {
            if (_lastMode != TextureAddressMode.Wrap)
                Engine.Device.SamplerStates[0].AddressU = Engine.Device.SamplerStates[0].AddressV = _lastMode;

            GameVariables.CurrentEffect.TexCoordsOffset = Vector2.Zero;
            GameVariables.CurrentEffect.CommitChanges();
        }

        public override void Update()
        {
            float y = Math.Min(1, _vehicle.Chassis.Body.AngularVelocity.Y / 10);
            Speed.X = y;
            Speed.Y = Math.Min(1, _vehicle.Chassis.Body.LinearVelocity.Length() / 10); 
            _uvOffset += Speed * Engine.ElapsedSeconds;
            if (_uvOffset.X > 1) _uvOffset.X = 1 - _uvOffset.X;
            if (_uvOffset.Y > 1) _uvOffset.Y = 1 - _uvOffset.Y;

            if (_vehicle.CurrentSpecialVolume.Count > 0)
            {
                SpecialVolume vol = _vehicle.CurrentSpecialVolume.Peek();
                this.Material.Texture = ResourceCache.GetMaterial(vol.WindscreenMaterial).Texture;
            }
            else
            {
                this.Material.Texture = _origTexture;
            }
        }
    }
}
