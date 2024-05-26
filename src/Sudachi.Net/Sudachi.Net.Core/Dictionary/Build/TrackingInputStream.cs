using System.IO;

namespace Sudachi.Net.Core.Dictionary.Build
{
    public class TrackingStream : Stream
    {
        private readonly Stream _inner;
        private long _position;

        public TrackingStream(Stream inner)
        {
            _inner = inner;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = _inner.Read(buffer, offset, count);
            if (read != -1)
            {
                _position += read;
            }
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPosition = _inner.Seek(offset, origin);
            _position = newPosition;
            return newPosition;
        }

        public override void SetLength(long value)
        {
            _inner.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _inner.Write(buffer, offset, count);
            _position += count;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;
        public override long Length => _inner.Length;

        public override long Position
        {
            get => _position;
            set
            {
                _inner.Position = value;
                _position = value;
            }
        }

        public override void Flush()
        {
            _inner.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _inner.Dispose();
            }
        }
    }
}
