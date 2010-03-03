using System;
using System.Collections.Generic;
using System.Text;

namespace Carmageddon.Parsers
{
    class SoundsFile : BaseTextFile
    {
        public List<SoundDesc> Sounds = new List<SoundDesc>();

        public SoundsFile(string filename)
            : base(filename)
        {

            while (_file.BaseStream.Position < _file.BaseStream.Length)
            {
                SoundDesc sound = new SoundDesc();
                sound.Id = ReadLineAsInt();
                string[] flags = ReadLine().Split(',');
                sound.FileName = ReadLine();
                sound.Priority = ReadLineAsInt();
                sound.RepeatRate = ReadLineAsInt();
                sound.MinMaxVolume = ReadLine();
                sound.MinMaxPitch = ReadLine();
                sound.MinMaxSpeed = ReadLine();
                ReadLine(); //unused

                int lowMemAlts = ReadLineAsInt();
                for (int i = 0; i < lowMemAlts; i++)
                    ReadLine(); //unused

                if (flags[0] == "0x00")
                {
                    Sounds.Add(sound);
                }
            }
            CloseFile();
        }
    }
}
