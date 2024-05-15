using System;
using System.Collections.Generic;
using System.Text;

namespace Sudachi.Net.Core.Dictionary.Lexicon
{
    public interface ILexicon
    {
        IEnumerator<int[]> Lookup(byte[] text, int offset);

        int GetWordId(string headword, short posId, string readingForm);

        /// <summary>
        /// Returns the left-ID of the morpheme specified by the word ID.
        /// </summary>
        /// <param name="wordId">The word ID of the morpheme.</param>
        /// <returns>The left-ID of the morpheme.</returns>
        /// <remarks>
        /// When the word ID is out of range, the behavior is undefined.
        /// </remarks>
        short LeftId { get; }

        /// <summary>
        /// Returns the right-ID of the morpheme specified by the word ID.
        /// </summary>
        /// <param name="wordId">The word ID of the morpheme.</param>
        /// <returns>The right-ID of the morpheme.</returns>
        /// <remarks>
        /// When the word ID is out of range, the behavior is undefined.
        /// </remarks>
        short RightId { get; }

        /// <summary>
        /// Returns the word occurrence cost of the morpheme specified by the word ID.
        /// </summary>
        /// <param name="wordId">The word ID of the morpheme.</param>
        /// <returns>The word occurrence cost.</returns>
        /// <remarks>
        /// When the word ID is out of range, the behavior is undefined.
        /// </remarks>
        short Cost { get; }

        /// <summary>
        /// Returns the informations of the morpheme specified by the word ID.
        /// </summary>
        /// <param name="wordId">The word ID of the morpheme.</param>
        /// <returns>The informations of the morpheme.</returns>
        /// <remarks>
        /// When the word ID is out of range, the behavior is undefined.
        /// </remarks>
        WordInfo WordInfo { get; }

        ///// <summary>
        ///// Returns the ID of the dictionary containing the morpheme specified by the word ID.
        ///// If the morpheme is in the system dictionary, it returns 0.
        ///// </summary>
        ///// <param name="wordId">The word ID of the morpheme.</param>
        ///// <returns>The dictionary ID.</returns>
        //[Obsolete("Use WordId.Dic(int) instead.")]
        //int GetDictionaryId(int wordId) => WordId.Dic(wordId);

        /// <summary>
        /// Returns the number of morphemes in the dictionary.
        /// </summary>
        /// <returns>The number of morphemes.</returns>
        int Size { get; }
    }
}