using System;
using System.Collections.Generic;
using System.Text;

namespace Sudachi.Net.Core.Dictionary.Build
{
    /**
      * Compiles model parameters into the binary format
      */

    public class Parameters : IWriteDictionary
    {
        private byte[] _data;
        private int _position;
        private int _maxLeft = int.MaxValue;
        private int _maxRight = int.MaxValue;

        public Parameters(int initialSize)
        {
            _data = new byte[initialSize];
            _position = 0;
        }

        public Parameters() : this(1024 * 1024) // default 1M
        {
        }

        public void Add(short left, short right, short cost)
        {
            MaybeResize();
            if (left >= _maxLeft)
            {
                throw new ArgumentException($"left {left} is larger than max value {_maxLeft}");
            }
            if (right >= _maxRight)
            {
                throw new ArgumentException($"right {right} is larger than max value {_maxRight}");
            }
            WriteShort(left);
            WriteShort(right);
            WriteShort(cost);
        }

        public void SetLimits(int left, int right)
        {
            _maxLeft = left;
            _maxRight = right;
        }

        private void MaybeResize()
        {
            if (_data.Length - _position < 6)
            {
                byte[] newData = new byte[_data.Length * 2];
                Array.Copy(_data, newData, _position);
                _data = newData;
            }
        }

        public void WriteTo(ModelOutput output)
        {
            output.WithPart("word parameters", () =>
            {
                output.Write(_data, 0, _position);
            });
        }

        private void WriteShort(short value)
        {
            _data[_position++] = (byte)(value & 0xFF);
            _data[_position++] = (byte)((value >> 8) & 0xFF);
        }
    }
}
