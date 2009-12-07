using System;
using System.Collections.Generic;
using System.Text;

namespace Carmageddon.Parsers.Funks
{
    class FunkReader
    {
        public bool AtEnd;

        public BaseFunk Read(BaseTextFile file)
        {
            string materialName = file.ReadLine();

            if (materialName == "END OF FUNK")
            {
                AtEnd = true;
                return null;
            }

            string activation = file.ReadLine();
            string animation = file.ReadLine().ToUpper();
            while (animation != "FLIC" && animation != "FRAMES" && animation != "ROLL"
                && animation != "THROB" && animation != "SLITHER")
            {
                animation = file.ReadLine().ToUpper();
            }

            if (animation == "FLIC")
            {
                file.ReadLine(); //approximate/accurate

                FlicFunk flic = new FlicFunk();
                flic.MaterialName = materialName;
                flic.FliName = file.ReadLine();
                ReadToEndOfFunk(file);
                return flic;
            }
            else if (animation == "FRAMES")
            {
                file.ReadLine(); //approximate/accurate
                file.ReadLine(); //continuous

                FramesFunk frames = new FramesFunk();
                frames.MaterialName = materialName;

                frames.Speed = 1 / file.ReadLineAsFloat();
                int nbrFrames = file.ReadLineAsInt();
                for (int i = 0; i < nbrFrames; i++)
                {
                    frames.FrameNames.Add(file.ReadLine());
                }
                ReadToEndOfFunk(file);
                return frames;
            }
            else if (animation == "ROLL")
            {
                string loop = file.ReadLine();
                RollFunk roll = new RollFunk();
                roll.MaterialName = materialName;
                roll.Speed = file.ReadLineAsVector2(false);
                ReadToEndOfFunk(file);
                return roll;
            }
            else if (animation == "SLITHER")
            {
                SlitherFunk slither = new SlitherFunk();
                slither.MaterialName = materialName;
                slither.Motion = (Motion)Enum.Parse(typeof(Motion), file.ReadLine(), true);
                slither.CyclesPerSecond = file.ReadLineAsVector2(false);
                slither.MoveDistance = file.ReadLineAsVector2(false)/12;
                slither.Initialize();
                ReadToEndOfFunk(file);
                return slither;
            }
            else if (animation == "THROB")
            {
                ThrobFunk throb = new ThrobFunk();
                throb.MaterialName = materialName;
                throb.Motion = (Motion)Enum.Parse(typeof(Motion), file.ReadLine(), true);
                throb.CyclesPerSecond = file.ReadLineAsVector2(false);
                throb.MoveDistance = file.ReadLineAsVector2(false) / 12;
                throb.Initialize();
                ReadToEndOfFunk(file);
                return throb;
            }
            else
            {
                ReadToEndOfFunk(file);
                return null;
            }
        }

        private void ReadToEndOfFunk(BaseTextFile file)
        {
            while (true)
            {
                string line = file.ReadLine();
                if (line == "NEXT FUNK")
                    return;
                if (line == "END OF FUNK")
                {
                    AtEnd = true;
                    return;
                }
            }
        }
    }
}
