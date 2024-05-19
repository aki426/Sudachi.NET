namespace DartsClone.Net.Details
{
    public class KeySet
    {
        private byte[][] Keys { get; init; }
        private int[] Values { get; init; }

        public KeySet(byte[][] keys, int[] values)
        {
            Keys = keys;
            Values = values;
        }

        public int Size => Keys.Length;

        public byte[] GetKey(int keyId) => Keys[keyId];

        public byte GetKeyByte(int keyId, int byteId)
            => byteId >= Keys[keyId].Length ? (byte)0 : Keys[keyId][byteId];

        // NOTE: JAVA版の実装がnullチェックのみであるのに対して、C#版は要素数も判定条件に入れている点に注意。
        public bool HasValues => Values != null && Values.Length != 0;

        public int GetValue(int id) => HasValues ? Values[id] : id;
    }
}
