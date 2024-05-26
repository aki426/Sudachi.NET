using System;
using System.Collections.Generic;
using System.Text;

namespace Sudachi.Net.Core.Dictionary.Build
{
    public class POSTable : IWriteDictionary
    {
        private readonly List<POS> _table = new List<POS>();
        private readonly Dictionary<POS, short> _lookup = new Dictionary<POS, short>();
        private int _builtin = 0;

        internal short GetId(POS pos)
        {
            if (!_lookup.TryGetValue(pos, out short id))
            {
                int next = _table.Count;
                if (next >= short.MaxValue)
                {
                    throw new ArgumentException("maximum POS number exceeded by " + pos);
                }
                _table.Add(pos);
                id = (short)next;
                _lookup[pos] = id;
            }
            return id;
        }

        public void PreloadFrom(Grammar grammar)
        {
            int partOfSpeechSize = grammar.PartOfSpeechSize;
            for (short i = 0; i < partOfSpeechSize; ++i)
            {
                POS pos = grammar.GetPartOfSpeechString(i);
                _table.Add(pos);
                _lookup[pos] = i;
            }
            _builtin += partOfSpeechSize;
        }

        internal IReadOnlyList<POS> GetList()
        {
            return _table;
        }

        public void WriteTo(ModelOutput output)
        {
            output.WithPart("POS table", () =>
            {
                DicBuffer buffer = new DicBuffer(128 * 1024);
                buffer.PutShort((short)OwnedLength());
                for (int i = _builtin; i < _table.Count; ++i)
                {
                    foreach (string s in _table[i])
                    {
                        if (!buffer.Put(s))
                        {
                            // handle buffer overflow, this should be extremely rare
                            buffer.Consume(size => output.Write(buffer.Buffer, 0, size));
                            buffer.Put(s);
                        }
                    }
                }
                buffer.Consume(size => output.Write(buffer.Buffer, 0, size));
            });
        }

        public int OwnedLength()
        {
            return _table.Count - _builtin;
        }
    }
}
