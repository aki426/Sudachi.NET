using System;
using System.Collections.Generic;

namespace DartsClone.Net.Details
{
    internal class DAWGBuilder
    {
        private class Node
        {
            public int Child { get; set; }
            public int sibling;
            public byte label;
            public bool isState;
            public bool hasSibling;

            public void Reset()
            {
                Child = 0;
                sibling = 0;
                label = 0;
                isState = false;
                hasSibling = false;
            }

            //public int GetValue() => Child;

            //public void SetValue(int value) => Child = value;

            public int Unit()
            {
                if (label == 0)
                {
                    return (Child << 1) | (hasSibling ? 1 : 0);
                }
                return (Child << 2) | (isState ? 2 : 0) | (hasSibling ? 1 : 0);
            }
        }

        private class Unit
        {
            public int unit;

            public int Child() => unit >>> 2;

            public bool HasSibling() => (unit & 1) == 1;

            public int Value() => unit >>> 1;

            public bool IsState() => (unit & 2) == 2;
        }

        private const int INITIAL_TABLE_SIZE = 1 << 10; // 1024

        private List<Node> nodes = new List<Node>();
        private List<Unit> units = new List<Unit>();
        private List<byte> labels = new List<byte>();
        private BitVector isIntersections = new BitVector();
        private List<int> table = new List<int>();
        private List<int> nodeStack = new List<int>();
        private List<int> recycleBin = new List<int>();
        private int numStates;

        public int Root() => 0;

        public int Child(int id) => units[id].Child();

        public int Sibling(int id) => (units[id].HasSibling()) ? (id + 1) : 0;

        public int Value(int id) => units[id].Value();

        public bool IsLeaf(int id) => Label(id) == 0;

        public byte Label(int id) => labels[id];

        public bool IsIntersection(int id) => isIntersections.Get(id);

        public int IntersectionId(int id) => isIntersections.Rank(id) - 1;

        public int NumIntersections() => isIntersections.NumOnes;

        public int Size() => units.Count;

        public void Init()
        {
            table.Capacity = INITIAL_TABLE_SIZE;
            for (int i = 0; i < INITIAL_TABLE_SIZE; i++)
            {
                table.Add(0);
            }

            AppendNode();
            AppendUnit();

            numStates = 1;

            nodes[0].label = 0xFF;
            nodeStack.Add(0);
        }

        public void Finish()
        {
            Flush(0);

            units[0].unit = nodes[0].Unit();
            labels[0] = nodes[0].label;

            nodes = null;
            table = null;
            nodeStack = null;
            recycleBin = null;

            isIntersections.Build();
        }

        public void Insert(byte[] key, int value)
        {
            if (value < 0)
            {
                throw new ArgumentException("negative value");
            }
            if (key.Length == 0)
            {
                throw new ArgumentException("zero-length key");
            }

            int id = 0;
            int keyPos = 0;

            for (; keyPos <= key.Length; keyPos++)
            {
                int childId = nodes[id].Child;
                if (childId == 0)
                {
                    break;
                }

                byte keyLabel = (keyPos < key.Length) ? key[keyPos] : (byte)0;
                if (keyPos < key.Length && keyLabel == 0)
                {
                    throw new ArgumentException("invalid null character");
                }

                byte unitLabel = nodes[childId].label;
                if (keyLabel < unitLabel)
                {
                    throw new ArgumentException("wrong key order");
                }
                else if (keyLabel > unitLabel)
                {
                    nodes[childId].hasSibling = true;
                    Flush(childId);
                    break;
                }
                id = childId;
            }

            if (keyPos > key.Length)
            {
                return;
            }

            for (; keyPos <= key.Length; keyPos++)
            {
                byte keyLabel = (keyPos < key.Length) ? key[keyPos] : (byte)0;
                int childId = AppendNode();

                if (nodes[id].Child == 0)
                {
                    nodes[childId].isState = true;
                }
                nodes[childId].sibling = nodes[id].Child;
                nodes[childId].label = keyLabel;
                nodes[id].Child = childId;
                nodeStack.Add(childId);

                id = childId;
            }
            nodes[id].Child = value;
        }

        public void Clear()
        {
            nodes?.Clear(); // = null;
            units?.Clear(); // = null;
            labels?.Clear(); // = null;
            isIntersections?.Clear(); // = null;
            table?.Clear(); // = null;
            nodeStack?.Clear();  // = null;
            recycleBin?.Clear(); // = null;
        }

