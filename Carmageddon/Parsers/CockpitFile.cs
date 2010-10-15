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
        public Rectangle ForwardRect, LeftRect, RightRect;
        public List<CockpitHandFrame> LeftHands = new List<CockpitHandFrame>();
        public List<CockpitHandFrame> RightHands = new List<CockpitHandFrame>();
        public CockpitHandFrame CenterHands;
        public bool IsHighRes;
        

		public CockpitFile(string filename) : base(filename)
		{
            string folderName = Path.GetDirectoryName(filename);
            
            if (filename.Contains("64x48"))
                IsHighRes = true;

			Forward = GetTextureFromPixFile(folderName, ReadLine());
            ForwardRect = ReadLineAsRect();            
            Left = GetTextureFromPixFile(folderName, ReadLine());
            LeftRect = ReadLineAsRect();
            Right = GetTextureFromPixFile(folderName, ReadLine());
            RightRect = ReadLineAsRect();
			SkipLines(6); //internal & external speedo, tacho, gears
			int nbrHandFrames = ReadLineAsInt();

            int center = nbrHandFrames / 2;
            for (int i = 0; i < nbrHandFrames; i++)
            {
                CockpitHandFrame frame = new CockpitHandFrame();
                string[] frameParts = ReadLine().Split(',');
                frame.Position1 = new Vector2(int.Parse(frameParts[0]), int.Parse(frameParts[1]));
                frame.Position2 = new Vector2(int.Parse(frameParts[3]), int.Parse(frameParts[4]));
                frame.Texture1 = GetTextureFromPixFile(folderName, frameParts[2]);
                frame.Texture2 = GetTextureFromPixFile(folderName, frameParts[5]);
                if (i < center)
                    LeftHands.Insert(0, frame);
                else if (i == center)
                    CenterHands = frame;
                else
                    RightHands.Add(frame);
            }

			SkipLines(2); //mirror

			CloseFile();

            int w = 640; int h = 480;
            if (!IsHighRes)
            {
                w = 320; h = 200;
            }

            foreach (var x in LeftHands)
            {
                x.Position1 += new Vector2(-20, 0);
                x.Position2 += new Vector2(-20, 0);
                x.Position1 /= new Vector2(w, h);
                x.Position2 /= new Vector2(w, h);
            }
            foreach (var x in RightHands)
            {
                x.Position1 += new Vector2(-20, 0);
                x.Position2 += new Vector2(-20, 0);
                x.Position1 /= new Vector2(w, h);
                x.Position2 /= new Vector2(w, h);
            }
            CenterHands.Position1 += new Vector2(-20, 0);
            CenterHands.Position2 += new Vector2(-20, 0);
            CenterHands.Position1 /= new Vector2(w, h);
            CenterHands.Position2 /= new Vector2(w, h);
		}

        private Texture2D GetTextureFromPixFile(string curFolder, string filename)
        {
            if (filename == "none") return null;
            PixFile pixFile = new PixFile(filename);
            return pixFile.PixMaps[0].Texture;
        }
	}
}
