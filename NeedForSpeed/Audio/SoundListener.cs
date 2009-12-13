using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.DirectSound;
using Microsoft.Xna.Framework;


namespace Carmageddon.Audio
{
	class SoundListener
	{
		Listener3D _listener;

		internal SoundListener(Device device)
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
				orientation.Front = value.Forward.ToMdx();
				orientation.Top = value.Up.ToMdx();
				_listener.Orientation = orientation;
			}
		}

		public Vector3 Position
		{
			set { _listener.Position = value.ToMdx(); }
		}

		public Vector3 Velocity
		{
			set { _listener.Velocity = value.ToMdx(); }
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
