using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Carmageddon.Parsers
{
    class RaceFile : BaseTextFile
    {
        public List<string> MaterialFiles { get; private set; }
        public List<string> PixFiles { get; private set; }
        public string ModelFile { get; private set; }
        public string ActorFile { get; private set; }
        public string AdditionalActorFile { get; private set; }
        public string SkyboxTexture { get; private set; }
        public string DepthCueMode { get; private set; }

        public RaceFile(string filename) : base(filename)
        {

            MaterialFiles = new List<string>();
            PixFiles = new List<string>();
                        
            string version = ReadLine();
            string gridPosition = ReadLine();
            int gridDirection = ReadLineAsInt();
            string initialTimerPerSkill = ReadLine();
            int lapCount = ReadLineAsInt();
            SkipLines(3);
            SkipLines(2); //checkpoint width/height ?
            int nbrCheckPoints = ReadLineAsInt();
            SkipLines(9 * nbrCheckPoints);

            int nbrPixMaps = ReadLineAsInt();
            for (int i = 0; i < nbrPixMaps; i++)
            {
                PixFiles.Add(ReadLine());
            }
            int nbrPixMapsLowMem = ReadLineAsInt();
            SkipLines(nbrPixMapsLowMem);
            int nbrShadeTabs = ReadLineAsInt();
            SkipLines(nbrShadeTabs);

            int nbrMaterials = ReadLineAsInt();
            for (int i = 0; i < nbrMaterials; i++)
            {
                MaterialFiles.Add(ReadLine());
            }
            int nbrMaterialsLowMem = ReadLineAsInt();
            SkipLines(nbrMaterialsLowMem);

            int nbrModels = ReadLineAsInt();
            ModelFile = ReadLine();

            if (version == "VERSION 6")
            {
                int nbrLowMemModels = ReadLineAsInt();
                SkipLines(nbrLowMemModels);
            }

            ActorFile = ReadLine();
            SkipLines(1); //low mem actor
            AdditionalActorFile = ReadLine();

            SkyboxTexture = ReadLine();
            int skyboxRepetitionsX = ReadLineAsInt();
            ReadLine();
            ReadLine();
            DepthCueMode = ReadLine();

            CloseFile();
        }
    }
}
