using System;
using System.Collections.Generic;
using System.Text;

namespace OpenC1.Parsers
{
    class RaceInfo
    {
        public string Name;
        public string FliFileName;
        public string RaceFilename;
        public string Description = "";

    }
    class RacesFile : BaseTextFile
    {
        public List<RaceInfo> Races;

        static RacesFile _instance;

        public static RacesFile Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RacesFile();
                return _instance;
            }
        }

        private RacesFile()
            : base(GameVars.BasePath + "data\\races.txt")
        {
            Races = new List<RaceInfo>();

            while (true)
            {
                RaceInfo info = new RaceInfo();
                info.Name = ReadLine();
                if (info.Name == "END") break;
                string files = ReadLine();
                info.FliFileName = files.Split(',')[0];
                info.RaceFilename = ReadLine();

                int nbrTextChunks = ReadLineAsInt();
                for (int i = 0; i < nbrTextChunks; i++)
                {
                    if (i == nbrTextChunks - 1) info.Description += "\r\n"; //add extra line before the big description chunk
                    SkipLines(2);
                    int lines = ReadLineAsInt();
                    for (int j = 0; j < lines; j++)
                        info.Description += ReadLine() + "\r\n";
                }

                Races.Add(info);
            }

            CloseFile();
        }
    }
}
