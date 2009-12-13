using System;
using System.Collections.Generic;

using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.Parsers.Grooves;

namespace Carmageddon.Parsers
{
    class CrushPoint
    {
        public int VertexIndex;
        public int Direction;
    }
    class CrushData
    {
        public int RefVertex;
        public Matrix Matrix;
        public Vector3 V1, V2;
        public List<CrushPoint> Points;

        public CrushData()
        {
            Points = new List<CrushPoint>();
        }
    }

    class CrushSection
    {
        public List<CrushData> Data;

        public CrushSection()
        {
            Data = new List<CrushData>();
        }
    }

    class PhysicalProperties
    {
        public List<Vector3> WheelPositions = new List<Vector3>();
        public BoundingBox BoundingBox;
        public List<Vector3> ExtraBoundingBoxPoints = new List<Vector3>();

        public float NonDrivenWheelRadius, DrivenWheelRadius;
        public float RideHeight;
        public float SuspensionDamping;
        public Vector3 CenterOfMass;
        public float Mass;
    }

    class CarFile : BaseTextFile
    {
        public List<string> MaterialFiles { get; private set; }
        public List<string> PixFiles { get; private set; }
        public string ModelFile { get; private set; }
        public string BonnetModelFile { get; private set; }
        public string ActorFile { get; private set; }
        public string BonnetActorFile { get; private set; }
        public List<CrushSection> CrushSections = new List<CrushSection>();
        public PhysicalProperties PhysicalProperties=new PhysicalProperties();
        public List<BaseGroove> Grooves;
        public int EngineNoiseId;

        public CarFile(string filename)
            : base(filename)
        {
            MaterialFiles = new List<string>();
            PixFiles = new List<string>();

            SkipLines(7);  //car name, pratcam shit

            string[] engineNoises = ReadLine().Split(',');
            EngineNoiseId = int.Parse(engineNoises[0]);
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
            ActorFile = ActorFile.Substring(ActorFile.IndexOf(",") + 1);  //this is in the format 0,Eagle.act
            BonnetActorFile = ReadLine();

            string windscreenReflection = ReadLine();
            int nbrSteerableWheels = ReadLineAsInt();
            Debug.Assert(nbrSteerableWheels == 2);
            int wheel1Ref = ReadLineAsInt();
            int wheel2Ref = ReadLineAsInt();
            SkipLines(6); //suspension refs

            PhysicalProperties.NonDrivenWheelRadius = ReadLineAsFloat() / 2f;
            PhysicalProperties.DrivenWheelRadius = ReadLineAsFloat() / 2f;

            ReadFunkSection();
            ReadGrooveSection();

            ReadCrushDataSection();

            ReadMechanicsSection();

            CloseFile();
        }

        private void ReadFunkSection()
        {
            Debug.Assert(ReadLine() == "START OF FUNK");
            while (ReadLine() != "END OF FUNK") { }
        }

        private void ReadGrooveSection()
        {
            Debug.Assert(ReadLine() == "START OF GROOVE");
            Grooves = new List<BaseGroove>();
            GrooveReader reader = new GrooveReader();

            while (!reader.AtEnd)
            {
                BaseGroove g = reader.Read(this);
                if (g != null) Grooves.Add(g);
            }
        }

        private void ReadCrushDataSection()
        {
            for (int i = 0; i < 3; i++)
            {
                CrushSection section = new CrushSection();
                CrushSections.Add(section);

                Debug.WriteLine("CRUSH " + i);
                SkipLines(6); //unk1
                int nbrData = ReadLineAsInt();

                for (int m = 0; m < nbrData; m++)
                {
                    CrushData crushData = new CrushData();
                    section.Data.Add(crushData);

                    crushData.RefVertex = ReadLineAsInt();
                    //crushData.Matrix = ReadMatrix();
                    crushData.V1 = ReadLineAsVector3();
                    crushData.V2 = ReadLineAsVector3();

                    SkipLines(2);

                    int nbrPoints = ReadLineAsInt();
                    int curVertex = 0; // crushData.RefVertex;

                    for (int p = 0; p < nbrPoints; p++)
                    {
                        CrushPoint point = new CrushPoint();
                        curVertex += ReadLineAsInt();
                        point.VertexIndex = curVertex;
                        point.Direction = ReadLineAsInt();
                        crushData.Points.Add(point);
                    }
                }
            }
        }

        private void ReadMechanicsSection()
        {
            string startOfMechanics = ReadLine();
            Debug.Assert(startOfMechanics.StartsWith("START OF MECHANICS"));

            for (int i = 0; i < 4; i++)
            {
                Vector3 wheelpos = ReadLineAsVector3();
            }

            PhysicalProperties.CenterOfMass = ReadLineAsVector3(true);
            if (startOfMechanics.EndsWith("2"))
            {
                int nbrBoxes = ReadLineAsInt();
                Debug.Assert(nbrBoxes == 1);
            }
            PhysicalProperties.BoundingBox = new BoundingBox(ReadLineAsVector3(), ReadLineAsVector3());

            if (!startOfMechanics.EndsWith("2"))
            {
                int nbrPoints = ReadLineAsInt();
                for (int i = 0; i < nbrPoints; i++)
                    PhysicalProperties.ExtraBoundingBoxPoints.Add(ReadLineAsVector3());
            }

            SkipLines(2);
            PhysicalProperties.RideHeight = ReadLineAsFloat();
            PhysicalProperties.SuspensionDamping = ReadLineAsFloat(false);
            PhysicalProperties.Mass = ReadLineAsFloat(false) * 1000;
        }
    }
}
