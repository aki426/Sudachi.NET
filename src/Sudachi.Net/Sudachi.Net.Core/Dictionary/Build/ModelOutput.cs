using System;
using System.Collections.Generic;
using System.IO;

namespace Sudachi.Net.Core.Dictionary.Build
{
    public class ModelOutput : Stream
    {
        public delegate void IORunnable();

        public delegate long SizedRunnable();

        public class Part
        {
            public string Name { get; }
            public long Time { get; }
            public long Size { get; }

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

        public ModelOutput(Stream stream)
        {
            _internal = stream;
        }

        public void SetProgressor(Progress progress)
        {
            _progressor = progress;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _internal.Read(buffer, offset, count);
        }

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

        public void WithPart(string name, IORunnable inner)
        {
            long pos = Position;
            long start = DateTime.Now.Ticks;
            _progressor?.StartBlock(name, start, Progress.Kind.Output);

            inner();

            long time = DateTime.Now.Ticks - start;
            long size = Position - pos;
            _progressor?.EndBlock(size, time);
            _parts.Add(new Part(name, time, size));
        }

        public void WithSizedPart(string name, SizedRunnable inner)
        {
            long start = DateTime.Now.Ticks;
            _progressor?.StartBlock(name, start, Progress.Kind.Output);

            long size = inner();

            long time = DateTime.Now.Ticks - start;
            _progressor?.EndBlock(size, time);
            _parts.Add(new Part(name, time, size));
        }

        public IReadOnlyList<Part> Parts => _parts;

        public void ReportProgress(long current, long max)
        {
            _progressor?.ReportProgress(current, max);
        }
    }
}
