using System;
using System.Collections.Generic;
using System.Text;

namespace Sudachi.Net.Core.Dictionary.Build
{
    public class DicBuilder
    {
        private DicBuilder()
        {
            /* instantiations are forbidden */
        }

        public static SystemNoMatrix System()
        {
            return new SystemNoMatrix(new System());
        }

        public static User User(DictionaryAccess system)
        {
            return new User(system);
        }

        public abstract class Base<T> where T : Base<T>
        {
            protected readonly POSTable Pos = new POSTable();
            protected readonly ConnectionMatrix Connection = new ConnectionMatrix();
            protected readonly Index Index = new Index();
            protected string Description = "";
            protected long Version;
            protected long CreationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            private readonly List<ModelOutput.Part> _inputs = new List<ModelOutput.Part>();
            private IProgress _progress;

            protected CsvLexicon Lexicon { get; } = new CsvLexicon(Pos);

            protected virtual IWordIdResolver Resolver()
            {
                return new WordLookup.Csv(Lexicon);
            }

            private T Self()
            {
                return (T)this;
            }

            public BuildStats Build(Stream result)
            {
                Lexicon.SetResolver(Resolver());
                ModelOutput output = new ModelOutput(result);
                if (_progress != null)
                {
                    output.SetProgressor(_progress);
                }
                DictionaryHeader header = new DictionaryHeader(Version, CreationTime, Description);

                byte[] headerBuffer = header.ToBytes();

                output.Write(headerBuffer, 0, headerBuffer.Length);
                Pos.WriteTo(output);
                Connection.WriteTo(output);
                Index.WriteTo(output);
                Lexicon.WriteTo(output);
                return new BuildStats(_inputs, output.Parts);
            }

            public T Lexicon(string data)
            {
                using (WebClient client = new WebClient())
                {
                    byte[] bytes = client.DownloadData(data);
                    using (MemoryStream stream = new MemoryStream(bytes))
                    {
                        return LexiconImpl(new Uri(data).LocalPath, stream, bytes.Length);
                    }
                }
            }

            public T Lexicon(string path)
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    return LexiconImpl(Path.GetFileName(path), stream, stream.Length);
                }
            }

            public T Lexicon(Stream data)
            {
                return LexiconImpl("<input stream>", data, data.Length);
            }

            public T LexiconImpl(string name, Stream data, long size)
            {
                long startTime = DateTime.UtcNow.Ticks;
                if (_progress != null)
                {
                    _progress.StartBlock(name, startTime, Progress.Kind.Input);
                }

                TrackingStream tracker = new TrackingStream(data);
                using (StreamReader reader = new StreamReader(tracker, Encoding.UTF8))
                using (CsvReader parser = new CsvReader(reader))
                {
                    int line = 1;
                    while (parser.Read())
                    {
                        try
                        {
                            CsvLexicon.WordEntry e = Lexicon.ParseLine(parser.GetRecord<IReadOnlyList<string>>());
                            int wordId = Lexicon.AddEntry(e);
                            if (e.Headword != null)
                            {
                                Index.Add(e.Headword, wordId);
                            }
                            line++;
                        }
                        catch (Exception ex)
                        {
                            throw new InputFileException(line, parser.GetField(0), ex);
                        }
                        if (_progress != null)
                        {
                            _progress.ReportProgress(tracker.Position, size);
                        }
                    }
                }

                long time = DateTime.UtcNow.Ticks - startTime;
                if (_progress != null)
                {
                    _progress.EndBlock(line, new TimeSpan(time));
                }

                _inputs.Add(new ModelOutput.Part(name, time, line));

                return Self();
            }

            public T Description(string description)
            {
                Description = description;
                return Self();
            }

            public T Progress(IProgress progress)
            {
                _progress = progress;
                return Self();
            }
        }

        public sealed class System : Base<System>
        {
            public System()
            {
                Version = DictionaryVersion.SystemDictVersion2;
            }

            private void ReadMatrix(Stream matrix)
            {
                Connection.ReadEntries(matrix);
                Lexicon.SetLimits(Connection.NumLeft, Connection.NumRight);
            }
        }

        public sealed class User : Base<User>
        {
            private readonly DictionaryAccess _dictionary;

            internal User(DictionaryAccess dictionary)
            {
                _dictionary = dictionary;
                Version = DictionaryVersion.UserDictVersion3;
                Connection conn = _dictionary.Grammar.Connection;
                Lexicon.SetLimits(conn.LeftSize, conn.RightSize);
                Connection.MakeEmpty();
                Pos.PreloadFrom(_dictionary.Grammar);
            }

            protected override IWordIdResolver Resolver()
            {
                return new WordLookup.Chain(new WordLookup.Prebuilt(_dictionary.Lexicon), new WordLookup.Csv(Lexicon));
            }
        }

        public sealed class SystemNoMatrix
        {
            private readonly System _inner;

            internal SystemNoMatrix(System inner)
            {
                _inner = inner;
            }

            public System Matrix(Stream data)
            {
                _inner.ReadMatrix(data);
                return _inner;
            }

            public System Matrix(string data)
            {
                using (WebClient client = new WebClient())
                {
                    byte[] bytes = client.DownloadData(data);
                    using (MemoryStream stream = new MemoryStream(bytes))
                    {
                        return Matrix(stream);
                    }
                }
            }

            public System Matrix(string path)
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    return Matrix(stream);
                }
            }
        }
    }
}
