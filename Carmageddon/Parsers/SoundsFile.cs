using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace OpenC1.Parsers
{
    class SoundsFile : BaseTextFile
    {
        public List<SoundDesc> Sounds = new List<SoundDesc>();

        public SoundsFile(string filename)
            : base(filename)
        {

            while (!_file.EndOfStream)
            {
                SoundDesc sound = new SoundDesc();
                string id = ReadLine();
                if (id == null) break;
                sound.Id = int.Parse(id);
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
