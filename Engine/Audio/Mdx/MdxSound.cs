using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.DirectSound;
using Microsoft.Xna.Framework;

namespace OneAmEngine.Audio
{
	class MdxSound : ISound
	{
		SecondaryBuffer _buffer;
		Buffer3D _buffer3d;
        bool _is3d;

        public int Id { get; set; }        
        public object Owner { get; set; }
        public bool MuteAtMaximumDistance { get; set; }

        internal MdxSound(Device device, string filename, bool is3d)
		{
			BufferDescription desc = new BufferDescription();

            if (is3d)
            {
                desc.Control3D = true;
                desc.Guid3DAlgorithm = DSoundHelper.Guid3DAlgorithmDefault;
                desc.Mute3DAtMaximumDistance = true;
            }
			desc.ControlVolume = true;
			desc.ControlFrequency = true;
			_buffer = new SecondaryBuffer(filename, desc, device);
            
            if (is3d)
            {
                _buffer3d = new Buffer3D(_buffer);
                _buffer3d.Mode = Mode3D.Normal;
                _is3d = true;
            }
		}

        public float Volume
        {
            get { return _buffer.Volume; }
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
            get
            {
                if (_buffer3d == null) return Vector3.Zero;
                return MdxHelpers.ToXna(_buffer3d.Position);
            }
            set
            {
                if (_buffer3d == null) return;
                _buffer3d.Position = MdxHelpers.ToMdx(value);
            }
		}

        public Vector3 Velocity
        {
            set
            {
                if (_buffer3d == null) return;
                _buffer3d.Velocity = MdxHelpers.ToMdx(value);
            }
        }

		public int Frequency
		{
			set { _buffer.Frequency = value; }
		}

		public void Play(bool loop)
		{
			_buffer.Play(0, loop ? BufferPlayFlags.Looping : BufferPlayFlags.Default);
            if (_is3d && loop && MuteAtMaximumDistance)
            {
                Engine.Audio.Register3dSound(this);
            }
		}

		public void Stop()
		{
			_buffer.Stop();
            _buffer.SetCurrentPosition(0);
            Engine.Audio.Unregister3dSound(this);
		}

        public void Pause()
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

        public float MaximumDistance
        {
            get
            {
                if (_buffer3d == null) return 0;
                return _buffer3d.MaxDistance;
            }
            set
            {
                if (_buffer3d == null) return;
                _buffer3d.MaxDistance = value;
            }
        }

        public float MinimumDistance
        {
            get
            {
                if (_buffer3d == null) return 0;
                return _buffer3d.MinDistance;
            }
            set
            {
                if (_buffer3d == null) return;
                _buffer3d.MinDistance = value;
            }
        }
	}
}
