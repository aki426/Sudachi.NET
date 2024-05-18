using System;
using System.Collections.Generic;
using System.Text;

namespace Sudachi.Net.Core.Dictionary
{
    /// <summary>
    /// Categories of characters.
    ///
    /// These categories are used in the <see cref="com.worksap.nlp.sudachi.OovProviderPlugin"/> and
    /// <see cref="com.worksap.nlp.sudachi.PathRewritePlugin"/>.
    ///
    /// <para>You can defined the range of each category in the file which specified
    /// "characterDefinitionFile" of the settings.</para>
    /// </summary>
    public enum CategoryType
    {
        /// <summary>The fall back category.</summary>
        Default = 1,

        /// <summary>White spaces.</summary>
        Space = 1 << 1,

        /// <summary>CJKV ideographic characters.</summary>
        Kanji = 1 << 2,

        /// <summary>Symbols.</summary>
        Symbol = 1 << 3,

        /// <summary>Numerical characters.</summary>
        Numeric = 1 << 4,

        /// <summary>Latin alphabets.</summary>
        Alpha = 1 << 5,

        /// <summary>Hiragana characters.</summary>
        Hiragana = 1 << 6,

        /// <summary>Katakana characters.</summary>
        Katakana = 1 << 7,

        /// <summary>Kanji numeric characters.</summary>
        KanjiNumeric = 1 << 8,

        /// <summary>Greek alphabets.</summary>
        Greek = 1 << 9,

        /// <summary>Cyrillic alphabets.</summary>
        Cyrillic = 1 << 10,

        /// <summary>User defined category.</summary>
        User1 = 1 << 11,

        /// <summary>User defined category.</summary>
        User2 = 1 << 12,

        /// <summary>User defined category.</summary>
        User3 = 1 << 13,

        /// <summary>User defined category.</summary>
        User4 = 1 << 14,

        /// <summary>Characters that cannot be the beginning of word</summary>
        NoOovBow = 1 << 15
    }

    /// <summary>
    /// CategoryType enum's extensions class.
    /// </summary>
    public static class CategoryTypeExtensions
    {
        /// <summary>
        /// Returns the integer ID number of the category.
        /// </summary>
        /// <param name="type">The CategoryType value.</param>
        /// <returns>The ID number of the category.</returns>
        public static int GetId(this CategoryType type) => (int)type;

        /// <summary>
        /// Returns the category to which the specified ID is mapped, or <see langword="null"/> if
        /// there is no associated category.
        /// </summary>
        /// <param name="id">The ID number of category.</param>
        /// <returns>
        /// The category to which the specified ID is mapped, or <see langword="null"/>
        /// if there is no associated category.
        /// </returns>
        public static CategoryType? GetType(int id)
        {
            foreach (CategoryType type in System.Enum.GetValues(typeof(CategoryType)))
            {
                if (type.GetId() == id)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
