using System;

namespace Sudachi.Net.Core.Utility
{
    //   Copyright (c) 2024 KANEDA Akihiro <taoist.aki@gmail.com>
    //
    //   Licensed under the Apache License, Version 2.0 (the "License");
    //   you may not use this file except in compliance with the License.
    //   You may obtain a copy of the License at
    //
    //       http://www.apache.org/licenses/LICENSE-2.0
    //
    //   Unless required by applicable law or agreed to in writing, software
    //   distributed under the License is distributed on an "AS IS" BASIS,
    //   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    //   See the License for the specific language governing permissions and
    //   limitations under the License.

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
