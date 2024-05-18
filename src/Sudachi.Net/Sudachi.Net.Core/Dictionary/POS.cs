using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudachi.Net.Core.Dictionary
{
    /// <summary>
    /// Part-of-Speech
    /// <p>
    /// Sudachi POS are 6-component and consist of:
    /// 4 layers of POS tags, conjugation type, conjugation form.
    /// </summary>
    public sealed record POS // : IList<string>
    {
        public static readonly int DEPTH = 6;
        public static readonly int MAX_COMPONENT_LENGTH = 127;

        private string[] elems { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="POS"/> class.
        /// </summary>
        /// <param name="elems">non-null string array of exactly six elements</param>
        public POS(params string[] elems)
        {
            if (elems == null)
            {
                throw new ArgumentNullException("pos must not be null");
            }
            if (elems.Length != DEPTH)
            {
                throw new ArgumentException(
                    $"pos must have exactly 6 elements, was {elems.Length}: {string.Join(",", elems)}");
            }
            foreach (string e in elems)
            {
                if (e == null)
                {
                    throw new ArgumentException("POS components can't be null");
                }

                if (e.Length > MAX_COMPONENT_LENGTH)
                {
                    throw new ArgumentException(
                        $"POS component had length ({e.Length}) > {MAX_COMPONENT_LENGTH}: {e}");
                }
            }
            this.elems = elems;
        }

        /// <summary>
        /// Creates new POS instance from elements. Elements must be 6-length string list.
        /// </summary>
        /// <param name="elems">POS object elements</param>
        public POS(IList<string> elems) : this(elems.ToArray()) { }

        /// <summary>
        /// Indexer
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public new string this[int i] => elems[i];

        /// <summary>Count</summary>
        public new int Count => elems.Length;

        //public override bool Equals(object o)
        //{
        //    if (this == o) return true;
        //    return o is POS pos && Equals(pos.elems, elems);
        //}

        //public override int GetHashCode()
        //{
        //    int result = 0xfeed;
        //    result = 31 * result + (elems != null ? elems.GetHashCode() : 0);
        //    return result;
        //}

        //public override string ToString()
        //{
        //    return string.Join(",", elems);
        //}
    }
}
