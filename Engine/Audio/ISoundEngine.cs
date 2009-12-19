using System;
using System.Collections.Generic;
using System.Text;

namespace NFSEngine.Audio
{
    public interface ISoundEngine
    {
        IListener CreateListener();
        ISound Load(string name, bool is3d);
        void Play(ISound sound, float duration);
        void Update();
    }
}
