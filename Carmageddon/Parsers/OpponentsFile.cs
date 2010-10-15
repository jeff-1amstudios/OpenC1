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
                SkipLines(1); //short name, race #
                int raceNumber = ReadLineAsInt();
                opp.StrengthRating = ReadLineAsInt();
                SkipLines(1);
                if (GameVars.Emulation != EmulationMode.Demo)
                    SkipLines(1);
                opp.FileName = ReadLine();
                SkipLines(1); //fli

                int nbrTextChunks = ReadLineAsInt();
                for (int j = 0; j < nbrTextChunks; j++)
                {
                    SkipLines(2);
                    int nbrLines = ReadLineAsInt();
                    SkipLines(nbrLines);
                }

                if (GameVars.Emulation == EmulationMode.Demo)
                {
                    if (i != 0 && i != 16 && i != 12 && i != 22 && i != 19 && i != 7)
                        continue;
                }
                Opponents.Add(opp);
            }

            CloseFile();
        }
    }
}
