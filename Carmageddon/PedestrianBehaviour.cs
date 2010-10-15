using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Carmageddon
{
    class PedestrianBehaviour
    {
        public int RefNumber;
        public float Height;
        public int PointsValue;
        public float HitPoints;
        public int[] ExplodingSounds;
        public int FallingNoise;
        public float Acceleration;
        public int ImageIndex;
        public string PixFile;

        public PedestrianAction Standing, Running, FatalImpact, NonFatalImpact, AfterNonFatalImpact, FatalFalling, NonFatalFalling, Giblets;
        public List<PedestrianSequence> Sequences = new List<PedestrianSequence>();
        public List<PedestrianAction> Actions = new List<PedestrianAction>();
    }


    class PedestrianAction
    {
        public int DangerLevel;
        public int PercentageChance;
        public float InitialSpeed, LoopingSpeed;
        public float ReactionTime;
        public int[] Sounds;
        public List<PedestrianActionSequenceMap> Sequences = new List<PedestrianActionSequenceMap>();
    }

    public class PedestrianActionSequenceMap
    {
        public int MaxBearing;
        public int SequenceIndex;
    }

    class PedestrianSequence
    {
        public bool Collide;
        public PedestrianSequenceFrameRate FrameRateType;
        public float MinFrameRate, MaxFrameRate;
        public List<PedestrianFrame> InitialFrames = new List<PedestrianFrame>();
        public List<PedestrianFrame> LoopingFrames = new List<PedestrianFrame>();
    }

    enum PedestrianSequenceFrameRate
    {
        Variable,
        Fixed,
        Speed
    }

    class PedestrianFrame
    {
        public string PixName;
        public Texture2D Texture;
        public bool Flipped;
        public Vector3 Offset;
    }
}
