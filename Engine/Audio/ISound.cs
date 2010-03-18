using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace NFSEngine.Audio
{

    public interface ISound
    {
        int Id { get; set; }
        float Duration { get; }
        float Volume { get;  set; }
        void Stop();
        void Play(bool loop);
        Vector3 Position { set; }
        Vector3 Velocity { set; }
        int Frequency { set; }
        bool IsPlaying { get; }
        float MaximumDistance { set; }
    }
}
