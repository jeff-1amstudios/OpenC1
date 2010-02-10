using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.HUD;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using NFSEngine;
using Microsoft.Xna.Framework;
using Carmageddon.Physics;

namespace Carmageddon.CameraViews
{
    class ChaseView : ICameraView
    {
        List<BaseHUDItem> _hudItems = new List<BaseHUDItem>();
        VehicleModel _vehicle;
        FixedChaseCamera _camera;

        public ChaseView(VehicleModel vehicle)
        {
            _camera = new FixedChaseCamera(6.8f, 7);
            _camera.FieldOfView = MathHelper.ToRadians(55.55f);

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
            VehicleChassis chassis = _vehicle.Chassis;
            _camera.Position = chassis.Body.GlobalPosition;

            if (!chassis.InAir)
            {
                _camera.Orientation = chassis.Body.GlobalOrientation.Forward;
                if (chassis.Speed > 15)
                {
                    _camera.Rotation = (chassis.Backwards ? MathHelper.Pi : 0);
                }
                _camera.HeightOverride = 0;
            }
            else
            {
                _camera.HeightOverride = 2;
            }

            foreach (BaseHUDItem item in _hudItems)
                item.Update();
        }

        public void Render()
        {
            _vehicle.Render();

            foreach (BaseHUDItem item in _hudItems)
                item.Render();            
        }

        public void Activate()
        {
            Engine.Instance.Camera = _camera;
        }

        public void Deactivate()
        {
            
        }

        #endregion
    }
}
