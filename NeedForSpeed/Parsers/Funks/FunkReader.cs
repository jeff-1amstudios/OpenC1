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

                int speed = file.ReadLineAsInt();
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
                roll.LoopSpeed = file.ReadLineAsVector2(false);
                ReadToEndOfFunk(file);
                return roll;
            }
            else
            {
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
