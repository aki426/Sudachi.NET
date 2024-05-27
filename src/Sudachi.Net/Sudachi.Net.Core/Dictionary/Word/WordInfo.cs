using System;

namespace Sudachi.Net.Core.Dictionary.Word
{
    /// <summary>
    /// Informations of the morpheme.
    /// This class has the informations which are not used in the graph calculation.
    /// </summary>
    /// <param name="Surface">the text of the morpheme.</param>
    /// <param name="HeadwordLength">the length of the text in internal use unit.
    /// This length is used to place a node in the Lattice, does not equals Surface.Length.</param>
    /// <param name="POSId">the part-of-speech ID of the morpheme.
    /// The strings of part-of-speech name can be gotten with Grammar#getPartOfSpeechString</param>
    /// <param name="NormalizedForm">the normalized form of the morpheme.</param>
    /// <param name="DictionaryFormWordId">the word ID of the dictionary form of the morpheme.
    /// The information of the dictionary form can be gotten with Lexicon#getWordInfo.</param>
    /// <param name="DictionaryForm">the dictionary form of the morpheme.</param>
    /// <param name="ReadingForm">the reading form of the morpheme.</param>
    /// <param name="AUnitSplit">the array of word IDs which the morpheme is compounded of in A mode.</param>
    /// <param name="BUnitSplit">the array of word IDs which the morpheme is compounded of in B mode.</param>
    /// <param name="WordStructure">the array of the morphemes which the morpheme is compounded of.</param>
    /// <param name="SynonymGroupIds">the array of the synonym groups.</param>
    public record WordInfo(
        string Surface,
        short HeadwordLength,
        short POSId,
        string NormalizedForm,
        int DictionaryFormWordId,
        string DictionaryForm,
        string ReadingForm,
        int[] AUnitSplit,
        int[] BUnitSplit,
        int[] WordStructure,
        int[] SynonymGroupIds)
    {
        /// <summary>
        /// Allocates informations of morpheme not in the lexicons.
        /// </summary>
        /// <param name="surface">the text of the morpheme</param>
        /// <param name="headwordLength">the length of the morpheme</param>
        /// <param name="posId">the ID of the part-of-speech of the morpheme</param>
        /// <param name="normalizedForm">the normalized form of the morpheme</param>
        /// <param name="dictionaryForm">the dictionary form of the morpheme</param>
        /// <param name="readingForm">the reading form of the morpheme</param>
        public WordInfo(
            string surface,
            short headwordLength,
            short posId,
            string normalizedForm,
            string dictionaryForm,
            string readingForm) :
            this(
                surface,
                headwordLength,
                posId,
                normalizedForm,
                -1,
                dictionaryForm,
                readingForm,
                Array.Empty<int>(),
                Array.Empty<int>(),
                Array.Empty<int>(),
                Array.Empty<int>())
        { }
    }
}
