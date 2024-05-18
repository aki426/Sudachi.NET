using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Sudachi.Net.Core.Utility
{
    //public class DoubleArray
    //{
    //    public Memory<int> Array { get; }
    //    private Memory<byte> ByteArray { get; }

    //    public int Size => Array.Length;
    //    public int TotalSize => Array.Length * sizeof(int);

    //    public void Build(byte[][] keys, int[] values, Action<int, int> progressFunction)
    //    {
    //        var keySet = new KeySet(keys, values);
    //        var builder = new DoubleArrayBuilder(progressFunction);
    //        builder.Build(keySet);
    //        ByteArray = builder.Copy().Memory;
    //        Array = MemoryMarshal.Cast<byte, int>(ByteArray);
    //    }

    //    public void Open(FileStream inputFile, long position = 0, long totalSize = 0)
    //    {
    //        if (totalSize <= 0)
    //            totalSize = inputFile.Length;

    //        ByteArray = MemoryMarshal.AsMemory(
    //            inputFile.SafeMemoryMappedViewAccessor.CreateView(position, totalSize)
    //        );
    //        Array = MemoryMarshal.Cast<byte, int>(ByteArray);
    //    }

    //    public void Save(FileStream outputFile)
    //    {
    //        outputFile.Write(ByteArray.Span);
    //    }

    //    public int[] ExactMatchSearch(ReadOnlySpan<byte> key)
    //    {
    //        var result = new int[2] { -1, 0 };
    //        var nodePos = 0;
    //        var unit = Array.Span[nodePos];
    //        foreach (var k in key)
    //        {
    //            nodePos ^= Offset(unit) ^ k;
    //            unit = Array.Span[nodePos];
    //            if (Label(unit) != k)
    //                return result;
    //        }

    //        if (!HasLeaf(unit))
    //            return result;

    //        unit = Array.Span[nodePos ^ Offset(unit)];
    //        result[0] = Value(unit);
    //        result[1] = key.Length;
    //        return result;
    //    }

    //    public List<int[]> CommonPrefixSearch(ReadOnlySpan<byte> key, int offset, int maxNumResult)
    //    {
    //        var result = new List<int[]>();
    //        var nodePos = 0;
    //        var unit = Array.Span[nodePos];
    //        nodePos ^= Offset(unit);

    //        for (var i = offset; i < key.Length; ++i)
    //        {
    //            var k = key[i];
    //            nodePos ^= k;
    //            unit = Array.Span[nodePos];
    //            if (Label(unit) != k)
    //                return result;

    //            nodePos ^= Offset(unit);
    //            if (HasLeaf(unit) && result.Count < maxNumResult)
    //            {
    //                var r = new[] { Value(Array.Span[nodePos]), i + 1 };
    //                result.Add(r);
    //            }
    //        }

    //        return result;
    //    }

    //    public IEnumerable<int[]> CommonPrefixSearch(ReadOnlySpan<byte> key, int offset)
    //    {
    //        var nodePos = 0;
    //        var unit = Array.Span[nodePos];
    //        nodePos ^= Offset(unit);

    //        foreach (var (k, i) in key.EnumerateWithIndex(offset))
    //        {
    //            nodePos ^= k;
    //            unit = Array.Span[nodePos];
    //            if (Label(unit) != k)
    //                yield break;

    //            nodePos ^= Offset(unit);
    //            if (HasLeaf(unit))
    //            {
    //                yield return new[] { Value(Array.Span[nodePos]), i + 1 };
    //            }
    //        }
    //    }

    //    public TraverseResult Traverse(ReadOnlySpan<byte> key, int offset, int length, int nodePosition)
    //    {
    //        var nodePos = nodePosition;
    //        var id = nodePosition;
    //        var unit = Array.Span[nodePosition];

    //        for (var i = offset; i < length; ++i)
    //        {
    //            var k = key[i];
    //            id ^= Offset(unit) ^ k;
    //            unit = Array.Span[id];
    //            if (Label(unit) != k)
    //                return new TraverseResult(-2, i, nodePos);

    //            nodePos = id;
    //        }

    //        return HasLeaf(unit)
    //            ? new TraverseResult(Value(Array.Span[nodePos ^ Offset(unit)]), length, nodePos)
    //            : new TraverseResult(-1, length, nodePos);
    //    }

    //    public TraverseResult Traverse(ReadOnlySpan<byte> key, int offset, int nodePosition)
    //        => Traverse(key, offset, key.Length, nodePosition);

    //    private static bool HasLeaf(int unit) => (unit >> 8 & 1) == 1;

    //    private static int Value(int unit) => unit & 0x7fffffff;

    //    private static int Label(int unit) => unit & -2147483393;

    //    private static int Offset(int unit) => unit >> 10 << ((unit & 512) >> 6);

    //    public record TraverseResult(int Result, int Offset, int NodePosition);
    //}
}
