﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using DartsClone.Net.Details;

namespace DartsClone.Net
{
    /// <summary>
    /// A trie structure using Double-Array.
    ///
    /// This class provides storage of key-value pairs. The key is an array of bytes,
    /// unique, and does not include <c>\0</c>. The value is a non-negative integer.
    /// </summary>
    public class DoubleArray
    {
        /// <summary>
        /// The result of a traverse operation.
        ///
        /// This class contains the result of a traverse, the position of the key,
        /// and the index of the node after the traverse.
        /// </summary>
        public record TraverseResult
        {
            /// <summary>The result of a traverse</summary>
            public int Result { get; init; }
            /// <summary>The position of the key after a traverse</summary>
            public int Offset { get; init; }
            /// <summary>The node after a traverse</summary>
            public int NodePosition { get; init; }

            public TraverseResult(int result, int offset, int nodePosition)
            {
                Result = result;
                Offset = offset;
                NodePosition = nodePosition;
            }
        }

        // NOTE: ＜C#版の実装方針＞
        // DARTSの計算ロジックで用いるのはIntのBuffer（JAVA）でありintのSpan（C#）である。
        // データの実体としては、JAVAではByteBufferが全ての基本単位となっており、
        // ファイルのIOはByteBufferで行いIntArrayはByteBuffer.asIntBuffferから作成している。
        // これによって実体はByteBufferであるが実体を共有するViewとしてIntのBufferを実現している。
        // C#でもファイルのIOはbyte[]であるし、Memory<byte>.SpanからMemoryMarshallでSpan<int>を作成できるため、
        // Memory<byte>を基本単位としてViewとしてSpan<int>を使用する。

        /// <summary>the structures of Darts-clone as an array of byte</summary>
        private Memory<byte> buffer;

        /// <summary>integer array as structures of Darts-clone.</summary>
        // public Memory<int> array { get; private set; }
        private Span<int> array => MemoryMarshal.Cast<byte, int>(buffer.Span);

        /// <summary>the structures of Darts-clone as an array of byte</summary>
        public ReadOnlyMemory<byte> ByteBuffer => buffer;

        /// <summary>Integer span as structures of Darts-clone.</summary>
        public ReadOnlySpan<int> IntBuffer => array; // MemoryMarshal.Cast<byte, int>(buffer.Span);

        /// <summary>
        /// the number of the internal elements in the structures of Darts-clone.
        ///
        /// It is not the number of keys in the trie.
        /// </summary>
        public int Size => IntBuffer.Length; // { get; private set; } // number of elements

        /// <summary>the size of the storage used in the structures of Darts-clone.</summary>
        public int TotalSize => array.Length * sizeof(int); // JAVAでは4 * size

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleArray"/> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="size">The size.</param>
        public DoubleArray(Memory<byte> buffer)
        {
            // NOTE: 何が入って来るかわからないので、念のためbyte列の長さがintへ変換可能な長さかチェックする。
            if (buffer.Length % sizeof(int) != 0)
            {
                throw new ArgumentException("Buffer length must be a multiple of sizeof(int).");
            }

            this.buffer = buffer;
        }

        ///// <summary>
        ///// Set an integer array as structures of Darts-clone.
        ///// </summary>
        ///// <param name="array">The structures of Darts-clone</param>
        ///// <param name="size">The number of elements</param>
        //public void SetArray(Memory<int> array, int size)
        //{
        //    this.array = array;
        //    this.Size = size;
        //}

        /// <summary>Removes all structures.</summary>
        public void Clear()
        {
            // TODO: 機能としてbuffer.Span.Clear();で代替可能か検討する。
            buffer = Memory<byte>.Empty;
            //Size = 0;
        }

        // NOTE: DoubleArrayBuilderがいるのにBuildメソッドをDoubleArrayが持つのは不合理なので、
        // C#版ではBuildメソッドはDoubleArrayBuilderへ移行。

