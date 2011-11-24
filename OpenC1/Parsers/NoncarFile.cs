using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace OpenC1.Parsers
{
    class NoncarFile : BaseTextFile
    {
        public int IndexNumber { get; set; }
        public Vector3 CenterOfMass, CenterOfMassWhenAttached;
        public BoundingBox BoundingBox;
        public List<Vector3> ExtraBoundingBoxPoints = new List<Vector3>();
        public float Mass, MassWhenAttached;
        public float BendAngleBeforeSnapping;
        public float TorqueRequiredToMove;
        
        public NoncarFile(string filename)
            : base(filename)
        {
            IndexNumber = ReadLineAsInt();
            CenterOfMass = ReadLineAsVector3();
            CenterOfMassWhenAttached = ReadLineAsVector3();
            BoundingBox = new BoundingBox(ReadLineAsVector3(), ReadLineAsVector3());

            int nbrExtraPoints = ReadLineAsInt();
            for (int i = 0; i < nbrExtraPoints; i++)
            {
                ExtraBoundingBoxPoints.Add(ReadLineAsVector3());
            }

            string massline = ReadLine();
            float[] masses = ReadLineAsFloatList();
            Mass = masses[0] * 1000;
            MassWhenAttached = masses[1] * 1000;
            
            ReadLine(); //ang mom dimensions
            BendAngleBeforeSnapping = ReadLineAsFloat(false);
            TorqueRequiredToMove = ReadLineAsFloat(false);

            CloseFile();

            if (IndexNumber == 1)  //PEDCROSS.TXT
            {
                //Physx doesnt handle the thin stem and thick box very well so we make PhysX happy here
                Vector3 oldMax = BoundingBox.Max;
                Vector3 oldMin = BoundingBox.Min;

                BoundingBox.Min.X = BoundingBox.Min.Z = -0.35f;
                BoundingBox.Max.X = BoundingBox.Max.Z = 0.35f;
                BoundingBox.Min.Y = 0.01f;
                ExtraBoundingBoxPoints.Clear();
                ExtraBoundingBoxPoints.Add(oldMax);
                ExtraBoundingBoxPoints.Add(new Vector3(oldMax.X, oldMax.Y, oldMin.Z));
                ExtraBoundingBoxPoints.Add(new Vector3(oldMin.X, oldMax.Y, oldMax.Z));
                ExtraBoundingBoxPoints.Add(new Vector3(oldMin.X, oldMax.Y, oldMin.Z));
            }
        }
    }
}
