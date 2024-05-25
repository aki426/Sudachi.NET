using System.Collections.Generic;

namespace DartsClone.Net.Details
{
    public class BitVector
    {
        private static readonly int UNIT_SIZE = 32;
        private List<int> units = new List<int>();
        private int[]? ranks;
        public int NumOnes { get; private set; }

        public int Size { get; private set; }

        public BitVector()
        { }

        public bool Get(int id) => (units[id / UNIT_SIZE] >>> (id % UNIT_SIZE) & 1) == 1;

        public int Rank(int id)
        {
            int unitId = id / UNIT_SIZE;
            return ranks[unitId] + PopCount(units[unitId] & (~0 >>> (UNIT_SIZE - (id % UNIT_SIZE) - 1)));
        }

        public void Set(int id, bool bit)
        {
            if (bit)
            {
                units[id / UNIT_SIZE] |= 1 << (id % UNIT_SIZE);
            }
            else
            {
                units[id / UNIT_SIZE] &= ~(1 << (id % UNIT_SIZE));
            }
        }

        public bool IsEmpty() => units.Count == 0;

        public void Append()
        {
            if (Size % UNIT_SIZE == 0)
            {
                units.Add(0);
            }

            Size++;
        }

        public void Build()
        {
            ranks = new int[units.Count];
            this.NumOnes = 0;

            for (int i = 0; i < units.Count; i++)
            {
                ranks[i] = NumOnes;
                this.NumOnes += PopCount(units[i]);
            }
        }

        public void Clear()
        {
            units.Clear();
            ranks = null;
        }

        private int PopCount(int unit)
        {
            unit = (int)(((unit & 0xAAAAAAAA) >>> 1) + (unit & 0x55555555));
            unit = (int)(((unit & 0xCCCCCCCC) >>> 2) + (unit & 0x33333333));
            unit = ((unit >>> 4) + unit) & 0x0F0F0F0F;
            unit += unit >>> 8;
            unit += unit >>> 16;
            return unit & 0xFF;
        }
    }
}
