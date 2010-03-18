﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.DirectSound;
using Microsoft.Xna.Framework;

namespace NFSEngine.Audio
{
	class MdxListener : IListener
	{
		Listener3D _listener;

        public MdxListener(Device device)
		{
			BufferDescription desc = new BufferDescription();
			desc.PrimaryBuffer = true;
			desc.Control3D = true;
			Microsoft.DirectX.DirectSound.Buffer buffer = new Microsoft.DirectX.DirectSound.Buffer(desc, device);
			_listener = new Listener3D(buffer);
			Orientation = Matrix.Identity;
		}

		public Matrix Orientation
		{
			set
			{
				Listener3DOrientation orientation = _listener.Orientation;
				orientation.Front = MdxHelpers.ToMdx(value.Forward);
				orientation.Top = MdxHelpers.ToMdx(value.Up);
				_listener.Orientation = orientation;
			}
		}

		public Vector3 Position
		{
			set { _listener.Position = MdxHelpers.ToMdx(value); }
		}

		public Vector3 Velocity
		{
			set { _listener.Velocity = MdxHelpers.ToMdx(value); }
		}

        public float DistanceFactor
        {
            set { _listener.DistanceFactor = value; }
        }

        public float RolloffFactor
        {
            set { _listener.RolloffFactor = value; }
        }

		public void BeginUpdate()
		{
			_listener.Deferred = true;
		}

		public void CommitChanges()
		{
			_listener.CommitDeferredSettings();
		}
	}
}
