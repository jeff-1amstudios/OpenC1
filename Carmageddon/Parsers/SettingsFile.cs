using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework.Storage;
using System.IO;

namespace Carmageddon.Parsers
{

    class SettingsFile : BaseTextFile
    {

        public int DrawDistance;

        public SettingsFile()
            : base(Path.Combine(StorageContainer.TitleLocation, "OpenCarmaSettings.txt"))
        {
            DrawDistance = ReadLineAsInt();
            CloseFile();
        }
    }
}
