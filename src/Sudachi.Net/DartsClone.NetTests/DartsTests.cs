using DartsClone.NetTests;
using FluentAssertions;
using Xunit;

namespace DartsClone.Net.Tests
{
    public class DartsTests
    {
        private const int NumValidKeys = 1 << 16;
        private const int NumInvalidKeys = 1 << 17;
        private const int MaxNumResult = 16;

        private readonly Random _random = new Random();
        private byte[][] _keys;
        private byte[][] _invalidKeys;
        private int[] _values;

        private readonly DartsTestsUtility.TemporaryFolder _temporaryFolder = new DartsTestsUtility.TemporaryFolder();

        public DartsTests()
        {
            var validKeys = DartsTestsUtility.GenerateValidKeys(NumValidKeys);
            var invalidKeys = DartsTestsUtility.GenerateInvalidKeys(NumInvalidKeys, validKeys);

            _keys = validKeys.ToArray();
            _invalidKeys = invalidKeys.ToArray();
            _values = new int[_keys.Length];
            int keyId = 0;
            for (int i = 0; i < _keys.Length; i++)
            {
                _values[i] = keyId++;
            }
        }

        public void Dispose()
        {
            _temporaryFolder.Dispose();
        }

        private void TestDic(DoubleArray dic)
        {
            for (int i = 0; i < _keys.Length; i++)
            {
                var result = dic.ExactMatchSearch(_keys[i]);
                result[1].Should().Be(_keys[i].Length);
                result[0].Should().Be(_values[i]);
            }

            foreach (var key in _invalidKeys)
            {
                var result = dic.ExactMatchSearch(key);
                result[0].Should().Be(-1);
            }
        }

        [Fact]
        public void BuildWithoutValue()
        {
            var dic = new DoubleArray();
            dic.Build(_keys, null, null);
            TestDic(dic);
        }
    }

    //    [Fact]
    //    public void Build()
    //    {
    //        var dic = new DoubleArray();
    //        dic.Build(_keys, _values, null);
    //        TestDic(dic);
    //    }

    //    [Fact]
    //    public void Size()
    //    {
    //        var dic = new DoubleArray();
    //        dic.Build(_keys, _values, null);
    //        dic.Size().Should().Be(dic.TotalSize() / 4);
    //    }

    //    [Fact]
    //    public void BuildWithRandomValue()
    //    {
    //        for (int i = 0; i < _values.Length; i++)
    //        {
    //            _values[i] = _random.Next(10);
    //        }
    //        var dic = new DoubleArray();
    //        dic.Build(_keys, _values, null);
    //        TestDic(dic);
    //    }

    //    [Fact]
    //    public void SaveAndOpen()
    //    {
    //        var dic = new DoubleArray();
    //        dic.Build(_keys, _values, null);

    //        var dicFile = _temporaryFolder.NewFile();
    //        using (var ostream = new FileStream(dicFile, FileMode.Create, FileAccess.Write))
    //        using (var outputFile = ostream.Channel)
    //        {
    //            dic.Save(outputFile);
    //        }

    //        var dicCopy = new DoubleArray();
    //        using (var istream = new FileStream(dicFile, FileMode.Open, FileAccess.Read))
    //        using (var inputFile = istream.Channel)
    //        {
    //            dicCopy.Open(inputFile, 0, -1);
    //        }
    //        TestDic(dicCopy);
    //    }

    //    [Fact]
    //    public void Array()
    //    {
    //        var dic = new DoubleArray();
    //        dic.Build(_keys, _values, null);
    //        var array = dic.Array();
    //        int size = dic.Size();

    //        var dicCopy = new DoubleArray();
    //        dicCopy.SetArray(array, size);
    //        TestDic(dicCopy);
    //    }

    //    [Fact]
    //    public void CommonPrefixSearch()
    //    {
    //        var dic = new DoubleArray();
    //        dic.Build(_keys, _values, null);

    //        for (int i = 0; i < _keys.Length; i++)
    //        {
    //            var results = dic.CommonPrefixSearch(_keys[i], 0, MaxNumResult);

    //            results.Count.Should().BeGreaterOrEqualTo(1);
    //            results.Count.Should().BeLessThan(MaxNumResult);

    //            results.Last()[0].Should().Be(_values[i]);
    //            results.Last()[1].Should().Be(_keys[i].Length);
    //        }

    //        foreach (var key in _invalidKeys)
    //        {
    //            var results = dic.CommonPrefixSearch(key, 0, MaxNumResult);
    //            results.Count.Should().BeLessThan(MaxNumResult);

    //            if (results.Any())
    //            {
    //                results.Last()[0].Should().NotBe(-1);
    //                key.Length.Should().BeGreaterThan(results.Last()[1]);
    //            }
    //        }
    //    }

    //    [Fact]
    //    public void CommonPrefixSearchWithIterator()
    //    {
    //        var dic = new DoubleArray();
    //        dic.Build(_keys, _values, null);

    //        for (int i = 0; i < _keys.Length; i++)
    //        {
    //            var iterator = dic.CommonPrefixSearch(_keys[i], 0);

    //            iterator.Any().Should().BeTrue();

    //            int[] result = null;
    //            while (iterator.MoveNext())
    //            {
    //                result = iterator.Current;
    //            }
    //            iterator.MoveNext().Should().BeFalse();
    //            result[0].Should().Be(_values[i]);
    //            result[1].Should().Be(_keys[i].Length);
    //        }

    //        foreach (var key in _invalidKeys)
    //        {
    //            var iterator = dic.CommonPrefixSearch(key, 0);

    //            int[] result = null;
    //            while (iterator.MoveNext())
    //            {
    //                result = iterator.Current;
    //            }
    //            if (result != null)
    //            {
    //                result[0].Should().NotBe(-1);
    //                key.Length.Should().BeGreaterThan(result[1]);
    //            }
    //        }
    //    }

    //}
}
