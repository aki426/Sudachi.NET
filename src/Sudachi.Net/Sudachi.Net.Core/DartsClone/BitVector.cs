using System;
using System.Collections.Generic;
using System.Text;

namespace Sudachi.Net.Core.DartsClone
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

        public bool Get(int id)
        {
            return ((uint)units[id / 32] >> id % 32 & 1) == 1;
        }

        public int Rank(int id)
        {
            int unitId = id / 32;
            // JAVA版では-1 >>> を使用していたが、C#10では>>>は存在しないため、代わりに0xFFFFFFFFを使用している。
            return ranks[unitId] + PopCount((int)(units[unitId] & (0xFFFFFFFF >> (32 - (id % 32) - 1))));
        }

        public void Set(int id, bool bit)
        {
            if (bit)
            {
                units[id / 32] |= 1 << (id % 32);
            }
            else
            {
                units[id / 32] &= ~(1 << (id % 32));
            }
        }

        public bool IsEmpty() => units.Count == 0;

        public void Append()
        {
            if (Size % 32 == 0)
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
            // JAVA版の>>>演算子を>>へ置き換えたのでint -> uint -> intのキャストで対応。
            unit = (int)((uint)(unit & -1431655766) >> 1) + (unit & 1431655765);
            unit = (int)((uint)(unit & -858993460) >> 2) + (unit & 858993459);
            unit = (int)((uint)unit >> 4) + (unit & 252645135);
            unit += (int)(uint)unit >> 8;
            unit += (int)(uint)unit >> 16;

            return unit & 255;
        }
    }
}
