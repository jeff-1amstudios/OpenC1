using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Carmageddon
{
    class SpecialVolume
    {
        public int Id;
        public BoundingBox BoundingBox;// Matrix;
        public float Gravity, Viscosity, CarDamagePerMs, PedDamagePerMs;
        public int CameraEffectIndex, SkyColor, EntrySoundId, ExitSoundId;
        public int EngineSoundIndex, MaterialIndex;
        public string WindscreenMaterial;

        public SpecialVolume Copy()
        {
            SpecialVolume vol2 = new SpecialVolume();
            vol2.Id = Id;
            vol2.Gravity = Gravity;
            vol2.Viscosity = Viscosity;
            vol2.CarDamagePerMs = CarDamagePerMs;
            vol2.PedDamagePerMs = PedDamagePerMs;
            vol2.CameraEffectIndex = CameraEffectIndex;
            vol2.SkyColor = SkyColor;
            vol2.EntrySoundId = EntrySoundId;
            vol2.ExitSoundId = ExitSoundId;
            vol2.EngineSoundIndex = EngineSoundIndex;
            vol2.MaterialIndex = MaterialIndex;
            vol2.WindscreenMaterial = WindscreenMaterial;
            return vol2;
        }
    }
}
