using System;
using System.Collections.Generic;
using System.Text;

namespace Carmageddon.Parsers
{
    class SoundsFile : BaseTextFile
    {
        public List<CSound> Sounds = new List<CSound>();

        public SoundsFile(string filename)
            : base(filename)
        {

            while (!_file.EndOfStream)
            {
                CSound sound = new CSound();
                sound.Id = ReadLineAsInt();
                string[] flags = ReadLine().Split(',');
                sound.FileName = ReadLine();
                sound.Priority = ReadLineAsInt();
                sound.RepeatRate = ReadLineAsInt();
                sound.MinMaxVolume = ReadLine();
                sound.MinMaxPitch = ReadLine();
                sound.MinMaxSpeed = ReadLine();
                sound.SpecialFxIndex = ReadLineAsInt();

                int lowMemAlts = ReadLineAsInt();
                for (int i = 0; i < lowMemAlts; i++)
                    ReadLine();

                if (flags[0] == "0x00")
                {
                    Sounds.Add(sound);
                }

                if (sound.SpecialFxIndex != 0)
                {
                }
            }

            CloseFile();
        }
    }
}
