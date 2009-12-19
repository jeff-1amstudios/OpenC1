using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Carmageddon
{
    class CActor
    {
        public string Name { get; private set; }
        public string ModelName { get; set; }
        public CModel Model { get; set; }
        public string MaterialName { get; set; }
        public CMaterial Material { get; set; }
        public Matrix Matrix, ParentMatrix;
        internal List<CActor> Children { get; set; }
        public int Level { get; set; }
        public BoundingBox BoundingBox;
        public byte[] Flags;
        public bool IsWheel;
        internal StillDesign.PhysX.Actor _physXActor;
        public bool IsAnimated;

        public CActor()
        {
            Children = new List<CActor>();
        }

        public void SetName(string name)
        {
            Name = name;
            IsWheel = (name.StartsWith("FLPIVOT") || name.StartsWith("FRPIVOT") || name.StartsWith("RLWHEEL") || name.StartsWith("RRWHEEL"));
            //if (name.StartsWith("&"))
            //    IsDynamic = true;
        }

        internal void AttachPhysxActor(StillDesign.PhysX.Actor instance)
        {
            // if this CActor is attached to a PhysX object, reduce the Matrix to a scale, 
            // as the position/orienation will come from PhysX
            _physXActor = instance;
            Vector3 scaleout, transout;
            Quaternion b;
            Matrix.Decompose(out scaleout, out b, out transout);
            Matrix = Matrix.CreateScale(scaleout);
            //IsDynamic = true;
        }

        public Matrix GetDynamicMatrix()
        {
            if (_physXActor == null) return Matrix;
            return Matrix * _physXActor.GlobalPose;
        }
    }
}
