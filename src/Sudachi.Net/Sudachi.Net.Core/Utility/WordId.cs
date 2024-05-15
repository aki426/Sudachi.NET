using System;

namespace Sudachi.Net.Core.Utility
{
    /// <summary>
    /// The word id.
    /// WordId is a 32-bit integer that consists of a dictionary id and an internal word id.
    /// Dictionary Id is a 4-bit. 0 is system, 1 and above are user.
    /// Word Id is a 28-bit.
    /// </summary>
    public static class WordId
    {
        /// <summary>Internal word ids can't be larger than this number.</summary>
        public static readonly int MAX_WORD_ID = 0x0fffffff;

        // NOTE: 0xe = 0b1110なので、符号付きIntで負のフラグが立たないようにしている？

        /// <summary>Dictionary ids can't be larger than this number.</summary>
        public static readonly int MAX_DIC_ID = 0xe;

        /// <summary>
        /// Makes the unchecked.
        /// </summary>
        /// <param name="dic">The dic.</param>
        /// <param name="word">The word.</param>
        /// <returns>An int.</returns>
        private static int MakeUnchecked(int dic, int word) => DicIdMask(dic) | word;

        /// <summary>
        /// Make combined WordId from dictionary and internal parts.
        /// This method does bound checking.
        /// </summary>
        /// <param name="dic">Dictionary id. 0 is system, 1 and above are user.</param>
        /// <param name="word">Word id inside the dictionary.</param>
        /// <returns>Combined word id.</returns>
        public static int Make(int dic, int word)
        {
            if (MAX_WORD_ID < word)
            {
                throw new IndexOutOfRangeException($"wordId is too large: {word}");
            }
            if (word < 0)
            {
                throw new IndexOutOfRangeException($"wordId is minus: {word}");
            }

            if (MAX_DIC_ID < dic)
            {
                throw new IndexOutOfRangeException($"dictionaryId is too large: {dic}");
            }
            if (dic < 0)
            {
                throw new IndexOutOfRangeException($"dictionaryId is minus: {dic}");
            }

            return MakeUnchecked(dic, word);
        }

        // TODO: intを右ビットシフトすると、符号ビットがコピーされるので、期待した値にならない可能性がある。

        /// <summary>
        /// Extract dictionary number from the combined word id.
        /// </summary>
        /// <param name="wordId">Combined word id.</param>
        /// <returns>Dictionary number.</returns>
        public static int Dic(int wordId) => (int)((uint)wordId >> 28);

        /// <summary>
        /// Extract internal word id from the combined word id.
        /// </summary>
        /// <param name="wordId">Combined word id.</param>
        /// <returns>Internal word id.</returns>
        public static int Word(int wordId) => wordId & MAX_WORD_ID;

        public static int DicIdMask(int dicId) => dicId << 28;

        public static int ApplyMask(int wordId, int dicIdMask) => wordId & MAX_WORD_ID | dicIdMask;
    }
}