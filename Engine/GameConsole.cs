using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NFSEngine
{
    public static class GameConsole
    {
        static int _lines = 0;
        static Texture2D _shadow;
        static List<string> _scrollingLines = new List<string>();
        static int _currentLine;
        private static bool _enabled = true;

        static GameConsole()
        {
            _shadow = TextureGenerator.Generate(new Color(0, 0, 0, 200));
            for (int i = 0; i < 10; i++)
            {
                _scrollingLines.Add("");
            }
        }

        public static void Clear()
        {
            _lines = 0;
        }

        public static void WriteLine(object o)
        {
            if (!_enabled) return;
            Engine.Instance.DebugRenderer.AddText(new Vector2(5, ((_lines++)+1) * 18), o.ToString(), Justify.TOP_LEFT, Color.YellowGreen);
        }

        public static void WriteLine(string s, float o)
        {
            WriteLine(s + ": " + Math.Round(o, 2));
        }

        public static void WriteLine(string s, Vector3 vec)
        {
            WriteLine(s + ": " + vec.X.ToString("0.00, ") + vec.Y.ToString("0.00, ") + vec.Z.ToString("0.00"));
        }

        public static void WriteEvent(string evt){
            _scrollingLines[_currentLine++] = evt;
            if (_currentLine == _scrollingLines.Count) _currentLine = 0;
        }

        public static void Render()
        {
            if (!_enabled) return;

            WriteLine(" ");
            for (int i = 0; i < _scrollingLines.Count; i++)
            {
                WriteLine(_scrollingLines[(i + _currentLine) % _scrollingLines.Count]);
            }
            Engine.Instance.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            Engine.Instance.SpriteBatch.Draw(_shadow, new Rectangle(0, 0, 220, (_lines+1) * 18), Color.White);
            Engine.Instance.SpriteBatch.End();
        }
    }
}
