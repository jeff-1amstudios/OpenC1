using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.EditModes;
using NFSEngine;
using Microsoft.Xna.Framework;
using Carmageddon.Physics;
using NFSEngine.Audio;

namespace Carmageddon.GameModes
{
    enum CompletionType
    {
        Checkpoints,
        Opponents,
        Peds
    }

    class RaceCompletedMode : GameMode
    {
        FixedChaseCamera _camera;

        public RaceCompletedMode(CompletionType type)
        {
            _camera = new FixedChaseCamera(6.3f, 2.3f);
            _camera.FieldOfView = MathHelper.ToRadians(55.55f);
            _camera.RotationSpeed = 0.8f;
            _camera.HeightOverride = 10;

            switch (type)
            {
                case CompletionType.Opponents:
                    //MessageRenderer.Instance.PostHeaderMessage("Every opponent wasted!!", 10);
                    break;
                case CompletionType.Checkpoints:
                    MessageRenderer.Instance.PostHeaderMessage("Every opponent wasted!!", 10);
                    break;
                case CompletionType.Peds:
                    MessageRenderer.Instance.PostHeaderMessage("Every pedestrian murdered!!", 10);
                    break;
            }
            
            MessageRenderer.Instance.PostMainMessage("raceover.pix", 8, 0.7f, 0.003f, 1.4f);
            
            SoundCache.Play(SoundIds.RaceCompleted, null, false);
            SoundCache.Play(SoundIds.Clapping, null, false);

            _camera.ResetRotation();
            _camera.RotateTo(MathHelper.Pi * 2);
            PlatformEngine.Engine.Camera = _camera;
        }

        public override void Activate()
        {
            
        }

        public override void Update()
        {
            VehicleChassis chassis = Race.Current.PlayerVehicle.Chassis;
            _camera.Position = Race.Current.PlayerVehicle.GetBodyBottom();

            _camera.Orientation = chassis.Actor.GlobalOrientation.Forward;

            if (_camera.Rotation == MathHelper.Pi * 2)
            {
                _camera.ResetRotation();
                _camera.RotateTo(MathHelper.Pi * 2);
            }
        }

        public override void Render()
        {
            Race.Current.PlayerVehicle.Render();
        }
    }
}
