using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sudachi.Net.Core.Dictionary.Word;

namespace Sudachi.Net.Core.Dictionary.Build
{
    public class CsvLexicon : IWriteDictionary
    {
        // NOTE: JAVA版ではByteは符号付きでMAX_VALUEは127だが、C#版では符号なしのため255になってしまう。

        private static readonly int ARRAY_MAX_LENGTH = byte.MaxValue;

        private static readonly int MIN_REQUIRED_NUMBER_OF_COLUMNS = 18;

        private static readonly Regex unicodeLiteral = new Regex(@"\\u([0-9a-fA-F]{4}|\{[0-9a-fA-F]+})");

        private static readonly Regex PATTERN_ID = new Regex(@"U?\d+");

        private readonly Parameters _parameters = new Parameters();

        // TODO: get; init; に変更
        private readonly POSTable _posTable;

        private readonly List<WordEntry> _entries = new List<WordEntry>();

        private IWordIdResolver _widResolver = new WordLookup.Noop();

        public CsvLexicon(POSTable pos)
        {
            _posTable = pos;
        }

        // TODO: setterまたはget; init;に変更

        public void SetResolver(IWordIdResolver widResolver)
        {
            _widResolver = widResolver;
        }

        /// <summary>
        /// Resolve unicode escape sequences in the string
        /// <p>
        /// Sequences are defined to be \\u0000-\\uFFFF: exactly four hexadecimal
        /// characters preceded by \\u \\u{...}: a correct unicode character inside
        /// brackets
        /// </summary>
        /// <param name="text">text to resolve sequences</param>
        /// <returns>string with unicode escapes resolved</returns>
        public static string Unescape(string text)
        {
            Match m = unicodeLiteral.Match(text);
            if (!m.Success)
            {
                return text;
            }

            StringBuilder sb = new StringBuilder();
            while (m.Success)
            {
                string u = m.Groups[1].Value;
                if (u.StartsWith("{"))
                {
                    u = u.Substring(1, u.Length - 2);
                }
                sb.Append(char.ConvertFromUtf32(int.Parse(u, System.Globalization.NumberStyles.HexNumber)));
                m = m.NextMatch();
            }
            sb.Append(text, m.Index + m.Length, text.Length - (m.Index + m.Length));
            return sb.ToString();
        }

        public IReadOnlyList<WordEntry> Entries => _entries;

        internal WordEntry ParseLine(IReadOnlyList<string> cols)
        {
            if (cols.Count < MIN_REQUIRED_NUMBER_OF_COLUMNS)
            {
                throw new ArgumentException("invalid format");
            }
            for (int i = 0; i < 15; i++)
            {
                cols = cols.Select((s, idx) => idx < 15 ? Unescape(s) : s).ToList();
            }

            if (Encoding.UTF8.GetByteCount(cols[0]) > DicBuffer.MaxString
                    || !DicBuffer.IsValidLength(cols[4]) || !DicBuffer.IsValidLength(cols[11])
                    || !DicBuffer.IsValidLength(cols[12]))
            {
                throw new ArgumentException("string is too long");
            }

            if (string.IsNullOrEmpty(cols[0]))
            {
                throw new ArgumentException("headword is empty");
            }

            WordEntry entry = new WordEntry();

            // headword for trie
            if (cols[1] != "-1")
            {
                entry.Headword = cols[0];
            }

            // left-id, right-id, cost
            _parameters.Add(short.Parse(cols[1]), short.Parse(cols[2]), short.Parse(cols[3]));

            // part of speech
            POS pos = new POS(cols[5], cols[6], cols[7], cols[8], cols[9], cols[10]);
            short posId = _posTable.GetId(pos);

            entry.AUnitSplitString = cols[15];
            entry.BUnitSplitString = cols[16];
            entry.WordStructureString = cols[17];
            CheckSplitInfoFormat(entry.AUnitSplitString);
            CheckSplitInfoFormat(entry.BUnitSplitString);
            CheckSplitInfoFormat(entry.WordStructureString);
            if (cols[14] == "A" && (entry.AUnitSplitString != "*" || entry.BUnitSplitString != "*"))
            {
                throw new ArgumentException("invalid splitting");
            }

            int[] synonymGids = Array.Empty<int>();
            if (cols.Count > 18)
            {
                synonymGids = ParseSynonymGids(cols[18]);
            }

            entry.WordInfo = new WordInfo(cols[4], // headword
                    (short)Encoding.UTF8.GetByteCount(cols[0]), posId, cols[12], // normalizedForm
                    (cols[13] == "*" ? -1 : int.Parse(cols[13])), // dictionaryFormWordId
                    "", // dummy
                    cols[11], // readingForm
                    null, null, null, synonymGids);

            return entry;
        }

