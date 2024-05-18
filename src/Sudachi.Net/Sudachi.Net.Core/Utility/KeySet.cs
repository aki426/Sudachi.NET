using System.Collections.Immutable;
using System.Linq;

namespace Sudachi.Net.Core.Utility
{
    public record KeySet
    {
        // ImmutableListとImmutableArrayには効率の差がある。
        // TODO: 実際の用途とパフォーマンスを見て判断する。
        // https://qiita.com/Tokeiya/items/f05163d83cc447f58460

        private ImmutableArray<ImmutableArray<byte>> Keys { get; init; }
        private ImmutableArray<int> Values { get; init; }

        public KeySet(byte[][] keys, int[] values)
        {
            Keys = keys.Select(k => ImmutableArray.Create(k)).ToImmutableArray();
            Values = values.ToImmutableArray();
        }

        public int Size => Keys.Length;

        public ImmutableArray<byte> GetKey(int keyId) => Keys[keyId];

        public byte GetKeyByte(int keyId, int byteId)
            => byteId >= Keys[keyId].Length ? (byte)0 : Keys[keyId][byteId];

        // NOTE: JAVA版の実装がnullチェックのみであるのに対して、C#版は要素数も判定条件に入れている点に注意。
        public bool HasValues => !Values.IsDefaultOrEmpty;

        public int GetValue(int id) => HasValues ? Values[id] : id;
    }
}
