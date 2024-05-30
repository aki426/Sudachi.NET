using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Sudachi.Net.Core.Dictionary.Build
{
    /// <summary>
    /// IWriteDictionary.WriteToの出力を表すクラス
    /// </summary>
    public class ModelOutput : Stream // JAVA版のSeekableByteChannnelをStreamで代替
    {
        public record Part
        {
            public string Name { get; init; }
            public long Time { get; init; }
            public long Size { get; init; }

            public Part(string name, long time, long size)
            {
                Name = name;
                Time = time;
                Size = size;
            }
        }

        private readonly Stream _internal;
        private readonly List<Part> _parts = new();
        private Progress? _progressor;
        private Stopwatch _stopwatch = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelOutput"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public ModelOutput(Stream stream, Progress progress)
        {
            _internal = stream;
            _progressor = progress ?? null;
        }

        //public void SetProgressor(Progress progress)
        //{
        //    _progressor = progress;
        //}

        /// <summary>
        /// Read from internal stream.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>An int.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _internal.Read(buffer, offset, count);
        }

        /// <summary>
        /// Writes to internal stream.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _internal.Write(buffer, offset, count);
        }

        public override long Position
        {
            get => _internal.Position;
            set => _internal.Position = value;
        }

        public override long Length => _internal.Length;

        public override void SetLength(long value)
        {
            _internal.SetLength(value);
        }

        public override bool CanRead => _internal.CanRead;
        public override bool CanSeek => _internal.CanSeek;
        public override bool CanWrite => _internal.CanWrite;

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _internal.Seek(offset, origin);
        }

        public override void Flush()
        {
            _internal.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _internal.Dispose();
            }
        }

        /// <summary>JAVA版のIORunnableの代替（runメソッドしか無い）。</summary>
        public delegate void IORunnable();

        /// <summary>
        /// 何らかの処理を実行し、その処理にかかった時間とサイズを記録する。
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="inner">The inner.</param>
        public void WithPart(string name, IORunnable inner)
        {
            // TODO: JAVAからの変換の都合上一旦DateTime.Now.Ticksと
            // _stopwatch.Elapsed両方使うadhocな実装にした。どちらかに統合する。
            long pos = Position;
            long start = DateTime.Now.Ticks;
            _stopwatch.Restart();

            _progressor?.StartBlock(name, start, Progress.Kind.Output);

            inner();

            long time = DateTime.Now.Ticks - start;
            _stopwatch.Stop();
            long size = Position - pos;
            _progressor?.EndBlock(size, _stopwatch.Elapsed);

            _parts.Add(new Part(name, time, size));
        }

        /// <summary>JAVA版のSizeRunnableの代替（longを返り値とするrunメソッドしか無い）。</summary>
        public delegate long SizedRunnable();

        /// <summary>
        ///　何らかの処理を実行し、その処理が返すサイズと時間を記録する。
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="inner">The inner.</param>
        public void WithSizedPart(string name, SizedRunnable inner)
        {
            long start = DateTime.Now.Ticks;
            _progressor?.StartBlock(name, start, Progress.Kind.Output);
            _stopwatch.Restart();

            long size = inner();

            long time = DateTime.Now.Ticks - start;
            _stopwatch.Stop();
            _progressor?.EndBlock(size, _stopwatch.Elapsed);

            _parts.Add(new Part(name, time, size));
        }

        // TODO: IReadOnlyListではなくImmutableListへ変更する。利用側も変更が必要。

        /// <summary>処理実行した記録。</summary>
        public IReadOnlyList<Part> Parts => _parts;

        /// <summary>
        /// 100ms以上の間隔でProgressに登録された処理を実行するように制限する関数。
        /// </summary>
        /// <param name="current">The current.</param>
        /// <param name="max">The max.</param>
        public void LimitedProgress(long current, long max)
        {
            _progressor?.LimitedProgress(current, max);
        }
    }
}
