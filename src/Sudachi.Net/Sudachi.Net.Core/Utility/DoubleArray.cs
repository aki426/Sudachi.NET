using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Linq;
using System.Text;

namespace Sudachi.Net.Core.Utility
{
    public class DoubleArray
    {
        //private Memory<int> _array;
        //private MemoryMappedViewAccessor _buffer;
        //private int _size;

        //public void SetArray(IReadOnlyList<int> array, int size)
        //{
        //    _array = array.ToArray().AsMemory();
        //    _size = size;
        //}

        //public Memory<int> Array => _array;
        //public MemoryMappedViewAccessor Buffer => _buffer;

        //public void Clear()
        //{
        //    _buffer?.Dispose();
        //    _buffer = null;
        //    _size = 0;
        //}

        //public int Size => _size;

        //public int TotalSize => 4 * _size;

        //public void Build(byte[][] keys, int[] values, Action<int, int> progressFunction)
        //{
        //    var keySet = new KeySet(keys, values);
        //    var builder = new DoubleArrayBuilder(progressFunction);
        //    builder.Build(keySet);
        //    _buffer = builder.Copy();
        //    _array = new MemoryMappedViewAccessor(_buffer).AsMemory().Span.AsMemory();
        //    _size = _array.Length;
        //}

        //public void Open(FileStream inputFile, long position = 0, long? totalSize = null)
        //{
        //    totalSize ??= inputFile.Length;
        //    _buffer = MemoryMappedFile.CreateViewAccessor(inputFile.SafeMemoryMappedFileHandle, position, totalSize.Value, MemoryMappedFileAccess.Read);
        //    _array = _buffer.AsMemory().Span.AsMemory();
        //    _size = _array.Length;
        //}

        //public void Save(FileStream outputFile)
        //{
        //    outputFile.Write(_buffer.SafeMemoryMappedViewHandle);
        //}

        //public int[] ExactMatchSearch(byte[] key)
        //{
        //    var result = new int[] { -1, 0 };
        //    var nodePos = 0;
        //    var unit = _array.Span[nodePos];
        //    nodePos ^= Offset(unit);

        //    foreach (var k in key)
        //    {
        //        nodePos ^= Byte.ToUInt32(k);
        //        unit = _array.Span[nodePos];
        //        if (Label(unit) != Byte.ToUInt32(k))
        //            return result;
        //    }

        //    if (!HasLeaf(unit))
        //        return result;

        //    unit = _array.Span[nodePos ^ Offset(unit)];
        //    result[0] = Value(unit);
        //    result[1] = key.Length;
        //    return result;
        //}

        //public List<int[]> CommonPrefixSearch(byte[] key, int offset, int maxNumResult)
        //{
        //    var result = new List<int[]>();
        //    var nodePos = 0;
        //    var unit = _array.Span[nodePos];
        //    nodePos ^= Offset(unit);

        //    for (var i = offset; i < key.Length; i++)
        //    {
        //        var k = key[i];
        //        nodePos ^= Byte.ToUInt32(k);
        //        unit = _array.Span[nodePos];
        //        if (Label(unit) != Byte.ToUInt32(k))
        //            return result;

        //        nodePos ^= Offset(unit);
        //        if (HasLeaf(unit) && result.Count < maxNumResult)
        //            result.Add(new[] { Value(_array.Span[nodePos]), i + 1 });
        //    }

        //    return result;
        //}

        //public IEnumerator<int[]> CommonPrefixSearch(byte[] key, int offset)
        //{
        //    return new DoubleArrayEnumerator(key, offset, this);
        //}

        //public TraverseResult Traverse(byte[] key, int offset, int length, int nodePosition)
        //{
        //    var nodePos = nodePosition;
        //    var id = nodePosition;
        //    var unit = _array.Span[nodePosition];

        //    for (var i = offset; i < length; i++)
        //    {
        //        var k = key[i];
        //        id ^= Offset(unit) ^ Byte.ToUInt32(k);
        //        unit = _array.Span[id];
        //        if (Label(unit) != Byte.ToUInt32(k))
        //            return new TraverseResult(-2, i, nodePos);

        //        nodePos = id;
        //    }

        //    return !HasLeaf(unit)
        //        ? new TraverseResult(-1, length, nodePos)
        //        : new TraverseResult(Value(_array.Span[nodePos ^ Offset(unit)]), length, nodePos);
        //}

        //public TraverseResult Traverse(byte[] key, int offset, int nodePosition)
        //{
        //    return Traverse(key, offset, key.Length, nodePosition);
        //}

        //private static bool HasLeaf(int unit) => (unit >> 8 & 1) == 1;
        //private static int Value(int unit) => unit & int.MaxValue;
        //private static int Label(int unit) => unit & -2147483393;
        //private static int Offset(int unit) => unit >> 10 << ((unit & 512) >> 6);

        //private class DoubleArrayEnumerator : IEnumerator<int[]>
        //{
        //    private readonly byte[] _key;
        //    private int _offset;
        //    private int _nodePos;
        //    private int?[] _next;
        //    private readonly DoubleArray _doubleArray;

        //    public DoubleArrayEnumerator(byte[] key, int offset, DoubleArray doubleArray)
        //    {
        //        _key = key;
        //        _offset = offset;
        //        _doubleArray = doubleArray;
        //        _nodePos = 0;
        //        var unit = doubleArray._array.Span[_nodePos];
        //        _nodePos ^= Offset(unit);
        //        _next = null;
        //    }

        //    public bool MoveNext()
        //    {
        //        if (_next is null)
        //            _next = GetNext();

        //        return _next is not null;
        //    }

        //    public int[] Current => _next ?? throw new InvalidOperationException();

        //    object IEnumerator.Current => Current;

        //    public void Reset() => throw new NotSupportedException();

        //    public void Dispose() { }

        //    private int?[] GetNext()
        //    {
        //        while (_offset < _key.Length)
        //        {
        //            var k = _key[_offset];
        //            _nodePos ^= Byte.ToUInt32(k);
        //            var unit = _doubleArray._array.Span[_nodePos];
        //            if (Label(unit) != Byte.ToUInt32(k))
        //            {
        //                _offset = _key.Length;
        //                return null;
        //            }

        //            _nodePos ^= Offset(unit);
        //            if (HasLeaf(unit))
        //            {
        //                var r = new[] { Value(_doubleArray._array.Span[_nodePos]), ++_offset };
        //                return r;
        //            }

        //            _offset++;
        //        }

        //        return null;
        //    }
        //}

        public record TraverseResult(int Result, int Offset, int NodePosition);
    }
}