        private int[] ParseSynonymGids(string str)
        {
            if (str == "*")
            {
                return Array.Empty<int>();
            }
            string[] ids = str.Split('/');
            if (ids.Length > ARRAY_MAX_LENGTH)
            {
                throw new ArgumentException("too many units");
            }
            int[] ret = new int[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                ret[i] = int.Parse(ids[i]);
            }
            return ret;
        }

        private int WordToId(string text)
        {
            string[] cols = text.Split(new[] { ',' }, 8);
            if (cols.Length < 8)
            {
                throw new ArgumentException("too few columns");
            }
            string headword = Unescape(cols[0]);
            POS pos = new POS(cols.Skip(1).Take(6).ToArray());
            short posId = _posTable.GetId(pos);
            string reading = Unescape(cols[7]);
            return _widResolver.Lookup(headword, posId, reading);
        }

        private void CheckSplitInfoFormat(string info)
        {
            if (info.Count(c => c == '/') + 1 > ARRAY_MAX_LENGTH)
            {
                throw new ArgumentException("too many units");
            }
        }

        private bool IsId(string text)
        {
            return PATTERN_ID.IsMatch(text);
        }

        private int[] ParseSplitInfo(string info)
        {
            if (info == "*")
            {
                return Array.Empty<int>();
            }
            string[] words = info.Split('/');
            if (words.Length > ARRAY_MAX_LENGTH)
            {
                throw new ArgumentException("too many units");
            }
            int[] ret = new int[words.Length];
            for (int i = 0; i < words.Length; i++)
            {
                string reference = words[i];
                if (IsId(reference))
                {
                    ret[i] = ParseId(reference);
                }
                else
                {
                    ret[i] = WordToId(reference);
                    if (ret[i] < 0)
                    {
                        throw new ArgumentException($"couldn't find {reference} in the dictionaries");
                    }
                }
            }
            return ret;
        }

        private int ParseId(string text)
        {
            int id;
            if (text.StartsWith("U"))
            {
                id = int.Parse(text.Substring(1));
                if (_widResolver.IsUser())
                {
                    id = WordId.Make(1, id);
                }
            }
            else
            {
                id = int.Parse(text);
            }
            _widResolver.Validate(id);
            return id;
        }

        public void WriteTo(ModelOutput output)
        {
            // write number of entries
            byte[] buf = BitConverter.GetBytes(_entries.Count);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buf);
            }
            output.Write(buf, 0, buf.Length);

            _parameters.WriteTo(output);

            int offsetsSize = 4 * _entries.Count;
            DicBuffer offsets = new DicBuffer(offsetsSize);
            long offsetsPosition = output.Position;
            // make a hole for
            output.Position += offsetsSize;

            output.WithSizedPart("word entries", () =>
            {
                DicBuffer buffer = new DicBuffer(128 * 1024);
                int offset = (int)output.Position;
                int numEntries = _entries.Count;
                for (int i = 0; i < numEntries; ++i)
                {
                    WordEntry entry = _entries[i];
                    if (buffer.WontFit(16 * 1024))
                    {
                        // offset += buffer.Consume(size => output.Write(buffer.Buffer, 0, size));
                        offset += buffer.Consume((buffer, offset, length) =>
                        {
                            output.Write(buffer, offset, length);
                            return length;
                        });
                    }
                    offsets.PutInt(offset + buffer.Position);

                    WordInfo wi = entry.WordInfo;
                    buffer.Put(wi.Surface);
                    buffer.PutLength(wi.HeadwordLength);
                    buffer.PutShort(wi.POSId);
                    buffer.PutEmptyIfEqual(wi.NormalizedForm, wi.Surface);
                    buffer.PutInt(wi.DictionaryFormWordId);
                    buffer.PutEmptyIfEqual(wi.ReadingForm, wi.Surface);
                    buffer.PutInts(ParseSplitInfo(entry.AUnitSplitString));
                    buffer.PutInts(ParseSplitInfo(entry.BUnitSplitString));
                    buffer.PutInts(ParseSplitInfo(entry.WordStructureString));
                    buffer.PutInts(wi.SynonymGroupIds);
                    output.ReportProgress(i, numEntries);
                }

                return buffer.Consume((buffer, offset, length) =>
                {
                    output.Write(buffer, offset, length);
                    return length;
                });
            });

            long pos = output.Position;
            output.Position = offsetsPosition;
            output.WithSizedPart("WordInfo offsets", () => offsets.Consume((buffer, offset, length) =>
            {
                output.Write(buffer, offset, length);
                return length;
            }));

            output.Position = pos;
        }

        public int AddEntry(WordEntry e)
        {
            int id = _entries.Count;
            _entries.Add(e);
            return id;
        }

        public void SetLimits(int left, int right)
        {
            _parameters.SetLimits(left, right);
        }

        public class WordEntry
        {
            public string Headword;
            public WordInfo WordInfo;
            public string AUnitSplitString;
            public string BUnitSplitString;
            public string WordStructureString;
        }
    }
}