        ///// <summary>
        ///// Builds the trie from the pairs of key and value.
        /////
        ///// When a pair of key and value is added to trie, <paramref name="progressFunction"/> is
        ///// called with the number of added pairs and the total of pairs.
        ///// </summary>
        ///// <param name="keys">Key with which the specified value is to be associated</param>
        ///// <param name="values">Value to be associated with the specified key</param>
        ///// <param name="progressFunction">The call for showing the progress of building</param>
        //public void Build(byte[][] keys, int[] values, Action<int, int> progressFunction)
        //{
        //    var keySet = new KeySet(keys, values);
        //    var builder = new DoubleArrayBuilder(progressFunction);
        //    builder.Build(keySet);
        //    buffer = new Memory<byte>(builder.Copy());
        //    // TODO: メモリ領域を共有するかしないか、所有権を移譲するかしないかの違いがある。
        //    // このままで良いか注意。
        //    array = new Memory<int>(MemoryMarshal.Cast<byte, int>(buffer.Span).ToArray());
        //    Size = array.Length;
        //}

        ///// <summary>
        ///// Reads the trie from the file.
        ///// </summary>
        ///// <param name="inputFile">The file to read</param>
        ///// <param name="position">The offset to read</param>
        ///// <param name="totalSize">The size to read</param>
        ///// <exception cref="IOException">If reading a file is failed</exception>
        //public void Open(FileStream inputFile, long position, long totalSize)
        //{
        //    if (position < 0)
        //    {
        //        position = 0;
        //    }
        //    if (totalSize <= 0)
        //    {
        //        totalSize = inputFile.Length;
        //    }

        //    ByteArray = new Memory<byte>(new byte[totalSize]);
        //    inputFile.Position = position;
        //    inputFile.Read(ByteArray.Span);
        //    Array = ByteArray.Cast<byte, int>();
        //    Size = Array.Length;
        //}

