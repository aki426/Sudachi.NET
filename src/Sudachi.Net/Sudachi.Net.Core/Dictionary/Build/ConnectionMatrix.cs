using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Sudachi.Net.Core.Dictionary.Build
{
    public class ConnectionMatrix : IWriteDictionary
    {
        private short _numLeft;
        private short _numRight;
        private byte[] _compiled;
        private static readonly Regex Whitespace = new Regex(@"\s+");
        private static readonly Regex OptWhitespace = new Regex(@"\s*");

        /**
         * @return compiled binary matrix representation with header
         */
        public byte[] Compiled => _compiled;

        /**
         * @return compiled binary matrix without header
         */

        public byte[] CompiledNoHeader
        {
            get
            {
                byte[] data = new byte[_compiled.Length - 4];
                Array.Copy(_compiled, 4, data, 0, data.Length);
                return data;
            }
        }

        /**
         * Read connection matrix in text format into the binary representation
         *
         * @param data input stream containing
         * @return number read of matrix values
         * @throws IOException when IO fails
         */

        public long ReadEntries(Stream data)
        {
            using var reader = new StreamReader(data, Encoding.UTF8);

            string header = reader.ReadLine();
            if (header == null)
            {
                throw new ArgumentException($"invalid format at line {reader.EndOfStream}");
            }

            string[] lr = Whitespace.Split(header, 2);
            if (lr.Length != 2)
            {
                throw new ArgumentException($"invalid header {header}, expected two 16-bit integers");
            }

            try
            {
                _numLeft = short.Parse(lr[0]);
                _numRight = short.Parse(lr[1]);
            }
            catch (FormatException)
            {
                throw new ArgumentException($"invalid header {header}, expected two 16-bit integers");
            }

            byte[] buffer = new byte[2 * _numLeft * _numRight + 4];

            Connection conn = new Connection(buffer, _numLeft, _numRight);

            long numLines = 0;

            while (true)
            {
                string line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }
                if (OptWhitespace.IsMatch(line))
                {
                    continue;
                }
                string[] cols = Whitespace.Split(line);
                if (cols.Length < 3)
                {
                    throw new InputFileException(reader.EndOfStream, line,
                        new ArgumentException("not enough entries"));
                }

                try
                {
                    short left = short.Parse(cols[0]);
                    short right = short.Parse(cols[1]);
                    short cost = short.Parse(cols[2]);
                    conn.SetCost(left, right, cost);
                }
                catch (FormatException e)
                {
                    throw new InputFileException(reader.EndOfStream, "", e);
                }

                numLines++;
            }
            _compiled = buffer;
            return numLines;
        }

        public void MakeEmpty()
        {
            byte[] data = new byte[4];
            data[0] = 0;
            data[1] = 0;
            data[2] = 0;
            data[3] = 0;
            _compiled = data;
        }

        public void WriteTo(ModelOutput output)
        {
            output.Write(_compiled, 0, _compiled.Length);
        }

        public short NumLeft => _numLeft;
        public short NumRight => _numRight;
    }
}
