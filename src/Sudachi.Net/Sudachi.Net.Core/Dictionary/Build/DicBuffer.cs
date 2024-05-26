using System;

namespace Sudachi.Net.Core.Dictionary.Build
{
    /**
      * Buffers dictionary data for writing into channels. Wrapper over a byte array.
      */

    public class DicBuffer
    {
        public const int MaxString = short.MaxValue;
        private readonly byte[] _buffer;
        private int _position;

        public DicBuffer(int length, int number)
            : this(length * number * 2 + number * 2)
        {
        }

        public DicBuffer(int size)
        {
            _buffer = new byte[size];
            _position = 0;
        }

        public static bool IsValidLength(string text)
        {
            return text.Length <= MaxString;
        }

        /**
         * Tries to put the string s into the buffer
         *
         * @param s the string to put into buffer
         * @return true if successful, false when the buffer does not have enough size.
         *         The buffer is not modified in that case.
         */

        public bool Put(string s)
        {
            int length = s.Length;
            if (!PutLength(length))
            {
                return false;
            }
            foreach (char c in s)
            {
                PutChar(c);
            }
            return true;
        }

        /**
         * Tries to put the length of a string into the buffer
         *
         * @param length the length of the string to put into buffer
         * @return true if successful, false when the buffer does not have enough size.
         *         The buffer is not modified in that case.
         */

        public bool PutLength(int length)
        {
            if (length >= MaxString)
            {
                throw new ArgumentException($"can't handle string with length >= {MaxString}");
            }
            int addLen = (length > byte.MaxValue) ? 2 : 1;
            if (WontFit(length * 2 + addLen))
            {
                return false;
            }
            if (length <= byte.MaxValue)
            {
                _buffer[_position++] = (byte)length;
            }
            else
            {
                _buffer[_position++] = (byte)((length >> 8) | 0x80);
                _buffer[_position++] = (byte)(length & 0xFF);
            }
            return true;
        }

        public T Consume<T>(IOConsumer<T> consumer)
        {
            T result = consumer(_buffer, 0, _position);
            _position = 0;
            return result;
        }

        public void PutShort(short val)
        {
            _buffer[_position++] = (byte)(val & 0xFF);
            _buffer[_position++] = (byte)((val >> 8) & 0xFF);
        }

        public void PutInt(int val)
        {
            _buffer[_position++] = (byte)(val & 0xFF);
            _buffer[_position++] = (byte)((val >> 8) & 0xFF);
            _buffer[_position++] = (byte)((val >> 16) & 0xFF);
            _buffer[_position++] = (byte)((val >> 24) & 0xFF);
        }

        public bool WontFit(int space)
        {
            return _buffer.Length - _position < space;
        }

        public int Position => _position;

        public void PutEmptyIfEqual(string field, string surface)
        {
            if (field == surface)
            {
                Put("");
            }
            else
            {
                Put(field);
            }
        }

        public void PutInts(int[] data)
        {
            _buffer[_position++] = (byte)data.Length;
            foreach (int v in data)
            {
                PutInt(v);
            }
        }

        private void PutChar(char c)
        {
            _buffer[_position++] = (byte)(c & 0xFF);
            _buffer[_position++] = (byte)((c >> 8) & 0xFF);
        }

        public delegate T IOConsumer<T>(byte[] buffer, int offset, int length);

        public byte[] Buffer => _buffer;
    }
}