        /// <summary>
        /// Reads the trie from the file.
        /// </summary>
        /// <param name="fileInfo">The file info.</param>
        /// <param name="position">The position.</param>
        /// <param name="totalSize">The total size.</param>
        public void Open(FileInfo fileInfo, long position, long totalSize)
        {
            if (position < 0)
            {
                position = 0;
            }
            if (totalSize <= 0)
            {
                totalSize = fileInfo.Length;
            }

            // NOTE: JAVA版ではFileChannelからMappedByteBufferを取得しているが、C#版では変換経路が異なり、
            // MemoryMappedFile => MemoryMappedViewAccessor => byte[] => Memory<byte>となる。
            // よって引数としてFileInfoを渡せば十分ということになる。

            // NOTE: より単純には次の方法で良いはずだが、positionとtotalSizeの指定という汎用性のために
            // MemoryMappedViewAccessorを用いている。
            //long fileSize = fileInfo.Length;
            //byte[] byteArray = File.ReadAllBytes(fileInfo.FullName);
            //Memory<byte> memory = new Memory<byte>(byteArray);

            // NOTE: 辞書データのロードの仕方として、MemoryではなくアプリケーションスタックにデータをロードするSpanを使う方法もある。
            // ただし、SudaciDictはFullの場合400MBにも及ぶためスタックオーバーフローを引き起こす可能性があり、Memoryを選択した。

            using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(fileInfo.FullName, FileMode.Open, null, totalSize, MemoryMappedFileAccess.Read))
            {
                using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(position, totalSize, MemoryMappedFileAccess.Read))
                {
                    var buffer = new byte[totalSize];
                    accessor.ReadArray<byte>(0, buffer, 0, buffer.Length);
                    this.buffer = new Memory<byte>(buffer);

                    if (this.buffer.Length % sizeof(int) != 0)
                    {
                        throw new ArgumentException("Buffer length must be a multiple of sizeof(int).");
                    }

                    var intBuffer = new int[this.buffer.Length / sizeof(int)];
                    accessor.ReadArray<int>(0, intBuffer, 0, intBuffer.Length);
                    var intArray = new Memory<int>(intBuffer);

                    int size = intArray.Length;
                }
            }
        }

        /// <summary>
        /// 最も単純にファイルからDoubleArrayを読み込む。
        /// </summary>
        /// <param name="fileInfo">The file info.</param>
        /// <returns>A DoubleArray.</returns>
        public static DoubleArray Load(FileInfo fileInfo)
        {
            // MemoryMappedFileを使うよりももっとずっと単純な方法。
            byte[] bytesLoaded = File.ReadAllBytes(fileInfo.FullName);
            return new DoubleArray(new Memory<byte>(bytesLoaded));
        }

        /// <summary>
        /// Writes this trie to the file.
        /// </summary>
        /// <param name="outputFile">The file to write</param>
        /// <exception cref="IOException">If writing a file is failed</exception>
        public void Save(FileStream outputFile)
        {
            outputFile.Write(buffer.ToArray(), 0, buffer.Length);
        }

        /// <summary>
        /// Returns the value to which the specified key is mapped, or a negative integer
        /// if this trie contains no mapping for the key.
        /// </summary>
        /// <param name="key">The key whose associated value is to be returned</param>
        /// <returns>The array of integer includes the value to which the specified key is
        /// mapped and the length of the key</returns>
        public int[] ExactMatchSearch(byte[] key)
        {
            int[] result = new int[] { -1, 0 };
            int nodePos = 0;
            int unit = array.Span[nodePos];

            foreach (byte k in key)
            {
                nodePos ^= Offset(unit) ^ k;
                unit = array.Span[nodePos];
                if (Label(unit) != k)
                {
                    return result;
                }
            }
            if (!HasLeaf(unit))
            {
                return result;
            }
            unit = array.Span[nodePos ^ Offset(unit)];
            result[0] = Value(unit);
            result[1] = key.Length;
            return result;
        }

        /// <summary>
        /// Returns the values to which the prefixes of the specified key is mapped.
        ///
        /// If <paramref name="offset"/> is not 0, the key is evaluated as the sub array removed the
        /// first <paramref name="offset"/> bytes.
        /// </summary>
        /// <param name="key">The key whose associated value is to be returned</param>
        /// <param name="offset">The offset of the key</param>
        /// <param name="maxNumResult">The maximum size of the list</param>
        /// <returns>The list of the array of integer includes the value to which the
        /// prefix of the specified key is mapped and the length of the prefix</returns>
        public List<int[]> CommonPrefixSearch(byte[] key, int offset, int maxNumResult)
        {
            var result = new List<int[]>();

            int nodePos = 0;
            int unit = array.Span[nodePos];
            nodePos ^= Offset(unit);
            for (int i = offset; i < key.Length; i++)
            {
                byte k = key[i];
                nodePos ^= k;
                unit = array.Span[nodePos];
                if (Label(unit) != k)
                {
                    return result;
                }

                nodePos ^= Offset(unit);
                if (HasLeaf(unit) && result.Count < maxNumResult)
                {
                    int[] r = new int[] { Value(array.Span[nodePos]), i + 1 };
                    result.Add(r);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the values to which the prefixes of the specified key is mapped.
        ///
        /// This method is equivalent to <c>commonPrefixSearch().iterator()</c>.
        ///
        /// If <paramref name="offset"/> is not 0, the key is evaluated as the sub array removed the
        /// first <paramref name="offset"/> bytes.
        /// </summary>
        /// <param name="key">The key whose associated value is to be returned</param>
        /// <param name="offset">The offset of the key</param>
        /// <returns>The list of the array of integer includes the value to which the
        /// prefix of the specified key is mapped and the length of the prefix</returns>
        public IEnumerator<int[]> CommonPrefixSearch(byte[] key, int offset)
        {
            return new Itr(key, offset, this);
        }

        /// <summary>
        /// Returns the value to which the specified key is mapped by traversing the trie
        /// from the specified node.
        ///
        /// If <paramref name="nodePosition"/> is 0, starts traversing from the root node.
        ///
        /// If <paramref name="offset"/> is not 0, the key is evaluated as the sub array removed the
        /// first <paramref name="offset"/> bytes.
        ///
        /// If <paramref name="length"/> is smaller than <c>key.Length</c>, the remains are ignored.
        ///
        /// Returns -1 as the value if a traverse is failed at the end of the key, or -2
        /// at a middle of the key.
        /// </summary>
        /// <param name="key">The key whose associated value is to be returned</param>
        /// <param name="offset">The offset of the key</param>
        /// <param name="length">The end of the key</param>
        /// <param name="nodePosition">The node to start a traverse</param>
        /// <returns>The value to which the specified key is mapped, the offset of the
        /// key, and the node after the traverse</returns>
        public TraverseResult Traverse(byte[] key, int offset, int length, int nodePosition)
        {
            int nodePos = nodePosition;
            int id = nodePos;
            int unit = array.Span[id];

            for (int i = offset; i < length; i++)
            {
                byte k = key[i];
                id ^= Offset(unit) ^ k;
                unit = array.Span[id];
                if (Label(unit) != k)
                {
                    return new TraverseResult(-2, i, nodePos);
                }
                nodePos = id;
            }
            if (!HasLeaf(unit))
            {
                return new TraverseResult(-1, length, nodePos);
            }
            unit = array.Span[nodePos ^ Offset(unit)];
            return new TraverseResult(Value(unit), length, nodePos);
        }

        private bool HasLeaf(int unit) => (unit >> 8 & 1) == 1;

        private int Value(int unit) => unit & 0x7fffffff;

        private int Label(int unit) => unit & -2147483393;

        private int Offset(int unit) => unit >> 10 << ((unit & 512) >> 6);

        /// <summary>
        /// Returns the value to which the specified key is mapped by traversing the trie
        /// from the specified node.
        ///
        /// If <paramref name="nodePosition"/> is 0, starts traversing from the root node.
        ///
        /// If <paramref name="offset"/> is not 0, the key is evaluated as the sub array removed the
        /// first <paramref name="offset"/> bytes.
        ///
        /// Returns -1 as the value if a traverse is failed at the end of the key, or -2
        /// at a middle of the key.
        /// </summary>
        /// <param name="key">The key whose associated value is to be returned</param>
        /// <param name="offset">The offset of the key</param>
        /// <param name="nodePosition">The node to start a traverse</param>
        /// <returns>The value to which the specified key is mapped, the offset of the
        /// key, and the node after the traverse</returns>
        public TraverseResult Traverse(byte[] key, int offset, int nodePosition)
        {
            return Traverse(key, offset, key.Length, nodePosition);
        }

        private class Itr : IEnumerator<int[]>
        {
            private readonly byte[] key;
            private int offset;
            private int nodePos;
            private int[] next;
            private readonly DoubleArray doubleArray;

            public Itr(byte[] key, int offset, DoubleArray doubleArray)
            {
                this.key = key;
                this.offset = offset;
                this.doubleArray = doubleArray;
                nodePos = 0;
                int unit = doubleArray.array.Span[nodePos];
                nodePos ^= doubleArray.Offset(unit);
                next = null;
            }

            public bool MoveNext()
            {
                if (next == null)
                {
                    next = GetNext();
                }
                return next != null;
            }

            public int[] Current
            {
                get
                {
                    int[] r = next ?? GetNext();
                    next = null;
                    if (r == null)
                    {
                        throw new InvalidOperationException();
                    }
                    return r;
                }
            }

            object System.Collections.IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            private int[] GetNext()
            {
                for (; offset < key.Length; offset++)
                {
                    byte k = key[offset];
                    nodePos ^= k;
                    int unit = doubleArray.array.Span[nodePos];
                    if (doubleArray.Label(unit) != k)
                    {
                        offset = key.Length; // no more loop
                        return null;
                    }

                    nodePos ^= doubleArray.Offset(unit);
                    if (doubleArray.HasLeaf(unit))
                    {
                        int[] r = new int[] { doubleArray.Value(doubleArray.array.Span[nodePos]), ++offset };
                        return r;
                    }
                }
                return null;
            }
        }
    }
}
