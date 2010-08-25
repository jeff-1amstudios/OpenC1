using System;
using System.Collections.Generic;

using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Carmageddon.Parsers.Grooves;
using Carmageddon.Parsers.Funks;
using Microsoft.Xna.Framework.Graphics;

namespace Carmageddon.Parsers
{
    enum DepthCueMode
    {
        None,
        Dark,
        Fog
    }

    class CopStartPoint
    {
        public Vector3 Position;
        public bool IsSpecialForces;
    }
    
    class RaceFile : BaseTextFile
    {
        public List<string> MaterialFiles { get; private set; }
        public List<string> PixFiles { get; private set; }
        public string ModelFile { get; private set; }
        public string ActorFile { get; private set; }
        public string AdditionalActorFile { get; private set; }
        public string SkyboxTexture { get; private set; }
        public float SkyboxPositionY, SkyboxRepetitionsX;
        public DepthCueMode DepthCueMode { get; private set; }
        public Vector3 GridPosition;
        public float GridDirection;
        public List<NoncarFile> NonCars { get; set; }
        public List<CopStartPoint> CopStartPoints { get; set; }
        public List<BaseGroove> Grooves;
        public List<BaseFunk> Funks;
        public List<MaterialModifier> MaterialModifiers;
        public List<Color> SmokeTables;
        public List<Checkpoint> Checkpoints;
        public int LapCount;
        public List<SpecialVolume> SpecialVolumes;
        public List<OpponentPathNode> OpponentPathNodes;

        int _fileVersion;
        

        public RaceFile(string filename) : base(filename)
        {
            MaterialFiles = new List<string>();
            PixFiles = new List<string>();
                        
            string version = ReadLine();
            _fileVersion = int.Parse(version.Substring(8,1));
            
            GridPosition = ReadLineAsVector3() + new Vector3(0, GameVariables.Scale.Y*0.5f, 0);

            GridDirection = MathHelper.ToRadians(ReadLineAsInt());
            string initialTimerPerSkill = ReadLine();
            LapCount = ReadLineAsInt();
            SkipLines(3);  //race completed bonuses
            SkipLines(2); //?
            
            ReadCheckpointsSection();

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

            if (_fileVersion == 6)
            {
                int nbrLowMemModels = ReadLineAsInt();
                SkipLines(nbrLowMemModels);
            }

            ActorFile = ReadLine();
            if (_fileVersion == 6)
            {
                SkipLines(1); //low mem actor
            }
            AdditionalActorFile = ReadLine();

            SkyboxTexture = ReadLine().ToLower();
            SkyboxRepetitionsX = ReadLineAsInt();
            ReadLine();
            SkyboxPositionY = ReadLineAsInt();
            string cueMode = ReadLine().ToLower();
            if (cueMode == "dark") DepthCueMode = DepthCueMode.Dark;
            else if (cueMode == "fog") DepthCueMode = DepthCueMode.Fog;
            else DepthCueMode = DepthCueMode.Fog; //default to fog?
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

            ReadOpponentPathsSection();

            ReadCopStartPointsSection();

            ReadMaterialModifierSection();

            ReadNonCarSection();

            ReadSmokeTablesSection();
            
            CloseFile();
        }

        private void ReadCheckpointsSection()
        {
            Checkpoints = new List<Checkpoint>();

            int nbrCheckPoints = ReadLineAsInt();
            for (int i = 0; i < nbrCheckPoints; i++)
            {
                SkipLines(2);
                int quads = ReadLineAsInt();
                Debug.Assert(quads == 1);
                Checkpoint point = new Checkpoint { Number = i };
                List<Vector3> points = new List<Vector3>();
                points.Add(ReadLineAsVector3());
                points.Add(ReadLineAsVector3());
                points.Add(ReadLineAsVector3());
                points.Add(ReadLineAsVector3());
                point.BBox = BoundingBox.CreateFromPoints(points);
                Checkpoints.Add(point);
                SkipLines(2);
            }
        }

