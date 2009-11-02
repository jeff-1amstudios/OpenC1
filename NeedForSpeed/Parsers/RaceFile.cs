using System;
using System.Collections.Generic;

using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using System.Diagnostics;

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
        public float SkyboxPositionY, SkyboxRepetitionsX;
        public string DepthCueMode { get; private set; }
        public Vector3 GridPosition;
        public int GridDirection;
        public Matrix WorldToMapTransform;

        public RaceFile(string filename) : base(filename)
        {
            MaterialFiles = new List<string>();
            PixFiles = new List<string>();
                        
            string version = ReadLine();
            
            GridPosition = ReadLineAsVector3() + new Vector3(0, GameVariables.Scale.Y, 0);

            GridDirection = ReadLineAsInt();
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
            if (version == "VERSION 6")
            {
                SkipLines(1); //low mem actor
            }
            AdditionalActorFile = ReadLine();

            SkyboxTexture = ReadLine().ToLower();
            SkyboxRepetitionsX = ReadLineAsInt();
            ReadLine();
            SkyboxPositionY = ReadLineAsInt() * GameVariables.Scale.Y;
            DepthCueMode = ReadLine().ToLower();
            ReadLine(); //degree of dark

            int defaultEngineNoise = ReadLineAsInt();
            
            ReadSpecialEffectsVolumes();

            SkipLines(3);  //reflective windscreen stuff
            int nbrAlternativeReflections = ReadLineAsInt();
            SkipLines(nbrAlternativeReflections * 2);
            SkipLines(5); // map name, map matrix

            ReadFunkSection();
            
            CloseFile();
        }

        private void ReadSpecialEffectsVolumes()
        {
            int nbrSpecialEffectsVolumes = ReadLineAsInt();
            for (int i = 0; i < nbrSpecialEffectsVolumes; i++)
            {
                string name = ReadLine();
                if (name != "DEFAULT WATER")
                {
                    Matrix m = ReadMatrix();
                }
                SkipLines(11);
            }
        }

        private void ReadFunkSection()
        {
            List<BaseFunk> funks = new List<BaseFunk>();
            string start = ReadLine();
            Debug.Assert(start == "START OF FUNK");
            while (true)
            {
                string line = ReadLine();
                if (line == "NEXT FUNK")
                    continue;
                else if (line == "END OF FUNK")
                    break;
                
                BaseFunk funk = new BaseFunk(this, line);
                funks.Add(funk);
            }
        }

        private void ReadGrooveSection()
        {

        }
    }
}
