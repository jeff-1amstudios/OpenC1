using System;
using System.Collections.Generic;

using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Carmageddon.Parsers.Grooves;
using Carmageddon.Parsers.Funks;

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
        public List<NoncarFile> NonCars { get; set; }
        public List<Vector3> CopStartPoints { get; set; }
        public List<BaseGroove> Grooves;
        public List<BaseFunk> Funks;

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
            SkyboxPositionY = ReadLineAsInt();
            DepthCueMode = ReadLine().ToLower();
            ReadLine(); //degree of dark

            int defaultEngineNoise = ReadLineAsInt();
            
            ReadSpecialEffectsVolumes();

            SkipLines(3);  //reflective windscreen stuff
            int nbrAlternativeReflections = ReadLineAsInt();
            SkipLines(nbrAlternativeReflections * 2);
            SkipLines(5); // map name, map matrix

            ReadFunkSection();

            ReadGrooveSection();

            ReadPedestrianSection();

            ReadOpponentPathSection();

            ReadCopStartPointsSection();

            ReadMaterialModifierSection();

            ReadNonCarSection();
            
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
            Debug.Assert(ReadLine() == "START OF FUNK");
            Funks = new List<BaseFunk>();
            FunkReader reader = new FunkReader();

            while (!reader.AtEnd)
            {
                BaseFunk f = reader.Read(this);
                if (f != null) Funks.Add(f);
            }
        }

        private void ReadGrooveSection()
        {
            string start = ReadLine();
            Debug.Assert(start == "START OF GROOVE");
            Grooves = new List<BaseGroove>();
            GrooveReader reader = new GrooveReader();
            while (!reader.AtEnd)
            {
                BaseGroove g = reader.Read(this);
                if (g != null) Grooves.Add(g);
            }
        }

        private void ReadPedestrianSection()
        {
            ReadLine(); //# of pedsubs
            int nbrPeds = ReadLineAsInt();

            for (int i = 0; i < nbrPeds; i++)
            {
                int pedNbr = ReadLineAsInt();
                int nbrInstructions = ReadLineAsInt();
                int initialInstruction = ReadLineAsInt();
                for (int j = 0; j < nbrInstructions; j++)
                {
                    string instruction = ReadLine();
                    if (instruction == "point") ReadLine(); //point data
                    else if (instruction == "reverse") { }
                    else
                    {
                    }
                }
            }
        }

        private void ReadOpponentPathSection()
        {
            Debug.Assert(ReadLine() == "START OF OPPONENT PATHS");
            int nbrNodes = ReadLineAsInt();
            SkipLines(nbrNodes);
            int nbrSections = ReadLineAsInt();
            SkipLines(nbrSections);
        }

        private void ReadCopStartPointsSection()
        {
            int nbrPoints = ReadLineAsInt();
            for (int i = 0; i < nbrPoints; i++)
            {
                ReadLine();
                //CopStartPoints.Add(ReadLineAsVector3());
            }
            Debug.Assert(ReadLine() == "END OF OPPONENT PATHS");
        }

        private void ReadMaterialModifierSection()
        {
            int nbrMaterialModifiers = ReadLineAsInt();
            for (int i = 0; i < nbrMaterialModifiers; i++)
            {
                float carWallFriction = ReadLineAsFloat(false);
                float tyreRoadFriction = ReadLineAsFloat(false);
                float downforce = ReadLineAsFloat(false);
                float bumpiness = ReadLineAsFloat(false);
                int tyreSoundIndex = ReadLineAsInt();
                int crashSoundIndex = ReadLineAsInt();
                int scrapeSoundIndex = ReadLineAsInt();
                float sparkiness = ReadLineAsFloat(false);
                int expansion = ReadLineAsInt();
                string skidMaterial = ReadLine();
            }
        }

        private void ReadNonCarSection()
        {
            NonCars = new List<NoncarFile>();

            int nbrNonCars = ReadLineAsInt();
            for (int i = 0; i < nbrNonCars; i++)
            {
                string filename = ReadLine();
                NoncarFile file = new NoncarFile(GameVariables.BasePath + "\\Noncars\\" + filename);
                NonCars.Add(file);
            }
        }
    }
}
