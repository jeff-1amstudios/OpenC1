using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StillDesign.PhysX;

namespace OpenC1
{
    class CActor
    {
        public string Name { get; private set; }
        public string ModelName { get; set; }
        public CModel Model { get; set; }
        public string MaterialName { get; set; }
        public CMaterial Material { get; set; }
        public Matrix Matrix, ParentMatrix;
        public CActor Parent;
        internal List<CActor> Children { get; set; }
        public BoundingBox BoundingBox;
        public byte[] Flags;
        public bool IsWheel;
        public StillDesign.PhysX.Actor PhysXActor { get; private set; }
        public bool IsAnimated;
        

        public CActor()
        {
            Children = new List<CActor>();
        }

        public void SetName(string name)
        {
            Name = name;
            IsWheel = (name.StartsWith("FLPIVOT") || name.StartsWith("FRPIVOT") || name.StartsWith("RLWHEEL") || name.StartsWith("RRWHEEL")
                || name.StartsWith("MLWHEEL") || name.StartsWith("MRWHEEL"));
        }

        internal void AttachToPhysX(Actor instance)
        {
            // if this CActor is attached to a PhysX object, reduce the Matrix to a scale, 
            // as the position/orienation will come from PhysX
            PhysXActor = instance;
            Vector3 scaleout, transout;
            Quaternion b;
            Matrix.Decompose(out scaleout, out b, out transout);
            Matrix = Matrix.CreateScale(scaleout);
        }

        public Matrix GetDynamicMatrix()
        {
            if (PhysXActor == null) return Matrix;
            return Matrix * PhysXActor.GlobalPose;
        }
    }
}
