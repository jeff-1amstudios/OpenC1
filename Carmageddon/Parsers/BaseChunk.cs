using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace NeedForSpeed.Parsers
{
    class BaseChunk
    {
        protected long _offset;
        protected int _length;

        public virtual void Read(BinaryReader reader)
        {
            _offset = reader.BaseStream.Position - 4;
            _length = reader.ReadInt32();
        }

        public void SkipHeader(BinaryReader reader)
        {
            reader.BaseStream.Position += 4;
        }
    }
}
