using System;

namespace Sudachi.Net.Core.Dictionary.Word
{
    internal class WordIdTable
    {
        private readonly Memory<byte> bytes;
        private readonly int size;
        private readonly int offset;
        private int dicIdMask = 0;

        public WordIdTable(Memory<byte> bytes, int offset)
        {
            this.bytes = bytes;
            // 4byte読み取ってintに変換してsizeとする。
            size = BitConverter.ToInt32(bytes.Span.Slice(offset, 4).ToArray(), 0);
            // 頭の4byteを読み飛ばす
            this.offset = offset + 4;
        }

        public int StorageSize()
        {
            return 4 + size;
        }

        public int[] Get(int index)
        {
            int length = bytes.Span[offset + index++];
            int[] result = new int[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = BitConverter.ToInt32(bytes.Span.Slice(offset + index, 4).ToArray(), 0);
                index += 4;
            }
            return result;
        }

        /**
         * Reads the word IDs to the passed WordLookup object
         *
         * @param index
         *            index in the word array
         * @param lookup
         *            object to read word IDs into
         * @return number of read IDs
         */

        public int ReadWordIds(int index, WordLookup lookup)
        {
            int offset = this.offset + index;
            Memory<byte> bytes = this.bytes;
            int length = bytes.Span[offset];
            offset += 1;
            int[] result = lookup.OutputBuffer(length);
            int dicIdMask = this.dicIdMask;
            for (int i = 0; i < length; i++)
            {
                int wordId = BitConverter.ToInt32(bytes.Span.Slice(offset, 4).ToArray(), 0);
                result[i] = WordId.ApplyMask(wordId, dicIdMask);
                offset += 4;
            }
            return length;
        }

        public void SetDictionaryId(int id)
        {
            dicIdMask = WordId.DicIdMask(id);
        }
    }
}
