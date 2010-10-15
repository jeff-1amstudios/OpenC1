using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Carmageddon.Parsers
{
    class PedestriansFile : BaseTextFile
    {
        public List<PedestrianBehaviour> _pedestrians = new List<PedestrianBehaviour>();

        public PedestriansFile()
            : base(GameVars.BasePath + "data\\pedestrn.txt")
        {

            bool atEnd = false;

            while (true)
            {
                PedestrianBehaviour ped = new PedestrianBehaviour();
                
                string line = ReadLine();
                if (line == "END OF PEDESTRIANS")
                    break;

                _pedestrians.Add(ped);
                ped.RefNumber = int.Parse(line);
                ped.Height = ReadLineAsFloat();
                ped.PointsValue = ReadLineAsInt();
                ped.HitPoints = ReadLineAsFloat();
                int[] sounds = ReadLineAsIntList();
                ped.ExplodingSounds = new int[sounds.Length - 1];
                Array.Copy(sounds, 1, ped.ExplodingSounds, 0, ped.ExplodingSounds.Length);

                ped.FallingNoise = ReadLineAsInt();
                ped.Acceleration = ReadLineAsFloat(false);
                ped.ImageIndex = ReadLineAsInt();
                SkipLines(1);
                ped.PixFile = ReadLine();
                SkipLines(1);

                List<int> actionIndexes = new List<int>();
                List<PedestrianAction> actions = new List<PedestrianAction>();
                for (int i = 0; i < 6; i++)
                    actionIndexes.Add(ReadLineAsInt());

                int nbrActions = ReadLineAsInt();

                for (int i = 0; i < nbrActions; i++)
                {
                    PedestrianAction action = new PedestrianAction();
                    actions.Add(action);

                    int[] dangerline = ReadLineAsIntList();
                    action.DangerLevel = dangerline[0];
                    action.PercentageChance = dangerline[1];

                    float[] speeds = ReadLineAsFloatList();
                    action.InitialSpeed = speeds[0];
                    action.LoopingSpeed = speeds[1];
                    action.ReactionTime = ReadLineAsFloat(false);

                    sounds = ReadLineAsIntList();
                    action.Sounds = new int[sounds.Length - 1];
                    Array.Copy(sounds, 1, action.Sounds, 0, action.Sounds.Length);

                    int nbrActionSequences = ReadLineAsInt();

                    for (int j = 0; j < nbrActionSequences; j++)
                    {
                        PedestrianActionSequenceMap seq = new PedestrianActionSequenceMap();
                        int[] seqMap = ReadLineAsIntList();
                        seq.MaxBearing = seqMap[0];
                        seq.SequenceIndex = seqMap[1];
                        action.Sequences.Add(seq);
                    }
                }

                ped.Standing = actions[0];
                ped.Running = actions[1];
                ped.FatalImpact = actions[actionIndexes[0]];
                ped.NonFatalImpact = actionIndexes[1] > -1 ? actions[actionIndexes[1]] : null;
                ped.AfterNonFatalImpact = actionIndexes[2] > -1 ? actions[actionIndexes[2]] : null;
                ped.FatalFalling = actionIndexes[3] > -1 ? actions[actionIndexes[3]] : null;
                ped.NonFatalFalling = actionIndexes[4] > -1 ? actions[actionIndexes[4]] : null;
                ped.Giblets = actionIndexes[5] > -1 ? actions[actionIndexes[5]] : null;
                ped.Actions = actions;

                int nbrSequences = ReadLineAsInt(); // this is not always correct (MOO 2)
                while (true)
                {
                    PedestrianSequence seq = new PedestrianSequence();
                    line = ReadLine();

                    if (line == "END OF PEDESTRIANS")
                    {
                        atEnd = true;
                        break;
                    }
                    if (line == "NEXT PEDESTRIAN")
                        break;

                    seq.Collide = (line == "collide");
                    seq.FrameRateType = (PedestrianSequenceFrameRate)Enum.Parse(typeof(PedestrianSequenceFrameRate), ReadLine(), true);
                    if (seq.FrameRateType != PedestrianSequenceFrameRate.Fixed)
                    {
                        float[] framespeeds = ReadLineAsFloatList();
                        seq.MinFrameRate = framespeeds[0];
                        seq.MaxFrameRate = framespeeds[1];
                    }

                    int nbrFrames = ReadLineAsInt();
                    seq.InitialFrames = new List<PedestrianFrame>();
                    for (int j = 0; j < nbrFrames; j++)
                    {
                        PedestrianFrame frame = new PedestrianFrame();
                        frame.PixName = ReadLine();
                        string[] flags = ReadLine().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        frame.Flipped = flags[2] == "flipped";
                        frame.Offset.X = float.Parse(flags[0]);
                        frame.Offset.Y = float.Parse(flags[1]);
                        frame.Offset *= GameVars.Scale;
                        seq.InitialFrames.Add(frame);
                    }

                    nbrFrames = ReadLineAsInt();
                    seq.LoopingFrames = new List<PedestrianFrame>();
                    for (int j = 0; j < nbrFrames; j++)
                    {
                        PedestrianFrame frame = new PedestrianFrame();
                        frame.PixName = ReadLine();
                        string[] flags = ReadLine().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        frame.Flipped = flags[2] == "flipped";
                        frame.Offset.X = float.Parse(flags[0]);
                        frame.Offset.Y = float.Parse(flags[1]);
                        seq.LoopingFrames.Add(frame);
                    }
                    ped.Sequences.Add(seq);
                }

                if (atEnd)
                    break;
            }

            CloseFile();
        }
    }
}
