using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace NFSEngine.Audio
{

    public interface ISound
    {
        int Id { get; set; }
        object Owner { get; set; }
        float Duration { get; }
        float Volume { get; set; }
        void Pause();
        void Stop();
        void Play(bool loop);
        Vector3 Position { get; set; }
        Vector3 Velocity { set; }
        int Frequency { set; }
        bool IsPlaying { get; }
        float MinimumDistance { get; set; }
        float MaximumDistance { get; set; }
        //    bool 
    }   
}
