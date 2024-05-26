using System;
using System.Collections.Generic;

namespace Sudachi.Net.Core.Dictionary
{
    /**
         * The parameters and grammatical information.
         */

    public interface IGrammar
    {
        /**
         * Returns the number of types of part-of-speech.
         *
         * The IDs of part-of-speech are within the range of 0 to
         * {@code GetPartOfSpeechSize() - 1}.
         *
         * @return the number of types of part-of-speech
         */
        int PartOfSpeechSize { get; }

        /**
         * Returns the array of strings of part-of-speech name.
         *
         * The name is divided into layers.
         *
         * @param posId the ID of the part-of-speech
         * @return the list of strings of part-of-speech name
         * @throws IndexOutOfBoundsExcepti         */

        POS GetPartOfSpeechString(short posId);

        /**
         * Returns the         *
         * <p>
         * If there is not such the part-of-speech name, -1 is returned.
         *
         * @param pos the list of string of part-of-speech name
         * @return the ID corresponding to the part-of-speech name, or -1 without         *         corresponding one.
         */

        short GetPartOfSpeechId(IList<string> pos);

        /**
         * Returns the cost of the specified         *
         * <p>
         * When the Id is out of the range, the behavior is undefined.
         *
         * @param left the right-ID of the left node
         * @param right the left-ID of the right node
         * @return the cost of the connection
         */

        short GetConnectCost(short left, short right);

        /**
         * Set the connection costs.
         *
         * <p>
         * When the Id is out of the range, the behavior is undefined.
         *
         * @param left the right-ID of the left node
         * @param right the left-ID of the right node
         * @param cost the cost of the connection
         */

        void SetConnectCost(short left, short right, short cost);

        /**
         * Returns the parameter of the beginning of sentence.
         *
         * <p>
         * The following are the parameters.
         *
         * <pre>
         * {@code { left-ID, rightID, cost } }
         * </pre>
         *
         * @return the parameter of the beginning of sentence
         */

        short[] GetBOSParameter();

        /**
         * Returns the parameter of the end of sentence.
         *
         * <p>
         * The following are the parameters.
         *
         * <pre>
         * {@code { left-ID, rightID, cost } }
         * </pre>
         *
         * @return the parameter of the end of sentence
         */

        short[] GetEOSParameter();

        CharacterCategory CharacterCategory { get; set; }

        /** the cost of inhibited connections */
        short InhibitedConnection { get; }

        Connection Connection { get; }

        bool IsValid();
    }

    //public static class GrammarExtensions
    //{
    //    public static Connection GetConnection(this IGrammar grammar)
    //    {
    //        throw new NotSupportedException();
    //    }
    //}
}
