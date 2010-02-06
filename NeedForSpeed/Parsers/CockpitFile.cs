using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace Carmageddon.Parsers
{
	class CockpitHandFrame
	{
		public Vector2 Position1, Position2;
		public Texture2D Texture1, Texture2;
	}

	class CockpitFile : BaseTextFile
	{
		public Texture2D Forward, Left, Right;
		public List<CockpitHandFrame> Hands = new List<CockpitHandFrame>();

		public CockpitFile(string filename) : base(filename)
		{
            string folderName = Path.GetDirectoryName(filename);

			Forward = GetTextureFromPixFile(folderName, ReadLine());
			ReadLine();
            Left = GetTextureFromPixFile(folderName, ReadLine());
			ReadLine();
            Right = GetTextureFromPixFile(folderName, ReadLine());
			ReadLine();
			SkipLines(6); //internal & external speedo, tacho, gears
			int nbrHandFrames = ReadLineAsInt();

            for (int i = 0; i < nbrHandFrames; i++)
            {
                CockpitHandFrame frame = new CockpitHandFrame();
                string[] frameParts = ReadLine().Split(',');
                frame.Position1 = new Vector2(int.Parse(frameParts[0]), int.Parse(frameParts[1]));
                frame.Position2 = new Vector2(int.Parse(frameParts[3]), int.Parse(frameParts[4]));
                frame.Texture1 = GetTextureFromPixFile(folderName, frameParts[2]);
                frame.Texture2 = GetTextureFromPixFile(folderName, frameParts[5]);
                Hands.Add(frame);
            }
			SkipLines(2); //mirror

			CloseFile();
		}

        private Texture2D GetTextureFromPixFile(string curFolder, string filename)
        {
            if (filename == "none") return null;
            PixFile pixFile = new PixFile(Path.Combine(curFolder, "..\\pixelmap\\" + filename));
            return pixFile.PixMaps[0].Texture;
        }
	}
}
