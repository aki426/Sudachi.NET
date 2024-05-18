using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sudachi.Net.Core.SentDetect;
using System.Xml.Linq;
using System.Net;

namespace Sudachi.Net.Core.SentDetect
{
    /// <summary>
    /// A container for the resource, allowing to combine lazy loading with providing
    /// prebuilt resources. Use <see cref="PathAnchor"/> to create resources.
    /// </summary>
    /// <typeparam name="T">Resource type of the built resource</typeparam>
    public abstract record Resource<T>
    {
        /// <summary>
        /// Create a real resource instance. File loading should be done inside the creator function.
        /// </summary>
        /// <param name="creator">Creator function</param>
        /// <returns>Created resource</returns>
        /// <exception cref="IOException">When IO fails</exception>
        public virtual T Consume(IOFunction<Resource<T>, T> creator) => creator(this);

        /// <summary>
        /// Open this resource as readable Stream. User should close it when the reading is done.
        /// </summary>
        /// <returns>Readable Stream.</returns>
        /// <exception cref="IOException">When IO fails</exception>
        public virtual Stream AsInputStream() => throw new NotSupportedException();

        /// <summary>
        /// Get view of this resource as a byte array. When it is possible, the data will be memory mapped,
        /// if it is not possible, it will be fully read into the memory.
        /// Will not work for files more than 2^31 bytes (2 GB) in size.
        /// </summary>
        /// <returns>Byte array containing the whole contents of the file</returns>
        /// <exception cref="IOException">When IO fails</exception>
        public virtual byte[] AsByteArray() => throw new NotSupportedException();

        /// <summary>
        /// Returns internal representation (for tests)
        /// </summary>
        /// <returns>Internal representation</returns>
        public virtual object Repr() => null;

        /// <summary>
        /// Filesystem-backed resource
        /// </summary>
        /// <typeparam name="T">Resource</typeparam>
        public sealed record FilesystemResource<T>(string Path) : Resource<T>
        {
            public override Stream AsInputStream() => File.OpenRead(Path);
            public override byte[] AsByteArray() => File.ReadAllBytes(Path);
            public override string ToString() => Path;
            public override object Repr() => Path;
        }

        /// <summary>
        /// Resource which is in Java classpath.
        /// </summary>
        /// <typeparam name="T">Resulting resource type</typeparam>
        public sealed record ClasspathResource<T>(Uri Url) : Resource<T>
        {
            public override Stream AsInputStream() => new WebClient().OpenRead(Url);

            public override byte[] AsByteArray() =>
                Url.IsFile
                    ? File.ReadAllBytes(Url.LocalPath)
                    : new WebClient().DownloadData(Url);

            public override string ToString() => Url.ToString();
            public override object Repr() => Url;
        }

        /// <summary>
        /// Prebuilt resource.
        /// </summary>
        /// <typeparam name="T">Resulting resource type</typeparam>
        private sealed record ReadyResource<T>(T Object) : Resource<T>
        {
            public override T Consume(IOFunction<Resource<T>, T> creator) => Object;
            public override object Repr() => Object;
        }

        public sealed record NotFoundResource<T>(string Path, PathAnchor Anchor) : Resource<T>
        {
            public override T Consume(IOFunction<Resource<T>, T> creator) => throw MakeException();
            public override Stream AsInputStream() => throw MakeException();
            public override byte[] AsByteArray() => throw MakeException();
            public override object Repr() => Path;

            private Exception MakeException() =>
                new ArgumentException($"Failed to resolve file: {Path}\nTried roots: {Anchor}");
        }

        /// <summary>
        /// Create a resource wrapper for a prebuilt resource
        /// </summary>
        /// <typeparam name="T">Type of the prebuilt resource</typeparam>
        /// <param name="obj">Prebuilt resource</param>
        /// <returns>Wrapper</returns>
        public static Resource<T> Ready<T>(T obj) => new ReadyResource<T>(obj);
    }

    /// <summary>
    /// IO Function delegate
    /// </summary>
    /// <typeparam name="T">Input type</typeparam>
    /// <typeparam name="R">Output type</typeparam>
    /// <param name="arg">Input argument</param>
    /// <returns>Output result</returns>
    /// <exception cref="IOException">When IO fails</exception>
    public delegate R IOFunction<T, R>(T arg);

    // NOTE: 元々のJAVAコードは次のInterfaceだった。
    // TODO: Delegateで良ければこのままDelegateで進める。
    //@FunctionalInterface
    //public interface IOFunction<T, R>
    //{
    //    R apply(T arg) throws IOException;
    //}
}
