using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.DirectSound;
using Microsoft.Xna.Framework;

namespace Carmageddon.Audio
{
	class Sound
	{
		SecondaryBuffer _buffer;
		Buffer3D _buffer3d;

		internal Sound(Device device, string filename)
		{
			BufferDescription desc = new BufferDescription();
			desc.Control3D = true;
			desc.ControlVolume = true;
			desc.ControlFrequency = true;
			desc.Guid3DAlgorithm = DSoundHelper.Guid3DAlgorithmDefault;

			_buffer = new SecondaryBuffer(filename, desc, device);

			_buffer3d = new Buffer3D(_buffer);
			_buffer3d.Mode = Mode3D.Normal;			
		}

		public Vector3 Position
		{
			set { _buffer3d.Position = value.ToMdx(); }
		}

		public Vector3 Velocity
		{
			set { _buffer3d.Velocity = value.ToMdx(); }
		}

		public int Frequency
		{
			set { _buffer.Frequency = value; }
		}

		public void Play(bool loop)
		{
			_buffer.Play(0, loop ? BufferPlayFlags.Looping : BufferPlayFlags.Default);
		}

		public void Stop()
		{
			_buffer.Stop();
		}

		public void Reset()
		{
			_buffer.SetCurrentPosition(0);
		}		
	}
}
