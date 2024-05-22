using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text;

using FluentAssertions;
using Xunit;
using DartsClone.Net;
using System.Runtime.InteropServices;
using Xunit.Abstractions;
using System.IO.MemoryMappedFiles;
using System.IO;

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
        public void SaveAndOpenMemoryTest()
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, "SaveAndOpenMemoryTest.dat");

            #region メモリをファイルに保存する

            int[] originalIntArray = new int[] { 1, 2, 4, 8, 65280, 2130706432 };
            Memory<int> intMemory = new Memory<int>(originalIntArray);

            // Memory<int>をfilePathに保存
            byte[] originalBytesArray = MemoryMarshal.AsBytes(intMemory.Span).ToArray();
            Memory<byte> byteMemory = new Memory<byte>(originalBytesArray);

            // int[]とbyte[]の中身を確認
            _output.WriteLine($"original int[]: {originalIntArray.Length}");
            _output.WriteLine(ConvertToHexStr(originalIntArray));
            // intをHEX表示したときは普通に表示される（リトルエンディアン、ビッグエンディアンの区別はない）
            ConvertToHexStr(originalIntArray).Should().Be("00000001 - 00000002 - 00000004 - 00000008 - 0000FF00 - 7F000000");

            _output.WriteLine($"original byte[]: {originalBytesArray.Length}");
            _output.WriteLine(ConvertToHexStr(originalBytesArray));
            // バイト配列に変換するとリトルエンディアンになる。
            ConvertToHexStr(originalBytesArray).Should().Be("01 - 00 - 00 - 00 - 02 - 00 - 00 - 00 - 04 - 00 - 00 - 00 - 08 - 00 - 00 - 00 - 00 - FF - 00 - 00 - 00 - 00 - 00 - 7F");

            // Memory<int>とMemory<byte>が実体を共有しているかチェック
            intMemory.Span[0].Should().Be(1);
            intMemory.Span[0] = 256;
            intMemory.Span[0].Should().Be(256);
            // int[]とは共有する。
            originalIntArray[0].Should().Be(256);

            // この方法ではMemory<int>とMemory<byte>はメモリを共有しない。
            byteMemory.Span[0].Should().Be(1); // 0x01
            byteMemory.Span[1].Should().Be(0); // 0x00

            // FileStreamを使ってファイルを開く
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                // バイト配列をファイルに書き込む
                // fs.Write(originalBytesArray, 0, originalBytesArray.Length);

                // Memory<int>からbyte[]に変換して書き込む
                var writeBytes = MemoryMarshal.AsBytes(intMemory.Span).ToArray();
                fs.Write(writeBytes, 0, writeBytes.Length);
            }

            #endregion メモリをファイルに保存する

            #region ファイルをメモリに読み込む - 配列に変換する方法。

            // ファイルからバイト配列を読み込む
            byte[] bytesLoaded = File.ReadAllBytes(filePath);

            // バイト配列をMemory<byte>に変換
            Memory<byte> byteMemoryLoaded = new Memory<byte>(bytesLoaded);

            // バイト配列をMemory<int>に変換
            // NOTE: MemoryMarshal.CastはSpanを経由してしまうがこれが良いのかどうか検証不足。
            // Span<int> intMemoryLoaded = MemoryMarshal.Cast<byte, int>(bytes);
            byte[] temp = new byte[sizeof(int)];
            Memory<int> intMemoryLoaded = new Memory<int>(Enumerable.Range(0, bytesLoaded.Length / sizeof(int))
                .Select(i =>
                {
                    Buffer.BlockCopy(bytesLoaded, i * sizeof(int), temp, 0, sizeof(int));
                    return BitConverter.ToInt32(temp, 0);
                })
                .ToArray());

            // assertion
            byteMemoryLoaded.Length.Should().Be(24);
            intMemoryLoaded.Length.Should().Be(6);

            _output.WriteLine($"loaded int[]: {intMemoryLoaded.Length}");
            _output.WriteLine(ConvertToHexStr(intMemoryLoaded.Span.ToArray()));
            // _output.WriteLine(ConvertToHexStr(intMemoryLoaded.ToArray()));
            ConvertToHexStr(intMemoryLoaded.Span.ToArray()).Should().Be(ConvertToHexStr(originalIntArray));

            _output.WriteLine($"loaded byte[]: {byteMemoryLoaded.Length}");
            _output.WriteLine(ConvertToHexStr(byteMemoryLoaded.Span.ToArray()));
            // _output.WriteLine(ConvertToHexStr(byteMemoryLoaded.ToArray()));
            // ファイルはoriginalIntArrayもしくはintMemoryと内容が一致するのでそちらでマッチングを見る。
            ConvertToHexStr(byteMemoryLoaded.Span.ToArray())
                .Should().Be(ConvertToHexStr(MemoryMarshal.AsBytes(intMemory.Span).ToArray()));

            // 読み込んだ値のチェック
            intMemoryLoaded.Span[0].Should().Be(256);
            byteMemoryLoaded.Span[0].Should().Be(0); // 0x00
            byteMemoryLoaded.Span[1].Should().Be(1); // 0x01 => 0x0100 = 256

            // 変更
            intMemoryLoaded.Span[0] = 1;
            intMemoryLoaded.Span[0].Should().Be(1);

            // この方法ではMemory<int>とMemory<byte>はメモリを共有しない。
            byteMemoryLoaded.Span[0].Should().Be(0); // 0x00
            byteMemoryLoaded.Span[1].Should().Be(1); // 0x01

            #endregion ファイルをメモリに読み込む - 配列に変換する方法。

            #region ファイルをメモリに読み込む - Memory関係のクラスを使う方法。

            //using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(fileInfo.FullName, FileMode.Open, null, totalSize, MemoryMappedFileAccess.Read))
            //{
            //    using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(position, totalSize, MemoryMappedFileAccess.Read))
            //    {
            //        var buffer = new byte[totalSize];
            //        accessor.ReadArray<byte>(0, buffer, 0, buffer.Length);
            //        this.buffer = new Memory<byte>(buffer);

            //        if (this.buffer.Length % sizeof(int) != 0)
            //        {
            //            throw new ArgumentException("Buffer length must be a multiple of sizeof(int).");
            //        }

            //        var intBuffer = new int[this.buffer.Length / sizeof(int)];
            //        accessor.ReadArray<int>(0, intBuffer, 0, intBuffer.Length);
            //        var intArray = new Memory<int>(intBuffer);

            //        int size = intArray.Length;
            //    }
            //}

            #endregion ファイルをメモリに読み込む - Memory関係のクラスを使う方法。
        }
    }
}