        private void ReadSpecialEffectsVolumes()
        {
            SpecialVolumes = new List<SpecialVolume>();
            int nbrSpecialEffectsVolumes = ReadLineAsInt();
            for (int i = 0; i < nbrSpecialEffectsVolumes; i++)
            {
                string name = ReadLine();

                SpecialVolume vol = new SpecialVolume();
                vol.Id = i;

                if (name != "DEFAULT WATER")
                {
                    Matrix m = ReadMatrix();
                                        
                    m = GameVariables.ScaleMatrix * Matrix.CreateScale(2) * m;
                    m.Translation = GameVariables.Scale * m.Translation;
                    vol.Matrix = m;
                }
                vol.Gravity = ReadLineAsFloat(false);
                vol.Viscosity = ReadLineAsFloat(false);
                vol.CarDamagePerMs = ReadLineAsFloat(false);
                vol.PedDamagePerMs = ReadLineAsFloat(false);
                vol.CameraEffectIndex = ReadLineAsInt();
                vol.SkyColor = ReadLineAsInt();
                vol.WindscreenMaterial = ReadLine();
                vol.EntrySoundId = ReadLineAsInt();
                vol.ExitSoundId = ReadLineAsInt();
                vol.EngineSoundIndex = ReadLineAsInt();
                vol.MaterialIndex = ReadLineAsInt();
                SpecialVolumes.Add(vol);
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

        private void ReadOpponentPathsSection()
        {
            Debug.Assert(ReadLine() == "START OF OPPONENT PATHS");
            
            int nbrNodes = ReadLineAsInt();

            OpponentPathNodes = new List<OpponentPathNode>();
            
            for (int i = 0; i < nbrNodes; i++)
            {
                OpponentPathNodes.Add(new OpponentPathNode { Position = ReadLineAsVector3(), Number=i });
            }

            int nbrSections = ReadLineAsInt();
            for (int i = 0; i < nbrSections; i++)
            {
                string[] tokens = ReadLine().Split(',');

                OpponentPathNode startNode = OpponentPathNodes[int.Parse(tokens[0])];

                OpponentPath path = new OpponentPath();
                path.Number = i;
                path.Start = startNode;
                path.End = OpponentPathNodes[int.Parse(tokens[1])];
                path.MinSpeedAtEnd = float.Parse(tokens[4]) * 2.2f; //speeds are in BRU (BRender units). Convert to game speed
                path.MaxSpeedAtEnd = float.Parse(tokens[5]) * 2.2f;
                path.Width = float.Parse(tokens[6]) * 6.5f;
                path.Type = (PathType)int.Parse(tokens[7]);

                startNode.Paths.Add(path);
            }
        }

        private void ReadCopStartPointsSection()
        {
            CopStartPoints = new List<CopStartPoint>();
            int nbrPoints = ReadLineAsInt();
            for (int i = 0; i < nbrPoints; i++)
            {
                string[] tokens = ReadLine().Split(',');
                Vector3 pos = new Vector3(float.Parse(tokens[0]), float.Parse(tokens[1]), float.Parse(tokens[2]));
                pos *= GameVariables.Scale;
                pos += new Vector3(0, 2, 0);
                CopStartPoints.Add(new CopStartPoint { Position = pos, IsSpecialForces = tokens[3].Contains("9") });
            }
            Debug.Assert(ReadLine() == "END OF OPPONENT PATHS");
        }

        private void ReadMaterialModifierSection()
        {
            MaterialModifiers = new List<MaterialModifier>();

            int nbrMaterialModifiers = ReadLineAsInt();
            for (int i = 0; i < nbrMaterialModifiers; i++)
            {
                MaterialModifier modifier = new MaterialModifier
                    {
                        CarWallFriction = ReadLineAsFloat(false),
                        TyreRoadFriction = ReadLineAsFloat(false),
                        Downforce = ReadLineAsFloat(false),
                        Bumpiness = ReadLineAsFloat(false),
                        TyreSoundIndex = ReadLineAsInt(),
                        CrashSoundIndex = ReadLineAsInt(),
                        ScrapeSoundIndex = ReadLineAsInt(),
                        Sparkiness = ReadLineAsFloat(false),
                        SmokeTableIndex = ReadLineAsInt()
                    };
                string matName = ReadLine();
                if (matName != "none")
                {
                    if (!File.Exists(GameVariables.BasePath + "data\\material\\" + matName))
                    {
                        matName = "SKIDMARK.MAT"; //default skidmark if invalid (in indust maps, this is "1" and skidmarks aren't shown)
                    }
                    MatFile matFile = new MatFile(GameVariables.BasePath + "data\\material\\" + matName);
                    modifier.SkidMaterial = matFile.Materials[0];
                    modifier.SkidMaterial.ResolveTexture();
                }
                MaterialModifiers.Add(modifier);
            }
        }

        private void ReadNonCarSection()
        {
            NonCars = new List<NoncarFile>();

            int nbrNonCars = ReadLineAsInt();
            for (int i = 0; i < nbrNonCars; i++)
            {
                string filename = ReadLine();
                NoncarFile file = new NoncarFile(GameVariables.BasePath + "Data\\Noncars\\" + filename);
                NonCars.Add(file);
            }
        }

        private void ReadSmokeTablesSection()
        {
            SmokeTables = new List<Color>();
            int nbrSmokeTables = ReadLineAsInt();
            for (int i = 0; i < nbrSmokeTables; i++)
            {
                SmokeTables.Add(ReadLineAsColor());
                ReadLine();  //strengths
            }

            // now we have smoke tables, initialize material modifiers

            foreach (MaterialModifier modifier in MaterialModifiers)
                modifier.Initialize(this);
        }
    }
}
