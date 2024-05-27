using System;

namespace Sudachi.Net.Core.Dictionary.Word
{
    internal class WordParameterList
    {
        private static readonly int ELEMENT_SIZE = 2 * 3;

        private byte[] bytes;
        private readonly int size;
        private int offset;
        private bool isCopied;

        public WordParameterList(byte[] bytes, int offset)
        {
            this.bytes = bytes;
            size = BitConverter.ToInt32(bytes, offset);
            this.offset = offset + 4;
            isCopied = false;
        }

        public int StorageSize()
        {
            return 4 + ELEMENT_SIZE * size;
        }

        public int Size()
        {
            return size;
        }

        public short GetLeftId(int wordId)
        {
            return BitConverter.ToInt16(bytes, offset + ELEMENT_SIZE * wordId);
        }

        public short GetRightId(int wordId)
        {
            return BitConverter.ToInt16(bytes, offset + ELEMENT_SIZE * wordId + 2);
        }

        public short GetCost(int wordId)
        {
            return BitConverter.ToInt16(bytes, offset + ELEMENT_SIZE * wordId);
        }

        public void SetCost(int wordId, short cost)
        {
            if (!isCopied)
            {
                CopyBuffer();
            }
            byte[] costBytes = BitConverter.GetBytes(cost);
            Buffer.BlockCopy(costBytes, 0, bytes, offset + ELEMENT_SIZE * wordId + 4, 2);
        }

        public int EndOffset()
        {
            return offset + 4 + ELEMENT_SIZE * size;
        }

        private void CopyBuffer()
        {
            lock (this)
            {
                byte[] newBuffer = new byte[ELEMENT_SIZE * size];
                Buffer.BlockCopy(bytes, offset, newBuffer, 0, ELEMENT_SIZE * size);
                bytes = newBuffer;
                offset = 0;
                isCopied = true;
            }
        }
    }
}
