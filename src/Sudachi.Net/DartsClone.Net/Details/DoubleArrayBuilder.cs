using System;
using System.Collections.Generic;

namespace DartsClone.Net.Details
{
    public class DoubleArrayBuilder
    {
        public static readonly int BLOCK_SIZE = 256;
        public static readonly int NUM_EXTRA_BLOCKS = 16;
        public static readonly int NUM_EXTRAS = BLOCK_SIZE * NUM_EXTRA_BLOCKS;
        public static readonly int UPPER_MASK = 0xFF << 21; //0x1FE_0000_0000
        public static readonly int LOWER_MASK = 0xFF;

        private class DoubleArrayBuilderExtraUnit
        {
            public int Prev;
            public int Next;
            public bool IsFixed;
            public bool IsUsed;
        }

        private Action<int, int> progressFunction;
        private List<DoubleArrayBuilderUnit> units = new();
        private DoubleArrayBuilderExtraUnit[]? extras;
        private List<byte> labels = new();
        private int[]? table;
        private int extrasHead;

        public DoubleArrayBuilder(Action<int, int> progressFunction)
        {
            this.progressFunction = progressFunction;
        }

        public static DoubleArray Build(byte[][] keys, int[] values, Action<int, int> progressFunction)
        {
            var keySet = new KeySet(keys, values);
            var builder = new DoubleArrayBuilder(progressFunction);
            builder.Build(keySet);

            return new DoubleArray(new Memory<byte>(builder.Copy()));
        }

        public void Build(KeySet keySet)
        {
            if (keySet.HasValues)
            {
                var dawgBuilder = new DAWGBuilder();
                BuildDAWG(keySet, dawgBuilder);
                BuildFromDAWG(dawgBuilder);
                dawgBuilder.Clear();
            }
            else
            {
                BuildFromKeySet(keySet);
            }
        }

        /// <summary>
        /// DoubleArrayBuilderUnitからbyte[]に変換する。
        /// NOTE: DoubleArrayBuilderUnitの内部表現はint型であるため、BitConverterを使ってbyte[]に変換する。
        /// TODO: DoubleArrayBuilderUnitのリスト（実体はintのリスト）->byte[]->Memory{byte}に変換する処理は、
        /// 直接DoubleArrayBuilderUnitのリスト（実体はintのリスト）->Memory{byte}に変換できそう。
        /// パフォーマンスの観点で必要そうなら今後実装する。
        /// </summary>
        /// <returns>An array of byte.</returns>
        public byte[] Copy()
        {
            var buffer = new byte[units.Count * 4];
            var index = 0;
            foreach (DoubleArrayBuilderUnit u in units)
            {
                BitConverter.GetBytes(u.Unit).CopyTo(buffer, index);
                index += 4;
            }
            return buffer;
        }

        public void Clear()
        {
            units = null;
            extras = null;
            labels = null;
            table = null;
        }

        private int NumBlocks() => units.Count / BLOCK_SIZE;

        private DoubleArrayBuilderExtraUnit Extras(int id) => extras[id % NUM_EXTRAS];

        private void BuildDAWG(KeySet keySet, DAWGBuilder dawgBuilder)
        {
            dawgBuilder.Init();

            for (int i = 0; i < keySet.Size; i++)
            {
                dawgBuilder.Insert(keySet.GetKey(i), keySet.GetValue(i));
                progressFunction?.Invoke(i + 1, keySet.Size + 1);
            }

            dawgBuilder.Finish();
        }

        private void BuildFromDAWG(DAWGBuilder dawg)
        {
            int numUnits = 1;
            while (numUnits < dawg.Size())
            {
                numUnits <<= 1;
            }

            units.Capacity = numUnits;
            table = new int[dawg.NumIntersections()];
            extras = new DoubleArrayBuilderExtraUnit[NUM_EXTRAS];

            for (int i = 0; i < extras.Length; i++)
            {
                extras[i] = new DoubleArrayBuilderExtraUnit();
            }

            ReserveId(0);
            Extras(0).IsUsed = true;
            units[0].SetOffset(1);
            units[0].SetLabel(0);

            if (dawg.Child(dawg.Root()) != 0)
            {
                BuildFromDAWG(dawg, dawg.Root(), 0);
            }

            FixAllBlocks();
            extras = null;
            labels.Clear(); // = null;
            table = null;
        }

