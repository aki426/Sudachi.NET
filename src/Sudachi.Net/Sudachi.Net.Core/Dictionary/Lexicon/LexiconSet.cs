using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudachi.Net.Core.Dictionary.Lexicon
{
    public class LexiconSet // : ILexicon
    {
        //private const int MaxDictionaries = 15;

        //private readonly List<DoubleArrayLexicon> _lexicons = new();
        //private readonly short _systemPartOfSpeechSize;
        //private readonly List<short> _posOffsets = new();

        //public LexiconSet(ILexicon systemLexicon, short systemPartOfSpeechSize)
        //{
        //    _systemPartOfSpeechSize = systemPartOfSpeechSize;
        //    Add(systemLexicon, 0);
        //}

        //public void Add(ILexicon lexicon, short posOffset)
        //{
        //    if (lexicon is not DoubleArrayLexicon daLexicon)
        //        throw new ArgumentException("Lexicon must be a DoubleArrayLexicon", nameof(lexicon));

        //    daLexicon.SetDictionaryId((short)_lexicons.Count);
        //    _lexicons.Add(daLexicon);
        //    _posOffsets.Add(posOffset);
        //}

        //public bool IsFull => _lexicons.Count >= MaxDictionaries;

        //public IEnumerator<int[]> Lookup(byte[] text, int offset)
        //{
        //    if (_lexicons.Count == 0)
        //        return Enumerable.Empty<int[]>().GetEnumerator();

        //    if (_lexicons.Count == 1)
        //        return _lexicons[0].Lookup(text, offset);

        //    return new LexiconSetEnumerator(text, offset, _lexicons.Count - 1, _lexicons).GetEnumerator();
        //}

        //public int GetWordId(string headword, short posId, string readingForm)
        //{
        //    for (var dictId = 1; dictId < _lexicons.Count; dictId++)
        //    {
        //        var wid = _lexicons[dictId].GetWordId(headword, posId, readingForm);
        //        if (wid >= 0)
        //            return BuildWordId((short)dictId, wid);
        //    }

        //    return _lexicons[0].GetWordId(headword, posId, readingForm);
        //}

        //public short LeftId(int wordId) => _lexicons[WordId.Dic(wordId)].LeftId(GetWordId(wordId));

        //public short RightId(int wordId) => _lexicons[WordId.Dic(wordId)].RightId(GetWordId(wordId));

        //public short Cost(int wordId) => _lexicons[WordId.Dic(wordId)].Cost(GetWordId(wordId));

        //public WordInfo WordInfo(int wordId)
        //{
        //    var dictionaryId = WordId.Dic(wordId);
        //    var internalId = WordId.Word(wordId);
        //    var wordInfo = _lexicons[dictionaryId].WordInfo(internalId);
        //    var posId = wordInfo.PosId;

        //    if (dictionaryId > 0 && posId >= _systemPartOfSpeechSize) // user defined part-of-speech
        //        wordInfo = wordInfo with { PosId = (short)(wordInfo.PosId - _systemPartOfSpeechSize + _posOffsets[dictionaryId]) };

        //    ConvertSplit(wordInfo.AUnitSplit, dictionaryId);
        //    ConvertSplit(wordInfo.BUnitSplit, dictionaryId);
        //    ConvertSplit(wordInfo.WordStructure, dictionaryId);

        //    return wordInfo;
        //}

        //public int Size => _lexicons.Sum(lexicon => lexicon.Size);

        //private static int GetWordId(int wordId) => WordId.Word(wordId);

        //private int BuildWordId(short dictId, int wordId)
        //{
        //    if (dictId >= _lexicons.Count)
        //        throw new IndexOutOfRangeException($"dictionaryId is too large: {dictId}");

        //    return WordId.Make(dictId, wordId);
        //}

        //private void ConvertSplit(int[] split, int dictionaryId)
        //{
        //    for (var i = 0; i < split.Length; i++)
        //    {
        //        if (WordId.Dic(split[i]) > 0)
        //            split[i] = BuildWordId((short)dictionaryId, GetWordId(split[i]));
        //    }
        //}

        ////public WordLookup MakeLookup() => new(_lexicons);

        //public bool IsValid => _lexicons is not null;

        ////public void Invalidate() => _lexicons = null;

        //private class LexiconSetEnumerator : IEnumerator<int[]>
        //{
        //    private readonly byte[] _text;
        //    private readonly int _offset;
        //    private int _dictId;
        //    private IEnumerator<int[]> _iterator;
        //    private readonly List<DoubleArrayLexicon> _lexicons;

        //    public LexiconSetEnumerator(byte[] text, int offset, int start, List<DoubleArrayLexicon> lexicons)
        //    {
        //        _text = text;
        //        _offset = offset;
        //        _dictId = start;
        //        _lexicons = lexicons;
        //        _iterator = _lexicons[_dictId].Lookup(text, offset);
        //    }

        //    public bool MoveNext()
        //    {
        //        while (!_iterator.MoveNext())
        //        {
        //            var nextId = _dictId - 1;
        //            if (nextId < 0)
        //                return false;

        //            _iterator = _lexicons[nextId].Lookup(_text, _offset);
        //            _dictId = nextId;
        //        }

        //        return true;
        //    }

        //    public void Reset() => throw new NotSupportedException();

        //    public int[] Current
        //    {
        //        get
        //        {
        //            if (_iterator.Current is not { } r)
        //                throw new InvalidOperationException();

        //            r[0] = BuildWordId((short)_dictId, r[0]);
        //            return r;
        //        }
        //    }

        //    object? IEnumerator.Current => Current;

        //    public void Dispose()
        //    { }

        //    private int BuildWordId(short dictId, int wordId) => WordId.Make(dictId, wordId);
        //}
    }
}