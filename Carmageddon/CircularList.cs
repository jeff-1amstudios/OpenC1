using System;
using System.Collections.Generic;
using System.Text;

namespace Carmageddon
{
    class CircularList
    {
        float[] _list;
        int _ptr;
        public CircularList(int size)
        {
            _list = new float[size];
        }

        public void Add(float value)
        {
            _list[_ptr] = value;
            _ptr++;
            _ptr %= _list.Length;
        }

        public float GetMax()
        {
            float max = 0;
            for (int i = 0; i < _list.Length; i++)
            {
                if (_list[i] > max)
                    max = _list[i];
            }

            return max;
        }

        public void Clear()
        {
            for (int i = 0; i < _list.Length; i++)
            {
                _list[i] = 0;
            }
        }
    }
}
