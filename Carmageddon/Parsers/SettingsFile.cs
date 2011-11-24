using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using OneAmEngine;

namespace OpenC1.Parsers
{

    class SettingsFile : BaseTextFile
    {
        public SettingsFile()
            : base(Path.Combine(StorageContainer.TitleLocation, "OpenC1Settings.txt"))
        {
            GameVars.DrawDistance = ReadLineAsInt() * 10;
            GameVars.FullScreen = ReadLineAsBool();
            
            CloseFile();
        }
    }
}
