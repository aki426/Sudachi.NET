using System;

namespace Sudachi.Net.Core.Dictionary
{
    /**
       * CRF weights compressed into 2D u16 matrix in MeCab manner
       */

    public sealed class Connection
    {
        private readonly short[] _matrix;
        public int LeftSize { get; }
        public int RightSize { get; }

        public Connection(short[] matrix, int leftSize, int rightSize)
        {
            _matrix = matrix;
            LeftSize = leftSize;
            RightSize = rightSize;
        }

        private int Index(int left, int right)
        {
            if (left >= LeftSize)
            {
                throw new ArgumentOutOfRangeException(nameof(left), $"left must be less than {LeftSize}");
            }
            if (right >= RightSize)
            {
                throw new ArgumentOutOfRangeException(nameof(right), $"right must be less than {RightSize}");
            }
            return right * LeftSize + left;
        }

        /**
         * @param left left connection index
         * @param right right connection index
         * @return connection weight in the matrix
         */

        public short GetCost(int left, int right)
        {
            return _matrix[Index(left, right)];
        }

        public void SetCost(int left, int right, short cost)
        {
            _matrix[Index(left, right)] = cost;
        }

        /**
         * @return a copy of itself with the buffer owned, instead of slice
         */

        public Connection OwnedCopy()
        {
            short[] copy = new short[_matrix.Length];
            Array.Copy(_matrix, copy, _matrix.Length);
            return new Connection(copy, LeftSize, RightSize);
        }

        public void Validate(int leftId)
        {
            if (_matrix == null)
            {
                // should never happen, but elides compiler checks
                throw new NullReferenceException("matrix");
            }

            if (leftId >= LeftSize)
            {
                // should never happen, but adds a compiler precondition to the inlined method
                throw new ArgumentOutOfRangeException(nameof(leftId),
                    $"leftId must be less than leftSize: ({leftId}, {LeftSize})");
            }
        }
    }
}
