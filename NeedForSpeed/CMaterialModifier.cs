using System;
using System.Collections.Generic;
using System.Text;
using StillDesign.PhysX;
using Carmageddon.Physics;
using Microsoft.Xna.Framework;
using PlatformEngine;

namespace Carmageddon
{
    class CMaterialModifier
    {
        public float CarWallFriction;
        public float TyreRoadFriction;
        public float Downforce;
        public float Bumpiness;
        public int TyreSoundIndex;
        public int CrashSoundIndex;
        public int ScrapeSoundIndex;
        public float Sparkiness;
        public int Expansion;
        public string SkidMaterial;

        private float _lastBump, _nextWheel;

        public void UpdateWheelShape(VehicleChassis chassis, VehicleWheel wheel)
        {
            if (Bumpiness > 0)
            {
                if (wheel.Index != _nextWheel ||_lastBump + 0.3f > Engine.Instance.TotalSeconds)
                    return;

                chassis.Body.AddForceAtLocalPosition(new Vector3(0, Bumpiness * 55, 0), wheel.Shape.LocalPosition, ForceMode.Impulse, true);
                _lastBump = Engine.Instance.TotalSeconds;
                _nextWheel = Engine.Instance.RandomNumber.Next(0, chassis.Wheels.Count - 1);
            }
        }
    }
}
