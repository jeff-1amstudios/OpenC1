using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace NFSEngine.Audio
{
    public interface IListener
    {
        void SetOrientation(Vector3 forward);
        Matrix Orientation { set; }
        Vector3 Position { get;  set; }
        Vector3 Velocity { set; }
        float DistanceFactor { set; }
        float RolloffFactor { set; }
        void BeginUpdate();
        void CommitChanges();
    }
}
