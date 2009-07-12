using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Carmageddon.Parsers
{
    class CarFile : BaseTextFile
    {
        public List<string> MaterialFiles { get; private set; }
        public List<string> PixFiles { get; private set; }
        public string ModelFile { get; private set; }
        public string BonnetModelFile { get; private set; }
        public string ActorFile { get; private set; }
        public string BonnetActorFile { get; private set; }


        public CarFile(string filename) : base(filename)
        {
            MaterialFiles = new List<string>();
            PixFiles = new List<string>();
                        
            SkipLines(7);  //car name, pratcam shit

            string engineNoise = ReadLine();
            SkipLines(1); //stealworthy

            //jump over the damage info for now

            SkipLinesTillComment("Grid image");

            int nbrLowMemPix = ReadLineAsInt();
            SkipLines(nbrLowMemPix);
            int nbrStdDetailPix = ReadLineAsInt();
            SkipLines(nbrStdDetailPix);

            int nbrHighDetailPix = ReadLineAsInt();

            for (int i = 0; i < nbrHighDetailPix; i++)
                PixFiles.Add(ReadLine());

            int nbrShadeTables = ReadLineAsInt();
            SkipLines(nbrShadeTables);

            int nbrLowMemMats = ReadLineAsInt();
            SkipLines(nbrLowMemMats);
            int nbrStdDetailMats = ReadLineAsInt();
            SkipLines(nbrStdDetailMats);

            int nbrHighDetailMats = ReadLineAsInt();

            for (int i = 0; i < nbrHighDetailMats; i++)
                MaterialFiles.Add(ReadLine());

            int nbrModels = ReadLineAsInt();
            Debug.Assert(nbrModels == 3);
            string lowPolyMode = ReadLine();
            ModelFile = ReadLine();
            BonnetModelFile = ReadLine();

            int nbrActors = ReadLineAsInt();
            Debug.Assert(nbrActors == 3);
            string lowPolyActor = ReadLine();
            ActorFile = ReadLine();
            ActorFile = ActorFile.Substring(ActorFile.IndexOf(",")+1);  //this is in the format 0,Eagle.act
            BonnetActorFile = ReadLine();
            
            SkipLinesTillComment("START OF MECHANICS STUFF");
            //read stuff for physics when we have it

            CloseFile();
        }
    }
}
