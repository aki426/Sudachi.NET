using System;
using System.Collections.Generic;
using System.Text;
using Sudachi.Net.Core.Utility;

namespace Sudachi.Net.Core.Dictionary.Build
{
    public static class WordLookup
    {
        public class Noop : IWordIdResolver
        {
            public int Lookup(string headword, short posId, string reading)
            {
                return -1;
            }

            public void Validate(int wordId)
            {
                // noop validator always works
            }

            public bool IsUser()
            {
                return false;
            }
        }

        public class Csv : IWordIdResolver
        {
            private readonly CsvLexicon _lexicon;

            public Csv(CsvLexicon lexicon)
            {
                _lexicon = lexicon;
            }

            public int Lookup(string headword, short posId, string reading)
            {
                IReadOnlyList<CsvLexicon.WordEntry> entries = _lexicon.Entries;
                for (int i = 0; i < entries.Count; ++i)
                {
                    CsvLexicon.WordEntry entry = entries[i];
                    if (entry.WordInfo.Surface == headword && entry.WordInfo.POSId == posId
                            && entry.WordInfo.ReadingForm == reading)
                    {
                        return i;
                    }
                }
                return -1;
            }

            public void Validate(int wordId)
            {
                if (wordId < 0)
                {
                    throw new ArgumentException("wordId can't be negative, was " + wordId);
                }
                IReadOnlyList<CsvLexicon.WordEntry> entries = _lexicon.Entries;
                if (wordId >= entries.Count)
                {
                    throw new ArgumentException(
                            $"wordId {wordId} was larger than number of dictionary entries ({entries.Count})");
                }
            }

            public bool IsUser()
            {
                return false;
            }
        }

        public class Prebuilt : IWordIdResolver
        {
            private readonly Lexicon _lexicon;
            private readonly int _prebuiltSize;

            public Prebuilt(Lexicon lexicon)
            {
                _lexicon = lexicon;
                _prebuiltSize = lexicon.Size;
            }

            public int Lookup(string headword, short posId, string reading)
            {
                return _lexicon.GetWordId(headword, posId, reading);
            }

            public void Validate(int wordId)
            {
                int word = WordId.GetWord(wordId);
                if (word > _prebuiltSize)
                {
                    throw new ArgumentException("WordId was larger than the number of dictionary entries");
                }
            }

            public bool IsUser()
            {
                return false;
            }
        }

        public class Chain : IWordIdResolver
        {
            private readonly IWordIdResolver _system;
            private readonly IWordIdResolver _user;

            public Chain(IWordIdResolver system, IWordIdResolver user)
            {
                _system = system;
                _user = user;
            }

            public int Lookup(string headword, short posId, string reading)
            {
                int wid = _user.Lookup(headword, posId, reading);
                if (wid == -1)
                {
                    return _system.Lookup(headword, posId, reading);
                }
                return WordId.Make(1, wid);
            }

            public void Validate(int wordId)
            {
                int dic = WordId.GetDic(wordId);
                if (dic == 0)
                {
                    _system.Validate(wordId);
                }
                else if (dic == 1)
                {
                    _user.Validate(WordId.GetWord(wordId));
                }
                else
                {
                    throw new ArgumentException("dictionary id can be only 0 or 1 at the build time");
                }
            }

            public bool IsUser()
            {
                return true;
            }
        }
    }
}
