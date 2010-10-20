using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework;
using System.Threading;
using OneAmEngine;
using System.Globalization;

namespace Carmageddon.Screens
{
    class LoadRaceScreen : IGameScreen
    {
        Texture2D _loadingTexture;

        public IGameScreen Parent { get; set; }
        private Thread _loadRaceThread;
        PlayGameScreen _raceScreen;

        public LoadRaceScreen(IGameScreen parent)
        {
            Parent = parent;
            _loadingTexture = new PixFile("LOADSCRN.pix").PixMaps[0].Texture;

            _loadRaceThread = new Thread(LoadRaceThreadProc);
            _loadRaceThread.Start();
        }

        public void Update()
        {
            if (_loadRaceThread.ThreadState != ThreadState.Running)
            {
                Engine.Screen = _raceScreen;
            }
        }

        public void Render()
        {
            Engine.SpriteBatch.Begin();
            Engine.SpriteBatch.Draw(_loadingTexture, new Rectangle(0, 0, Engine.Window.Width, Engine.Window.Height), Color.White);
            Engine.SpriteBatch.End();
        }

        private void LoadRaceThreadProc()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            _raceScreen = new PlayGameScreen(Parent);
        }
    }
}
