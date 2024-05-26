using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DartsClone.Net;

namespace Sudachi.Net.Core.Dictionary.Build
{
    /**
        * Dictionary Parts: Trie index and entry offsets
        */

    public class Index : IWriteDictionary
    {
        private readonly SortedDictionary<byte[], List<int>> _elements =
            new SortedDictionary<byte[], List<int>>(new ByteArrayComparer());

        private int _count = 0;

        public int Add(string key, int wordId)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(key);
            List<int> entries = _elements.GetOrAdd(bytes, k => new List<int>());
            if (entries.Count >= 255)
            {
                throw new ArgumentException($"key {key} has >= 255 entries in the dictionary");
            }
            entries.Add(wordId);
            _count++;
            return bytes.Length;
        }

        public void WriteTo(ModelOutput output)
        {
            DoubleArray trie = new DoubleArray();

            int size = _elements.Count;

            byte[][] keys = new byte[size][];
            int[] values = new int[size];
            byte[] wordIdTable = new byte[_count * (4 + 2)];
            using (MemoryStream ms = new MemoryStream(wordIdTable))
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                output.WithSizedPart("WordId table", () =>
                {
                    int i = 0;
                    int numEntries = _elements.Count;
                    foreach (KeyValuePair<byte[], List<int>> entry in _elements)
                    {
                        keys[i] = entry.Key;
                        values[i] = (int)ms.Position;
                        i++;
                        List<int> wordIds = entry.Value;
                        writer.Write((byte)wordIds.Count);
                        foreach (int wid in wordIds)
                        {
                            writer.Write(wid);
                        }
                        output.ReportProgress(i, numEntries);
                    }
                    return (int)ms.Position + 4;
                });
            }

            DicBuffer buffer = new DicBuffer(4);
            output.WithPart("double array Trie", () =>
            {
                trie.Build(keys, values, output.ReportProgress);
                buffer.PutInt(trie.Size);
                buffer.Consume(size => output.Write(buffer.Buffer, 0, size));
                output.Write(trie.ToBytes(), 0, trie.ToBytes().Length);
            });

            buffer.PutInt(wordIdTable.Length);
            buffer.Consume(size => output.Write(buffer.Buffer, 0, size));

            output.Write(wordIdTable, 0, wordIdTable.Length);
        }

        private class ByteArrayComparer : IComparer<byte[]>
        {
            public int Compare(byte[] x, byte[] y)
            {
                int minLength = Math.Min(x.Length, y.Length);
                for (int i = 0; i < minLength; i++)
                {
                    int result = x[i].CompareTo(y[i]);
                    if (result != 0)
                    {
                        return result;
                    }
                }
                return x.Length.CompareTo(y.Length);
            }
        }
    }

    public static class DictionaryExtensions
    {
        public static TV GetOrAdd<TK, TV>(this IDictionary<TK, TV> dict, TK key, Func<TK, TV> valueFactory)
        {
            if (dict.TryGetValue(key, out TV value))
            {
                return value;
            }
            return dict[key] = valueFactory(key);
        }
    }
}
