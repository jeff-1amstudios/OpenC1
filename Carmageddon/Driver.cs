using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using NFSEngine;
using Microsoft.Xna.Framework.Graphics;
using NFSEngine.Audio;

namespace Carmageddon
{
    class Driver : IPlayer
    {
        public Vector3 Position {get; set; }
        public Vector3 Velocity { get; set; }
        public Matrix Orientation { get; set; }

        public VehicleModel VehicleModel;

        IListener _audioListener;
                
        public Driver()
        {
            _audioListener = Engine.Audio.CreateListener();
            
        }

        public void Update()
        {
            _audioListener.BeginUpdate();
            _audioListener.Position = Position;
            _audioListener.Orientation = Orientation;
            _audioListener.Velocity = Velocity;
            _audioListener.CommitChanges();
        }
    }
}