        private void BuildFromDAWG(DAWGBuilder dawg, int dawgId, int dicId)
        {
            int dawgChildId = dawg.Child(dawgId);
            int offset;
            if (dawg.IsIntersection(dawgChildId))
            {
                offset = dawg.IntersectionId(dawgChildId);
                int storedOffset = table[offset];
                if (storedOffset != 0)
                {
                    offset = storedOffset ^ dicId;
                    if ((offset & UPPER_MASK) == 0 || (offset & LOWER_MASK) == 0)
                    {
                        if (dawg.IsLeaf(dawgChildId))
                        {
                            units[dicId].SetHasLeaf(true);
                        }
                        units[dicId].SetOffset(offset);
                        return;
                    }
                }
            }

            offset = ArrangeFromDAWG(dawg, dawgId, dicId);
            if (dawg.IsIntersection(dawgChildId))
            {
                table[dawg.IntersectionId(dawgChildId)] = offset;
            }

            while (true)
            {
                byte childLabel = dawg.Label(dawgChildId);
                int dicChildId = offset ^ childLabel;
                if (childLabel != 0)
                {
                    BuildFromDAWG(dawg, dawgChildId, dicChildId);
                }

                dawgChildId = dawg.Sibling(dawgChildId);
                if (dawgChildId == 0)
                    break;
            }
        }

        private int ArrangeFromDAWG(DAWGBuilder dawg, int dawgId, int dicId)
        {
            labels.Clear();

            int dawgChildId = dawg.Child(dawgId);
            for (; dawgChildId != 0; dawgChildId = dawg.Sibling(dawgChildId))
            {
                labels.Add(dawg.Label(dawgChildId));
            }

            int offset = FindValidOffset(dicId);
            units[dicId].SetOffset(dicId ^ offset);

            dawgChildId = dawg.Child(dawgId);

            foreach (byte l in labels)
            {
                int dicChildId = offset ^ l;
                ReserveId(dicChildId);

                if (dawg.IsLeaf(dawgChildId))
                {
                    units[dicId].SetHasLeaf(true);
                    units[dicChildId].SetValue(dawg.Value(dawgChildId));
                }
                else
                {
                    units[dicChildId].SetLabel(l);
                }
                dawgChildId = dawg.Sibling(dawgChildId);
            }

            Extras(offset).IsUsed = true;
            return offset;
        }

        private void BuildFromKeySet(KeySet keySet)
        {
            int numUnits = 1;
            while (numUnits < keySet.Size)
            {
                numUnits <<= 1;
            }

            units.Capacity = numUnits;
            extras = new DoubleArrayBuilderExtraUnit[NUM_EXTRAS];

            for (int i = 0; i < extras.Length; i++)
            {
                extras[i] = new DoubleArrayBuilderExtraUnit();
            }

            ReserveId(0);
            Extras(0).IsUsed = true;
            units[0].SetOffset(1);
            units[0].SetLabel(0);

            if (keySet.Size > 0)
            {
                BuildFromKeySet(keySet, 0, keySet.Size, 0, 0);
            }

            FixAllBlocks();
            extras = null;
            labels = null;
        }

        private void BuildFromKeySet(KeySet keySet, int begin, int end, int depth, int dicId)
        {
            int offset = ArrangeFromKeySet(keySet, begin, end, depth, dicId);

            while (begin < end && keySet.GetKeyByte(begin, depth) == 0)
            {
                begin++;
            }

            if (begin == end)
                return;

            int lastBegin = begin;
            byte lastLabel = keySet.GetKeyByte(begin, depth);

            while (true)
            {
                begin++;
                if (begin >= end)
                {
                    BuildFromKeySet(keySet, lastBegin, end, depth + 1, offset ^ lastLabel);
                    return;
                }

                byte label = keySet.GetKeyByte(begin, depth);
                if (label != lastLabel)
                {
                    BuildFromKeySet(keySet, lastBegin, begin, depth + 1, offset ^ lastLabel);
                    lastBegin = begin;
                    lastLabel = keySet.GetKeyByte(begin, depth);
                }
            }
        }

        private int ArrangeFromKeySet(KeySet keySet, int begin, int end, int depth, int dicId)
        {
            labels.Clear();
            int value = -1;

            for (int i = begin; i < end; i++)
            {
                byte label = keySet.GetKeyByte(i, depth);
                if (label == 0)
                {
                    if (depth < keySet.GetKey(i).Length)
                    {
                        throw new ArgumentException("invalid null character");
                    }

                    if (keySet.GetValue(i) < 0)
                    {
                        throw new ArgumentException("negative value");
                    }

                    if (value == -1)
                    {
                        value = keySet.GetValue(i);
                    }

                    progressFunction?.Invoke(i + 1, keySet.Size + 1);
                }

                if (labels.Count == 0)
                {
                    labels.Add(label);
                }
                else if (label != labels[labels.Count - 1])
                {
                    if (label < labels[labels.Count - 1])
                    {
                        throw new ArgumentException("wrong key order");
                    }
                    labels.Add(label);
                }
            }

            int offset = FindValidOffset(dicId);
            units[dicId].SetOffset(dicId ^ offset);

            foreach (byte l in labels)
            {
                int dicChildId = offset ^ l;
                ReserveId(dicChildId);
                if (l == 0)
                {
                    units[dicId].SetHasLeaf(true);
                    units[dicChildId].SetValue(value);
                }
                else
                {
                    units[dicChildId].SetLabel(l);
                }
            }

            Extras(offset).IsUsed = true;
            return offset;
        }

