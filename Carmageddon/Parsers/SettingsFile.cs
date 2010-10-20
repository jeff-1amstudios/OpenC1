using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using OneAmEngine;

namespace Carmageddon.Parsers
{

    class SettingsFile : BaseTextFile
    {

        public int DrawDistance;

        public SettingsFile()
            : base(Path.Combine(StorageContainer.TitleLocation, "OpenCarmaSettings.txt"))
        {
            GameVars.BasePath = ReadLine();
            GameVars.DrawDistance = ReadLineAsInt() * 10;
            string emu = ReadLine();
            GameVars.Emulation = (EmulationMode)Enum.Parse(typeof(EmulationMode), emu);
            GameVars.FullScreen = ReadLineAsBool();
            GameVars.DisableFog = ReadLineAsBool();
            
            CloseFile();
        }
    }
}