        private void Flush(int id)
        {
            while (StackTop(nodeStack) != id)
            {
                int nodeId = StackTop(nodeStack);
                StackPop(nodeStack);

                if (numStates >= table.Count - (table.Count >> 2))
                {
                    ExpandTable();
                }

                int numSiblings = 0;
                for (int i = nodeId; i != 0; i = nodes[i].sibling)
                {
                    numSiblings++;
                }

                int[] findResult = FindNode(nodeId);
                int matchId = findResult[0];
                int hashId = findResult[1];

                if (matchId != 0)
                {
                    isIntersections.Set(matchId, true);
                }
                else
                {
                    int unitId = 0;
                    for (int i = 0; i < numSiblings; i++)
                    {
                        unitId = AppendUnit();
                    }
                    for (int i = nodeId; i != 0; i = nodes[i].sibling)
                    {
                        units[unitId].unit = nodes[i].Unit();
                        labels[unitId] = nodes[i].label;
                        unitId--;
                    }
                    matchId = unitId + 1;
                    table[hashId] = matchId;
                    numStates++;
                }

                for (int i = nodeId, next; i != 0; i = next)
                {
                    next = nodes[i].sibling;
                    FreeNode(i);
                }

                nodes[StackTop(nodeStack)].Child = matchId;
            }
            StackPop(nodeStack);
        }

        private void ExpandTable()
        {
            int tableSize = table.Count << 1;
            table.Clear();
            table.Capacity = tableSize;
            for (int i = 0; i < tableSize; i++)
            {
                table.Add(0);
            }

            for (int id = 1; id < units.Count; id++)
            {
                if (labels[id] == 0 || units[id].IsState())
                {
                    int[] findResult = FindUnit(id);
                    int hashId = findResult[1];
                    table[hashId] = id;
                }
            }
        }

        private int[] FindUnit(int id)
        {
            int[] result = new int[2];
            int hashId = HashUnit(id) % table.Count;
            for (; ; hashId = (hashId + 1) % table.Count)
            {
                int unitId = table[hashId];
                if (unitId == 0)
                {
                    break;
                }
            }
            result[1] = hashId;
            return result;
        }

        private int[] FindNode(int nodeId)
        {
            int[] result = new int[2];
            int hashId = HashNode(nodeId) % table.Count;
            for (; ; hashId = (hashId + 1) % table.Count)
            {
                int unitId = table[hashId];
                if (unitId == 0)
                {
                    break;
                }

                if (AreEqual(nodeId, unitId))
                {
                    result[0] = unitId;
                    result[1] = hashId;
                    return result;
                }
            }
            result[1] = hashId;
            return result;
        }

        private bool AreEqual(int nodeId, int unitId)
        {
            for (int i = nodes[nodeId].sibling; i != 0; i = nodes[i].sibling)
            {
                if (!units[unitId].HasSibling())
                {
                    return false;
                }
                unitId++;
            }
            if (units[unitId].HasSibling())
            {
                return false;
            }

            for (int i = nodeId; i != 0; i = nodes[i].sibling, unitId--)
            {
                if (nodes[i].Unit() != units[unitId].unit || nodes[i].label != labels[unitId])
                {
                    return false;
                }
            }
            return true;
        }

        private int HashUnit(int id)
        {
            int hashValue = 0;
            for (; id != 0; id++)
            {
                int unit = units[id].unit;
                byte label = labels[id];
                hashValue ^= Hash((label << 24) ^ unit);

                if (!units[id].HasSibling())
                {
                    break;
                }
            }
            return hashValue;
        }

        private int HashNode(int id)
        {
            int hashValue = 0;
            for (; id != 0; id = nodes[id].sibling)
            {
                int unit = nodes[id].Unit();
                byte label = nodes[id].label;
                hashValue ^= Hash((label << 24) ^ unit);
            }
            return hashValue;
        }

        private int AppendUnit()
        {
            isIntersections.Append();
            units.Add(new Unit());
            labels.Add(0);

            return isIntersections.Size - 1;
        }

        private int AppendNode()
        {
            int id;
            if (recycleBin.Count == 0)
            {
                id = nodes.Count;
                nodes.Add(new Node());
            }
            else
            {
                id = StackTop(recycleBin);
                nodes[id].Reset();
                StackPop(recycleBin);
            }
            return id;
        }

        private void FreeNode(int id) => recycleBin.Add(id);

        private static int Hash(int key)
        {
            key = ~key + (key << 15); // key = (key << 15) - key - 1;
            key = key ^ (key >> 12);
            key = key + (key << 2);
            key = key ^ (key >> 4);
            key = key * 2057; // key = (key + (key << 3)) + (key << 11);
            key = key ^ (key >> 16);
            return key;
        }

        private static T StackTop<T>(List<T> stack) => stack[stack.Count - 1];

        private static void StackPop<T>(List<T> stack) => stack.RemoveAt(stack.Count - 1);
    }
}
