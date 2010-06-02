using System;
using System.Collections.Generic;
using System.Text;

namespace NFSEngine.Audio
{
    public interface ISoundEngine
    {
        void SetDefaultVolume(int volume);
        IListener GetListener();
        ISound Load(string name, bool is3d);
        //void Play(ISound sound, float duration);
        void Register3dSound(ISound sound);
        void Unregister3dSound(ISound sound);
        void Update();
    }
}
