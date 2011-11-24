using System;
using System.Collections.Generic;
using System.Text;

namespace OpenC1.Parsers
{
    class FontDescriptionFile : BaseTextFile
    {
        public int Height;
        public int FirstChar;
        public int Padding;
        public int[] CharWidths;
        public float Scale = 1;

        public FontDescriptionFile()
        {
        }

        public FontDescriptionFile(string name) : base(name)
        {
            Height = ReadLineAsInt();
            SkipLines(1);
            Padding = ReadLineAsInt();
            FirstChar = ReadLineAsInt();
            int nbrChars = ReadLineAsInt();
            CharWidths = new int[nbrChars];

            for (int i = 0; i < nbrChars; i++)
            {
                CharWidths[i] = ReadLineAsInt();
            }

            CloseFile();
        }
    }
}
