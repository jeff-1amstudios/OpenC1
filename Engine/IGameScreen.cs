using System;
using System.Collections.Generic;
using System.Text;

namespace OneAmEngine
{
    public interface IGameScreen
    {
        IGameScreen Parent { get; }
        void Update();
        void Render();
    }
}
