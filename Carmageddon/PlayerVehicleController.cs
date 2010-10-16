using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;
using OneAmEngine;

namespace Carmageddon
{
    class PlayerVehicleController
    {
        public static bool ForceBrake { get; set; }

        public static float Acceleration
        {
            get
            {
                if (ForceBrake)
                    return 0;

                if (Engine.Input.IsKeyDown(Keys.Up))
                    return 1.0f;

                return Engine.Input.GamePadState.Triggers.Right;
            }
        }

        public static float Brake
        {
            get
            {
                if (ForceBrake)
                    return 0f;

                if (Engine.Input.IsKeyDown(Keys.Down))
                    return 1.0f;

                return Engine.Input.GamePadState.Triggers.Left;
            }
        }

        public static float Turn
        {
            get
            {
                if (Engine.Input.IsKeyDown(Keys.Left))
                    return -1;
                else if (Engine.Input.IsKeyDown(Keys.Right))
                    return 1;

                return Engine.Input.GamePadState.ThumbSticks.Left.X;
            }
        }

        public static bool ChangeView
        {
            get
            {
                if (Engine.Input.WasPressed(Keys.C))
                    return true;
                if (Engine.Input.WasPressed(Buttons.RightShoulder))
                    return true;
                return false;
            }
        }

        public static bool GearUp
        {
            get
            {
                if (Engine.Input.WasPressed(Keys.A))
                    return true;
                if (Engine.Input.WasPressed(Buttons.B))
                    return true;
                return false;
            }
        }

        public static bool GearDown
        {
            get
            {
                if (Engine.Input.WasPressed(Keys.Z))
                    return true;
                if (Engine.Input.WasPressed(Buttons.X))
                    return true;
                return false;
            }
        }

        public static bool Handbrake
        {
            get
            {
                if (ForceBrake) return true;
                return Engine.Input.IsKeyDown(Keys.Space);
            }
        }
    }
}
