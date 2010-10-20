using System;
using System.Collections.Generic;
using System.Text;

namespace OpenC1.Parsers
{
    class GeneralSettingsFile : BaseTextFile
    {
        static GeneralSettingsFile _instance;

        public static GeneralSettingsFile Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GeneralSettingsFile();
                return _instance;
            }
        }

        public int[] TimePerPedKill;
        public float[] TimePerCarDamage;
        public int[] TimePerCarKill;

        private GeneralSettingsFile()
            : base(GameVars.BasePath + "data\\general.txt")
        {
            SkipLines(6);
            int[] initialCredits = ReadLineAsIntList();
            SkipLines(9);
            TimePerPedKill = ReadLineAsIntList();
            
            TimePerCarDamage = ReadLineAsFloatList();
            SkipLines(1);
            TimePerCarKill = ReadLineAsIntList();

            CloseFile();

            // override the demo values with values from the full game...
            if (GameVars.Emulation == EmulationMode.Demo)
            {
                TimePerPedKill[0] = 20; TimePerPedKill[1] = 8; TimePerPedKill[2] = 5;
                TimePerCarKill[0] = 120; TimePerCarKill[1] = 90; TimePerCarKill[2] = 60;
            }
        }        
    }
}
