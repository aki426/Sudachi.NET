using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sudachi.Net.Core.Dictionary.Lexicon
{
    //public class DoubleArrayLexicon : ILexicon
    //{
    //    private const int UserDictCostParMorph = -20;

    //    private readonly WordIdTable _wordIdTable;
    //    private readonly WordParameterList _wordParams;
    //    private readonly WordInfoList _wordInfos;
    //    private readonly DoubleArray _trie;

    //    public DoubleArrayLexicon(byte[] bytes, int offset, bool hasSynonymGid)
    //    {
    //        using var ms = new MemoryStream(bytes, offset, bytes.Length - offset);
    //        using var br = new BinaryReader(ms);

    //        _trie = new DoubleArray();
    //        int size = br.ReadInt32();
    //        _trie.SetArray(br.ReadArray<int>(size), size);

    //        _wordIdTable = new WordIdTable(br);
    //        _wordParams = new WordParameterList(br);
    //        _wordInfos = new WordInfoList(br, _wordParams.Size, hasSynonymGid);
    //    }

    //    public IEnumerator<int[]> Lookup(byte[] text, int offset)
    //    {
    //        var iterator = _trie.CommonPrefixSearch(text, offset);
    //        return iterator.MoveNext() ? new DoubleArrayLexiconEnumerator(iterator, _wordIdTable) : iterator;
    //    }

    //    public int GetWordId(string headword, short posId, string readingForm)
    //    {
    //        for (var wid = 0; wid < _wordInfos.Size; wid++)
    //        {
    //            var info = _wordInfos.GetWordInfo(wid);
    //            if (info.Surface == headword && info.PosId == posId && info.ReadingForm == readingForm)
    //                return wid;
    //        }

    //        return -1;
    //    }

    //    public short LeftId(int wordId) => _wordParams.LeftId(wordId);

    //    public short RightId(int wordId) => _wordParams.RightId(wordId);

    //    public short Cost(int wordId) => _wordParams.Cost(wordId);

    //    public WordInfo WordInfo(int wordId) => _wordInfos.GetWordInfo(wordId);

    //    public int Size => _wordParams.Size;

    //    public void CalculateCost(Tokenizer tokenizer)
    //    {
    //        for (var wordId = 0; wordId < _wordParams.Size; wordId++)
    //        {
    //            if (Cost(wordId) != short.MinValue)
    //                continue;

    //            var surface = WordInfo(wordId).Surface;
    //            var ms = tokenizer.Tokenize(surface);
    //            var cost = ms.InternalCost + UserDictCostParMorph * ms.Length;

    //            if (cost > short.MaxValue)
    //                cost = short.MaxValue;
    //            else if (cost < short.MinValue)
    //                cost = short.MinValue;

    //            _wordParams.SetCost(wordId, (short)cost);
    //        }
    //    }

    //    public void SetDictionaryId(int id) => _wordIdTable.SetDictionaryId(id);

    //    private class DoubleArrayLexiconEnumerator : IEnumerator<int[]>
    //    {
    //        private readonly IEnumerator<int[]> _iterator;
    //        private readonly WordIdTable _wordIdTable;
    //        private int?[] _wordIds;
    //        private int _length;
    //        private int _index;

    //        public DoubleArrayLexiconEnumerator(IEnumerator<int[]> iterator, WordIdTable wordIdTable)
    //        {
    //            _iterator = iterator;
    //            _wordIdTable = wordIdTable;
    //            _index = -1;
    //        }

    //        public bool MoveNext()
    //        {
    //            if (_index < 0)
    //                return _iterator.MoveNext();

    //            if (_index < _wordIds.Length)
    //                return true;

    //            return _iterator.MoveNext();
    //        }

    //        public int[] Current
    //        {
    //            get
    //            {
    //                if (_index < 0 || _index >= _wordIds.Length)
    //                {
    //                    var p = _iterator.Current;
    //                    _wordIds = _wordIdTable.Get(p[0]);
    //                    _length = p[1];
    //                    _index = 0;
    //                }

    //                return new[] { _wordIds[_index++].Value, _length };
    //            }
    //        }

    //        object IEnumerator.Current => Current;

    //        public void Reset() => throw new System.NotSupportedException();

    //        public void Dispose() { }
    //    }
    //}
}
