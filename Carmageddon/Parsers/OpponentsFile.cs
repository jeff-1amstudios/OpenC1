using System;
using System.Collections.Generic;
using System.Text;

namespace Carmageddon.Parsers
{
    class OpponentInfo
    {
        public string Name;
        public string FileName;
        public int StrengthRating;
    }

    class OpponentsFile : BaseTextFile
    {
        public List<OpponentInfo> Opponents;

        static OpponentsFile _instance;

        public static OpponentsFile Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new OpponentsFile();
                return _instance;
            }
        }

        private OpponentsFile()
            : base(GameVars.BasePath + "data\\opponent.txt")
        {

            Opponents = new List<OpponentInfo>();
            int nbrOpponents = ReadLineAsInt();

            for (int i = 0; i < nbrOpponents; i++)
            {
                OpponentInfo opp = new OpponentInfo();
                opp.Name = ReadLine();
                SkipLines(2); //short name, race #
                opp.StrengthRating = ReadLineAsInt();
                SkipLines(2); //network availability, unused fli
                opp.FileName = ReadLine();
                SkipLines(1); //fli

                int nbrTextChunks = ReadLineAsInt();
                for (int j = 0; j < nbrTextChunks; j++)
                {
                    SkipLines(2);
                    int nbrLines = ReadLineAsInt();
                    SkipLines(nbrLines);
                }

                Opponents.Add(opp);
            }

            CloseFile();
        }
    }
}
