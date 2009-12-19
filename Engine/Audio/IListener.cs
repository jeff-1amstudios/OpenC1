using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace NFSEngine.Audio
{
    public interface IListener
    {
        Matrix Orientation { set; }
        Vector3 Position { set; }
        Vector3 Velocity { set; }
        void BeginUpdate();
        void CommitChanges();
    }
}
