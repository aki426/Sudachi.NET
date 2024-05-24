using System.Text;

namespace DartsClone.NetTests
{
    public class DartsTestsUtility
    {
        public class TemporaryFolder : IDisposable
        {
            private readonly string _path;

            public TemporaryFolder()
            {
                _path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(_path);
            }

            public string NewFile()
            {
                return Path.Combine(_path, Guid.NewGuid().ToString());
            }

            public void Dispose()
            {
                Directory.Delete(_path, true);
            }
        }

        public class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] x, byte[] y)
            {
                if (x == null || y == null)
                {
                    return x == null && y == null;
                }
                if (x.Length != y.Length)
                {
                    return false;
                }
                for (int i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            public int GetHashCode(byte[] obj)
            {
                if (obj == null)
                {
                    return 0;
                }
                int result = 17;
                foreach (byte b in obj)
                {
                    result = result * 31 + b;
                }
                return result;
            }
        }

        public static HashSet<byte[]> GenerateInvalidKeys(int numKeys, ISet<byte[]> validKeys)
        {
            var keys = new HashSet<byte[]>(new ByteArrayEqualityComparer());
            var key = new StringBuilder();
            Random random = new Random();

            while (keys.Count < numKeys)
            {
                key.Clear();
                int length = random.Next(8) + 1;
                for (int i = 0; i < length; i++)
                {
                    key.Append((char)(1 + random.Next(65535)));
                }
                var k = Encoding.UTF8.GetBytes(key.ToString());
                if (!validKeys.Contains(k))
                {
                    keys.Add(k);
                }
            }
            return keys;
        }

        public static SortedSet<byte[]> GenerateValidKeys(int numKeys)
        {
            var keys = new SortedSet<byte[]>(new ByteArrayComparer());
            Random random = new Random();
            var key = new StringBuilder();
            while (keys.Count < numKeys)
            {
                key.Clear();
                int length = random.Next(8) + 1;
                for (int i = 0; i < length; i++)
                {
                    key.Append((char)(1 + random.Next(65535)));
                }
                keys.Add(Encoding.UTF8.GetBytes(key.ToString()));
            }
            return keys;
        }

        public class ByteArrayComparer : IComparer<byte[]>
        {
            public int Compare(byte[] x, byte[] y)
            {
                int n1 = x.Length;
                int n2 = y.Length;
                int min = Math.Min(n1, n2);
                for (int i = 0; i < min; i++)
                {
                    int c1 = x[i];
                    int c2 = y[i];
                    if (c1 != c2)
                    {
                        return c1 - c2;
                    }
                }
                return n1 - n2;
            }
        }
    }
}
