using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

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
        /// Unitの下から9ビット目が1＝Leafを持つ場合にtrueを返す。
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <returns>A bool.</returns>
        public static bool HasLeaf(int unit) => (unit >>> 8 & 1) == 1;

        /// <summary>
        /// UnitのValue部を取得する。32bit intの最上位ビットを除いた31ビットを返す。
        /// 0x7fffffff = (1 << 31) - 1
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <returns>An int.</returns>
        public static int Value(int unit) => unit & 0x7fffffff;

        /// <summary>
        /// UnitのLabel部を取得する。
        /// (1 << 31) | 0xFF = 0x800000FF = -2147483393:
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <returns>An int.</returns>
        public static int Label(int unit) => unit & ((1 << 31) | 0xFF);

        /// <summary>
        /// UnitのOffset部を取得する。
        /// 下から10ビット捨てる。10ビット目が1なら8、0なら0ビット、切り捨てた10ビットから11ビット目以上を左シフトする。
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <returns>An int.</returns>
        public static int Offset(int unit) => (unit >>> 10) << ((unit & (1 << 9)) >>> 6);

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

        // NOTE: 出来るだけDIかつImmutableなコードにしたいため、コンストラクタでMemory<byte>を渡した後は変更しない。

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

        /// <summary>Removes all structures.</summary>
        public void Clear()
        {
            // TODO: 機能としてbuffer.Span.Clear();で代替可能か検討する。
            buffer = Memory<byte>.Empty;
            //Size = 0;
        }

        // NOTE: DoubleArrayBuilderがいるのにBuildメソッドをDoubleArrayが持つのは不合理なので、
        // C#版ではBuildメソッドはDoubleArrayBuilderへ移行。

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

            using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(fileInfo.FullName, FileMode.Open, null, totalSize, MemoryMappedFileAccess.Read))
            {
                using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(position, totalSize, MemoryMappedFileAccess.Read))
                {
                    var byteArray = new byte[totalSize];
                    accessor.ReadArray<byte>(0, byteArray, 0, byteArray.Length);
                    this.buffer = new Memory<byte>(byteArray);

                    if (this.buffer.Length % sizeof(int) != 0)
                    {
                        throw new ArgumentException("Buffer length must be a multiple of sizeof(int).");
                    }
                }
            }
        }

        /// <summary>
        /// 最も単純にファイルからDoubleArrayを読み込む。
        /// NOTE: 読み取りのオフセットとサイズは指定できない。まるごと全部読み込む。
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
        /// Writes this trie to the file.
        /// </summary>
        /// <param name="fileInfo">The file info.</param>
        public void Save(FileInfo fileInfo)
        {
            File.WriteAllBytes(fileInfo.FullName, buffer.ToArray());
        }

        /// <summary>
        /// Returns the value to which the specified key is mapped, or a negative integer
        /// if this trie contains no mapping for the key.
        /// </summary>
        /// <param name="key">The key whose associated value is to be returned</param>
        /// <returns>The array of integer includes the value to which the specified key is
        /// mapped and the length of the key</returns>
        public int[] ExactMatchSearch(params byte[] key)
        {
            int[] result = new int[] { -1, 0 };
            int nodePos = 0;
            int unit = array[nodePos];

            foreach (byte k in key)
            {
                nodePos ^= Offset(unit) ^ k;
                unit = array[nodePos];
                if (Label(unit) != k)
                {
                    return result;
                }
            }
            if (!HasLeaf(unit))
            {
                return result;
            }
            unit = array[nodePos ^ Offset(unit)];
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
            int unit = array[nodePos];
            nodePos ^= Offset(unit);
            for (int i = offset; i < key.Length; i++)
            {
                byte k = key[i];
                nodePos ^= k;
                unit = array[nodePos];
                if (Label(unit) != k)
                {
                    return result;
                }

                nodePos ^= Offset(unit);
                if (HasLeaf(unit) && result.Count < maxNumResult)
                {
                    int[] r = new int[] { Value(array[nodePos]), i + 1 };
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
            int unit = array[id];

            for (int i = offset; i < length; i++)
            {
                byte k = key[i];
                id ^= Offset(unit) ^ k;
                unit = array[id];
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
            unit = array[nodePos ^ Offset(unit)];
            return new TraverseResult(Value(unit), length, nodePos);
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
                int unit = doubleArray.array[nodePos];
                nodePos ^= Offset(unit);
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
                // NOTE: Memory<byte>はマネージドリソースなのでGC任せでよくDispose不要。
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
                    int unit = doubleArray.array[nodePos];
                    if (Label(unit) != k)
                    {
                        offset = key.Length; // no more loop
                        return null;
                    }

                    nodePos ^= Offset(unit);
                    if (HasLeaf(unit))
                    {
                        int[] r = new int[] { Value(doubleArray.array[nodePos]), ++offset };
                        return r;
                    }
                }
                return null;
            }
        }
    }
}
