using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Carmageddon.Physics;
using OneAmEngine.Audio;
using OneAmEngine;
using Microsoft.Xna.Framework.Input;

namespace Carmageddon.GameModes
{
    enum CompletionType
    {
        Checkpoints,
        Opponents,
        Peds,
        TimeUp
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
                    MessageRenderer.Instance.PostHeaderMessage("Every opponent wasted!!", 10);
                    MessageRenderer.Instance.PostMainMessage("raceover.pix", 8, 0.7f, 0.003f, 1.4f);
                    SoundCache.Play(SoundIds.RaceCompleted, null, false);
                    SoundCache.Play(SoundIds.Clapping, null, false);
                    break;
                case CompletionType.Checkpoints:
                    MessageRenderer.Instance.PostHeaderMessage("All checkpoints passed!!", 10);
                    MessageRenderer.Instance.PostMainMessage("raceover.pix", 8, 0.7f, 0.003f, 1.4f);
                    SoundCache.Play(SoundIds.RaceCompleted, null, false);
                    SoundCache.Play(SoundIds.Clapping, null, false);
                    break;
                case CompletionType.Peds:
                    MessageRenderer.Instance.PostHeaderMessage("Every pedestrian murdered!!", 10);
                    MessageRenderer.Instance.PostMainMessage("raceover.pix", 8, 0.7f, 0.003f, 1.4f);
                    SoundCache.Play(SoundIds.RaceCompleted, null, false);
                    SoundCache.Play(SoundIds.Clapping, null, false);
                    break;
                case CompletionType.TimeUp:
                    MessageRenderer.Instance.PostMainMessage("timeup.pix", 8, 0.7f, 0.003f, 1.4f);
                    SoundCache.Play(SoundIds.OutOfTime, null, false);
                    break;
            }            
            
            _camera.ResetRotation();
            _camera.RotateTo(MathHelper.Pi * 2);
            Engine.Camera = _camera;
        }

        public override void Activate()
        {
            PlayerVehicleController.ForceBrake = true;
            foreach (IDriver driver in Race.Current.Drivers)
                if (driver is CpuDriver)
                    ((CpuDriver)driver).SetState(CpuDriverState.Sleeping);
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

            if (Engine.Input.WasPressed(Keys.Enter))
            {
                Engine.Game.Exit();
            }
        }

        public override void Render()
        {
            Race.Current.PlayerVehicle.Render();
        }
    }
}
