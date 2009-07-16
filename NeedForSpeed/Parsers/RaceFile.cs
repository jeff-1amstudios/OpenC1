using System;
using System.Collections.Generic;
using System.Linq;
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
            
            GridPosition = ReadLineAsVector3();

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
            int nbrSpecialEffectsVolumes = ReadLineAsInt();
            //SkipLines(12 + ((nbrSpecialEffectsVolumes - 1) * 16)); //first line is DEFAULT WATER (12 lines)

            //SkipLines(4); //reflective windscreen stuff

            //string mapPixName = ReadLine();
            //Debug.Assert(mapPixName.EndsWith(".PIX"));
            //WorldToMapTransform = new Matrix();
            //Vector3 matrixLine = ReadLineAsVector3(false);
            //WorldToMapTransform.M11 = matrixLine.X;
            //WorldToMapTransform.M12 = matrixLine.Y;
            //WorldToMapTransform.M13 = matrixLine.Z;
            //matrixLine = ReadLineAsVector3(false);
            //WorldToMapTransform.M21 = matrixLine.X;
            //WorldToMapTransform.M22 = matrixLine.Y;
            //WorldToMapTransform.M23 = matrixLine.Z;
            //matrixLine = ReadLineAsVector3(false);
            //WorldToMapTransform.M31 = matrixLine.X;
            //WorldToMapTransform.M32 = matrixLine.Y;
            //WorldToMapTransform.M33 = matrixLine.Z;
            //matrixLine = ReadLineAsVector3(false);
            //WorldToMapTransform.M41 = matrixLine.X;
            //WorldToMapTransform.M42 = matrixLine.Y;
            //WorldToMapTransform.M43 = matrixLine.Z;
            //WorldToMapTransform.M44 = 1;

            
            CloseFile();
        }
    }
}
