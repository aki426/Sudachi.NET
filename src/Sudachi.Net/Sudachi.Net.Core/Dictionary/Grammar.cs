using System.Collections.Generic;

namespace Sudachi.Net.Core.Dictionary
{
    public abstract record Grammar
    {
        /// <summary>
        /// Returns the number of types of part-of-speech.
        ///
        /// The IDs of part-of-speech are within the range of 0 to
        /// <code>PartOfSpeechSize - 1</code>.
        /// </summary>
        /// <returns>the number of types of part-of-speech</returns>
        public abstract int PartOfSpeechSize { get; }

        /// <summary>
        /// Returns the array of strings of part-of-speech name.
        ///
        /// The name is divided into layers.
        /// </summary>
        /// <param name="posId">the ID of the part-of-speech</param>
        /// <returns>the list of strings of part-of-speech name</returns>
        /// <exception cref="System.IndexOutOfRangeException">if <paramref name="posId"/> is out of the range</exception>
        public abstract POS GetPartOfSpeechString(short posId);

        /// <summary>
        /// Returns the the ID corresponding to the part-of-speech name.
        ///
        /// If there is not such the part-of-speech name, -1 is returned.
        /// </summary>
        /// <param name="pos">the list of string of part-of-speech name</param>
        /// <returns>the ID corresponding to the part-of-speech name, or -1 without corresponding one.</returns>
        public abstract short GetPartOfSpeechId(List<string> pos);

        /// <summary>
        /// Returns the cost of the specified connection.
        ///
        /// When the Id is out of the range, the behavior is undefined.
        /// </summary>
        /// <param name="left">the right-ID of the left node</param>
        /// <param name="right">the left-ID of the right node</param>
        /// <returns>the cost of the connection</returns>
        public abstract short GetConnectCost(short left, short right);

        /// <summary>
        /// Set the connection costs.
        ///
        /// When the Id is out of the range, the behavior is undefined.
        /// </summary>
        /// <param name="left">the right-ID of the left node</param>
        /// <param name="right">the left-ID of the right node</param>
        /// <param name="cost">the cost of the connection</param>
        public abstract void SetConnectCost(short left, short right, short cost);

        /// <summary>
        /// Returns the parameter of the beginning of sentence.
        ///
        /// The following are the parameters.
        ///
        /// <code>{ left-ID, rightID, cost }</code>
        /// </summary>
        /// <returns>the parameter of the beginning of sentence</returns>
        public abstract short[] BOSParameter { get; }

        /// <summary>
        /// Returns the parameter of the end of sentence.
        ///
        /// The following are the parameters.
        ///
        /// <code>{ left-ID, rightID, cost }</code>
        /// </summary>
        /// <returns>the parameter of the end of sentence</returns>
        public abstract short[] EOSParameter { get; }

        public abstract CharacterCategory CharacterCategory { get; set; }

        /// <summary>the cost of inhibited connections</summary>
        public static readonly short INHIBITED_CONNECTION = short.MaxValue;

        // TODO: Connectonクラスを実装したらコメントインする。
        //public virtual Connection GetConnection() => throw new NotSupportedException();

        public abstract bool IsValid();
    }
}
