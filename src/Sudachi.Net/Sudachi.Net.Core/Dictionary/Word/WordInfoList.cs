using System;
using System.Collections.Generic;
using System.Text;

namespace Sudachi.Net.Core.Dictionary.Word
{
    internal class WordInfoList
    {
        private readonly ByteBuffer bytes;
        private readonly int offset;
        private readonly int wordSize;
        private readonly bool hasSynonymGid;

        public WordInfoList(ByteBuffer bytes, int offset, int wordSize, bool hasSynonymGid)
        {
            this.bytes = bytes;
            this.offset = offset;
            this.wordSize = wordSize;
            this.hasSynonymGid = hasSynonymGid;
        }

        public WordInfo GetWordInfo(int wordId)
        {
            ByteBuffer buf = bytes.AsReadOnlyBuffer();
            buf.Order(bytes.Order());
            buf.Position(WordIdToOffset(wordId));

            string surface = BufferToString(buf);
            short headwordLength = (short)BufferToStringLength(buf);
            short posId = buf.GetShort();
            string normalizedForm = BufferToString(buf);
            if (string.IsNullOrEmpty(normalizedForm))
            {
                normalizedForm = surface;
            }
            int dictionaryFormWordId = buf.GetInt();
            string readingForm = BufferToString(buf);
            if (string.IsNullOrEmpty(readingForm))
            {
                readingForm = surface;
            }
            int[] aUnitSplit = BufferToIntArray(buf);
            int[] bUnitSplit = BufferToIntArray(buf);
            int[] wordStructure = BufferToIntArray(buf);

            int[] synonymGids = new int[0];
            if (hasSynonymGid)
            {
                synonymGids = BufferToIntArray(buf);
            }

            string dictionaryForm = surface;
            if (dictionaryFormWordId >= 0 && dictionaryFormWordId != wordId)
            {
                WordInfo wi = GetWordInfo(dictionaryFormWordId);
                dictionaryForm = wi.GetSurface();
            }

            return new WordInfo(surface, headwordLength, posId, normalizedForm, dictionaryFormWordId, dictionaryForm,
                    readingForm, aUnitSplit, bUnitSplit, wordStructure, synonymGids);
        }

        public int Size()
        {
            return wordSize;
        }

        private int WordIdToOffset(int wordId)
        {
            return bytes.GetInt(offset + 4 * wordId);
        }

        private int BufferToStringLength(ByteBuffer buffer)
        {
            byte length = buffer.Get();
            if (length < 0)
            {
                int high = length & 0xFF;
                int low = buffer.Get() & 0xFF;
                return ((high & 0x7F) << 8) | low;
            }
            return length;
        }

        private string BufferToString(ByteBuffer buffer)
        {
            int length = BufferToStringLength(buffer);
            byte[] bytes = new byte[length * 2];
            for (int i = 0; i < length; i++)
            {
                buffer.GetBytes(bytes, i * 2, 2);
            }
            return Encoding.Unicode.GetString(bytes);
        }

        private int[] BufferToIntArray(ByteBuffer buffer)
        {
            int length = buffer.Get() & 0xFF;
            int[] array = new int[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = buffer.GetInt();
            }
            return array;
        }
    }
}
