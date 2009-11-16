using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Carmageddon.Parsers
{
    class NoncarFile : BaseTextFile
    {
        public int IndexNumber { get; set; }
        public Vector3 CenterOfMass, CenterOfMassWhenAttached;
        public BoundingBox BoundingBox;
        public List<Vector3> ExtraBoundingBoxPoints = new List<Vector3>();
        public float Mass, MassWhenAttached;
        public float BendAngleBeforeSnapping;
        
        public NoncarFile(string filename)
            : base(filename)
        {
            IndexNumber = ReadLineAsInt();
            CenterOfMass = ReadLineAsVector3(false);
            CenterOfMassWhenAttached = ReadLineAsVector3(false);
            BoundingBox = new BoundingBox(ReadLineAsVector3(), ReadLineAsVector3());

            int nbrExtraPoints = ReadLineAsInt();
            for (int i = 0; i < nbrExtraPoints; i++)
            {
                ExtraBoundingBoxPoints.Add(ReadLineAsVector3());
            }

            string massline = ReadLine();
            string[] masses = massline.Split(',');
            Mass = float.Parse(masses[0]) * 1000;
            MassWhenAttached = float.Parse(masses[1]) * 1000;

            ReadLine(); //ang mom dimensions
            BendAngleBeforeSnapping = ReadLineAsFloat();

            CloseFile();
        }
    }
}
