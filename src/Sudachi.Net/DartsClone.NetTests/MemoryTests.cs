using System.IO.MemoryMappedFiles;
using System.IO;
using System.Runtime.InteropServices;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using System.Diagnostics;

namespace DartsClone.NetTests
{
    public class MemoryTests
    {
        private ITestOutputHelper _output;

        public MemoryTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private string ConvertToHexStr(int[] intArray)
            => string.Join(" - ", intArray.Select(x => x.ToString("X8")));

        private string ConvertToHexStr(byte[] byteArray)
            => string.Join(" - ", byteArray.Select(x => x.ToString("X2")));

        private string ConvertToHexStr(Span<int> intSpan)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < intSpan.Length; i++)
            {
                result.Add(intSpan[i].ToString("X8"));
            }

            return string.Join(" - ", result);
        }

        private string ConvertToHexStr(Span<byte> byteSpan)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < byteSpan.Length; i++)
            {
                result.Add(byteSpan[i].ToString("X2"));
            }

            return string.Join(" - ", result);
        }

        // NOTE: GCを避けて高速なコードにするために、MemoryまたはSpanを使う必要がある。
        // Spanをメンバーにしようとするとすべてref structになってしまうので、DoubleArrayの実装としては向かない。
        // なお、SpanにはSlow SpanとFast Spanがあり、.NET Standard 2.0以前または.Net Frameworkの場合Slow Spanになってしまう。
        // より高い性能が必要な場合は.NET Standard 2.1以降または.NET*へフレームワークを移したものを用意した方が良い。
        // https://ufcpp.net/study/csharp/resource/span/

        // NOTE: intとbyteの型変換にはいくつかの方法がある。
        // 1. BitConverter.GetBytes(int) / BitConverter.ToInt32(byte[], int)
        // 2. Buffer.BlockCopy(byte[], int, byte[], int, int)
        // 3. Marshal.Copy(IntPtr, byte[], int, int)
        // 4. unsafeコードでポインタを使う
        // 5. Span<byte>を使う
        // 6. MemoryMarshal.Cast<byte, int>(byte[])を使う
        // 7. MemoryMarshal.AsBytes(Memory<int>.Span)を使う
        // 8. MemoryMarshal.TryGetArray(Memory<int>, out ArraySegment<byte>)を使う
        // 9. MemoryMarshal.CreateFromPinnedArray(byte[])を使う
        // なるべくメモリ消費は抑えて、最終的にDoubleArrayで扱うintとbyteがメモリを共有できるようにする。
        // 変換はたかだか1回しか行われないし、辞書はファイルから読み込んだ場合実行中は不変としてよいはずなので
        // 一度読み込んだら内容を変更する際に多少コストがかかってもよい。

        [Fact]
        public void MemoryTest()
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, "SaveAndOpenMemoryTest.dat");

            #region メモリをファイルに保存する

            // Memory / Spanの最初の起点は配列から作成する。
            int[] originalIntArray = new int[] { 1, 2, 4, 8, 65280, 2130706432 };
            Memory<int> intMemory = new Memory<int>(originalIntArray);

            // Span<byte>は元のSpan<int>のビューとして振る舞う。
            var byteSpan = MemoryMarshal.Cast<int, byte>(intMemory.Span);

            // int[], Memory<int>, Span<byte>の中身を確認
            _output.WriteLine($"original int[]: {originalIntArray.Length}");
            _output.WriteLine(ConvertToHexStr(originalIntArray));
            // intをHEX表示したときは普通に表示される（リトルエンディアン、ビッグエンディアンの区別はない）
            ConvertToHexStr(originalIntArray).Should().Be("00000001 - 00000002 - 00000004 - 00000008 - 0000FF00 - 7F000000");

            _output.WriteLine($"original byte[]: {byteSpan.Length}");
            _output.WriteLine(ConvertToHexStr(byteSpan));
            // バイト配列に変換するとリトルエンディアンになる。
            ConvertToHexStr(byteSpan).Should().Be("01 - 00 - 00 - 00 - 02 - 00 - 00 - 00 - 04 - 00 - 00 - 00 - 08 - 00 - 00 - 00 - 00 - FF - 00 - 00 - 00 - 00 - 00 - 7F");

            intMemory.Span[0] = 256;
            intMemory.Span[0].Should().Be(256);
            // int[]とは共有する。
            originalIntArray[0].Should().Be(256);
            // Span<byte>もビューに過ぎないのでメモリを共有している。
            ConvertToHexStr(byteSpan.Slice(0, 4)).Should().Be("00 - 01 - 00 - 00");

            byteSpan[2] = 1;
            _output.WriteLine(ConvertToHexStr(intMemory.Span));
            intMemory.Span[0].Should().Be(65792);

            // int[]とMemory<int>とSpan<byte>は実体を共有する。
            ConvertToHexStr(originalIntArray).Should().Be(ConvertToHexStr(intMemory.Span));
            ConvertToHexStr(byteSpan).Should().Be(ConvertToHexStr(MemoryMarshal.AsBytes(intMemory.Span)));

            // FileStreamを使ってファイルを開く
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                // FileStreamにはReadOnlySpan<byte>を直接書き込むWrite関数がある。便利。
                fs.Write(MemoryMarshal.Cast<int, byte>(intMemory.Span));
            }

            #endregion メモリをファイルに保存する

            #region ファイルをメモリに読み込む - 配列に変換する方法。

            // NOTE: IOがbyte単位なので、Memory<int>はbyte[] -> Memory<byte> -> Span<byte> -> Memory<int>と変換するのが自然。
            // これはJAVA版のByteBuffer.asIntBufferに通じるものがある。

            // NOTE: JAVA版ではFileChannel.mapを用いてByteBufferをFileと同期させているが、この機能が必要とも思えない。
            // C#版ではMemoryMappedFileを使うことで同様の機能を実現できるが、その必要性があるかは検証不足。

            // ファイルからバイト配列を読み込む
            byte[] bytesLoaded = File.ReadAllBytes(filePath);
            MemoryMarshal.Cast<byte, int>(bytesLoaded);
            // バイト配列をMemory<byte>に変換
            Memory<byte> byteMemoryLoaded = new Memory<byte>(bytesLoaded);

            // おそらくMemory<byte>を生成した場合は、Memory<int>を別途持つ必要は無いはず。
            Span<int> intSpanLoaded = MemoryMarshal.Cast<byte, int>(byteMemoryLoaded.Span);

            byteMemoryLoaded.Span[0].Should().Be(0);
            byteMemoryLoaded.Span[1].Should().Be(1);
            byteMemoryLoaded.Span[2].Should().Be(1);

            intSpanLoaded[0].Should().Be(65792);
            intSpanLoaded[1].Should().Be(2);

            byteMemoryLoaded.Span[4] = 8;
            intSpanLoaded[1].Should().Be(8);

            intSpanLoaded[1] = 15;
            byteMemoryLoaded.Span[4].Should().Be(15);

            #endregion ファイルをメモリに読み込む - 配列に変換する方法。
        }

        /// <summary>
        /// Sudachi-Dictの読み込みテスト。
        /// NOTE: 結果としては以下のとおりで、ファイルとメモリを同期させないのであれば普通に読み込んだ方が早い。
        /// * MemoryMappedFile: 315ms
        /// * File.ReadAllBytes: 143ms
        /// </summary>
        [Fact]
        public void LoadDict()
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, "DATA", "system_core.dic");
            long totalSize = new FileInfo(filePath).Length;

            Memory<byte> byteMemory;

            Stopwatch sw = new Stopwatch();

            sw.Start();
            // MemoryMappedFile, MemoryMappedViewAccessorを経由して読み取り専用でファイルを読み込む。
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, totalSize, MemoryMappedFileAccess.Read))
            {
                using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(0, totalSize, MemoryMappedFileAccess.Read))
                {
                    var buffer = new byte[totalSize];
                    accessor.ReadArray<byte>(0, buffer, 0, buffer.Length);
                    byteMemory = new Memory<byte>(buffer);

                    if (byteMemory.Length % sizeof(int) != 0)
                    {
                        throw new ArgumentException("Buffer length must be a multiple of sizeof(int).");
                    }

                    // Span<int>も取得可能。
                    MemoryMarshal.Cast<byte, int>(byteMemory.Span);
                }
            }
            sw.Stop();
            _output.WriteLine($"MemoryMappedFile: {sw.ElapsedMilliseconds}ms");

            // NOTE: Memory<byte>に書き込むとファイルにも反映されるような関連付けも可能で、次のとおり。
            //using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, fileSize, MemoryMappedFileAccess.ReadWrite))
            //{
            //    using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(0, fileSize, MemoryMappedFileAccess.ReadWrite))
            //    {
            //        // ...
            //    }
            //}
            // とはいえ、Sudachi-Dictは読み込み専用で十分なので、この機能は必要ない。
            // Dartsとして必要かどうかは要検証だが、そのような応用的な実装が必要になった場合は別途対応で良いだろう。

            sw.Restart();
            // もっとずっと単純な方法。
            // ファイルからバイト配列を読み込む
            byte[] bytesLoaded = File.ReadAllBytes(filePath);
            // バイト配列をMemory<byte>に変換
            byteMemory = new Memory<byte>(bytesLoaded);
            // Span<int>も取得可能。
            _ = MemoryMarshal.Cast<byte, int>(byteMemory.Span);
            sw.Stop();
            _output.WriteLine($"File.ReadAllBytes: {sw.ElapsedMilliseconds}ms");
        }
    }
}
