using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.DirectSound;
using Microsoft.Xna.Framework;

namespace NFSEngine.Audio
{
	class MdxSound : ISound
	{
		SecondaryBuffer _buffer;
		Buffer3D _buffer3d;

        public int Id { get; set; }

        internal MdxSound(Device device, string filename, bool is3d)
		{
			BufferDescription desc = new BufferDescription();

            if (is3d)
            {
                desc.Control3D = true;
                desc.Guid3DAlgorithm = DSoundHelper.Guid3DAlgorithmDefault;
            }
			desc.ControlVolume = true;
			desc.ControlFrequency = true;
			
			_buffer = new SecondaryBuffer(filename, desc, device);

            if (is3d)
            {
                _buffer3d = new Buffer3D(_buffer);
                _buffer3d.Mode = Mode3D.Normal;
            }
		}

        public float Volume
        {
            set { _buffer.Volume = (int)value; }
        }

        public float Duration
        {
            get
            {
                return (float)_buffer.Caps.BufferBytes / (float)_buffer.Format.SamplesPerSecond;
            }
        }

		public Vector3 Position
		{
			set { _buffer3d.Position = MdxHelpers.ToMdx(value); }
		}

		public Vector3 Velocity
		{
			set { _buffer3d.Velocity = MdxHelpers.ToMdx(value); }
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

        public bool IsPlaying
        {
            get
            {
                return _buffer.Status.Playing;
            }
        }
	}
}
