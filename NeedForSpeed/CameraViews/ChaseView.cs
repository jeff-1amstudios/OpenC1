using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.HUD;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;

namespace Carmageddon.CameraViews
{
    class ChaseView : ICameraView
    {
        List<BaseHUDItem> _hudItems = new List<BaseHUDItem>();
        VehicleModel _vehicle;

        public ChaseView(VehicleModel vehicle)
        {
            _vehicle = vehicle;

            _hudItems.Add(new RevCounter(_vehicle.Chassis));
            _hudItems.Add(new Timer());
        }

        #region ICameraView Members


        public bool Selectable
        {
            get { return true; }
        }

        public void Update()
        {
            foreach (BaseHUDItem item in _hudItems)
                item.Update();
        }

        public void Render()
        {
            _vehicle.Render();

            foreach (BaseHUDItem item in _hudItems)
                item.Render();

            Engine.Instance.SpriteBatch.End();
            Engine.Instance.Device.RenderState.DepthBufferEnable = true;
            Engine.Instance.Device.RenderState.AlphaBlendEnable = false;
            Engine.Instance.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            Engine.Instance.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
        }

        public void Activate()
        {
            
        }

        public void Deactivate()
        {
            
        }

        #endregion
    }
}
