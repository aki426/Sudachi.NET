using Sudachi.Net.Core.Dictionary.Build;
using Sudachi.Net.Core.Utility;

namespace Sudachi.Net.Core.Dictionary.Word
{
    internal class WordIdTable
    {
        private readonly ByteBuffer bytes;
        private readonly int size;
        private readonly int offset;
        private int dicIdMask = 0;

        public WordIdTable(ByteBuffer bytes, int offset)
        {
            this.bytes = bytes;
            size = bytes.GetInt(offset);
            this.offset = offset + 4;
        }

        public int StorageSize()
        {
            return 4 + size;
        }

        public int[] Get(int index)
        {
            int length = bytes.Get(offset + index++);
            int[] result = new int[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = bytes.GetInt(offset + index);
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
            ByteBuffer bytes = this.bytes;
            int length = bytes.Get(offset);
            offset += 1;
            int[] result = lookup.OutputBuffer(length);
            int dicIdMask = this.dicIdMask;
            for (int i = 0; i < length; i++)
            {
                int wordId = bytes.GetInt(offset);
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
