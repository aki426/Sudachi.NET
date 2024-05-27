using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudachi.Net.Core.Dictionary.Word
{
    /**
     * This class is an abstraction for looking up words in a list of binary
     * dictionaries. It returns a list of WordIds for each matching key in the Trie
     * index. WordIds are stored in a plain int array to remove any possible boxing.
     * Memory for the lookup is kept for a single analysis step to decrease garbage
     * collection pressure.
     */

    public sealed class WordLookup
    {
        private readonly DoubleArrayLookup lookup = new DoubleArrayLookup();
        private WordIdTable words;

        // initial size 16 - one cache line (64 bytes) on most modern CPUs
        private int[] wordIds = new int[16];

        private int numWords;
        private readonly List<DoubleArrayLexicon> lexicons;
        private int currentLexicon = -1;

        public WordLookup(List<DoubleArrayLexicon> lexicons)
        {
            this.lexicons = lexicons;
        }

        private void Rebind(DoubleArrayLexicon lexicon)
        {
            lookup.SetArray(lexicon.GetTrieArray());
            words = lexicon.GetWordIdTable();
        }

        /**
         * Start the search for new key
         *
         * @param key
         *            utf-8 bytes corresponding to the trie key
         * @param offset
         *            offset of key start
         * @param limit
         *            offset of key end
         */

        public void Reset(byte[] key, int offset, int limit)
        {
            currentLexicon = lexicons.Count - 1;
            Rebind(lexicons[currentLexicon]);
            lookup.Reset(key, offset, limit);
        }

        /**
         * This is not public API. Returns the array for wordIds with the length at
         * least equal to the passed parameter
         *
         * @param length
         *            minimum requested length
         * @return WordId array
         */

        public int[] OutputBuffer(int length)
        {
            if (wordIds.Length < length)
            {
                wordIds = wordIds.Concat(new int[Math.Max(length, wordIds.Length * 2) - wordIds.Length]).ToArray();
            }
            return wordIds;
        }

        /**
         * Sets the wordIds, numWords, endOffset to the
         *
         * @return true if there was an entry in any of binary dictionaries
         */

        public bool Next()
        {
            while (!lookup.Next())
            {
                int nextLexicon = currentLexicon - 1;
                if (nextLexicon < 0)
                {
                    return false;
                }
                Rebind(lexicons[nextLexicon]);
                currentLexicon = nextLexicon;
            }
            int wordGroupId = lookup.GetValue();
            numWords = words.ReadWordIds(wordGroupId, this);
            return true;
        }

        /**
         * Returns trie key end offset
         *
         * @return number of utf-8 bytes corresponding to the end of key
         */

        public int GetEndOffset()
        {
            return lookup.GetOffset();
        }

        /**
         *
         * @return number of currently correct entries in the wordIds
         */

        public int GetNumWords()
        {
            return numWords;
        }

        /**
         * Returns array of word ids. Number of correct entries is specified by
         * {@link #getNumWords()}. WordIds have their dictionary part set.
         *
         * @return array consisting word ids for the current index entry
         * @see WordId
         */

        public int[] GetWordsIds()
        {
            return wordIds;
        }
    }
}