        private int FindValidOffset(int id)
        {
            if (extrasHead >= units.Count)
            {
                return units.Count | (id & LOWER_MASK);
            }
            else
            {
                int unfixedId = extrasHead;
                do
                {
                    int offset = unfixedId ^ labels[0];
                    if (IsValidOffset(id, offset))
                    {
                        return offset;
                    }
                    unfixedId = Extras(unfixedId).Next;
                } while (unfixedId != extrasHead);

                return units.Count | (id & LOWER_MASK);
            }
        }

        private bool IsValidOffset(int id, int offset)
        {
            if (Extras(offset).IsUsed)
            {
                return false;
            }

            int relOffset = id ^ offset;
            if ((relOffset & LOWER_MASK) != 0 && (relOffset & UPPER_MASK) != 0)
            {
                return false;
            }

            for (int i = 1; i < labels.Count; i++)
            {
                if (Extras(offset ^ labels[i]).IsFixed)
                {
                    return false;
                }
            }

            return true;
        }

        private void ReserveId(int id)
        {
            if (id >= units.Count)
            {
                ExpandUnits();
            }

            if (id == extrasHead)
            {
                extrasHead = Extras(id).Next;
                if (extrasHead == id)
                {
                    extrasHead = units.Count;
                }
            }

            Extras(Extras(id).Prev).Next = Extras(id).Next;
            Extras(Extras(id).Next).Prev = Extras(id).Prev;
            Extras(id).IsFixed = true;
        }

        private void ExpandUnits()
        {
            int srcNumUnits = units.Count;
            int srcNumBlocks = NumBlocks();
            int destNumUnits = srcNumUnits + BLOCK_SIZE;
            int destNumBlocks = srcNumBlocks + 1;

            if (destNumBlocks > NUM_EXTRA_BLOCKS)
            {
                FixBlock(srcNumBlocks - NUM_EXTRA_BLOCKS);
            }

            for (int i = srcNumUnits; i < destNumUnits; i++)
            {
                units.Add(new DoubleArrayBuilderUnit());
            }

            if (destNumBlocks > NUM_EXTRA_BLOCKS)
            {
                for (int i = srcNumUnits; i < destNumUnits; i++)
                {
                    Extras(i).IsUsed = false;
                    Extras(i).IsFixed = false;
                }
            }

            for (int i = srcNumUnits + 1; i < destNumUnits; i++)
            {
                Extras(i - 1).Next = i;
                Extras(i).Prev = i - 1;
            }

            Extras(srcNumUnits).Prev = destNumUnits - 1;
            Extras(destNumUnits - 1).Next = srcNumUnits;
            Extras(srcNumUnits).Prev = Extras(extrasHead).Prev;
            Extras(destNumUnits - 1).Next = extrasHead;
            Extras(Extras(extrasHead).Prev).Next = srcNumUnits;
            Extras(extrasHead).Prev = destNumUnits - 1;
        }

        private void FixAllBlocks()
        {
            int begin = 0;
            if (NumBlocks() > NUM_EXTRA_BLOCKS)
            {
                begin = NumBlocks() - NUM_EXTRA_BLOCKS;
            }

            int end = NumBlocks();
            for (int blockId = begin; blockId != end; blockId++)
            {
                FixBlock(blockId);
            }
        }

        private void FixBlock(int blockId)
        {
            int begin = blockId * BLOCK_SIZE;
            int end = begin + BLOCK_SIZE;
            int unusedOffset = 0;

            for (int id = begin; id != end; id++)
            {
                if (!Extras(id).IsUsed)
                {
                    unusedOffset = id;
                    break;
                }
            }

            for (int id = begin; id != end; id++)
            {
                if (!Extras(id).IsFixed)
                {
                    ReserveId(id);
                    units[id].SetLabel((byte)(id ^ unusedOffset));
                }
            }
        }
    }
}
