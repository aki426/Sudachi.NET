using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Sudachi.Net.Core.Dictionary
{
    /// <summary>
    /// A classifier of the categories of characters.
    /// </summary>
    public class CharacterCategory
    {
        private static readonly Regex PATTERN_SPACES = new Regex(@"\s+");
        private static readonly Regex PATTERN_EMPTY_OR_SPACES = new Regex(@"\s*");
        private static readonly Regex PATTERN_DOUBLE_PERIODS = new Regex(@"\.\.");
        private readonly List<Range> rangeList = new List<Range>();

        /// <summary>
        /// CategoryTypeに対して、その文字の範囲を定義する。
        /// </summary>
        /// <param name="Low"></param>
        /// <param name="High"></param>
        /// <param name="Categories"></param>
        public record Range(int Low, int High, IEnumerable<CategoryType> Categories)
        {
            /// <summary>
            /// コードポイントがRange内に含まれるかどうかを返す。
            /// </summary>
            /// <param name="codePoint">The code point.</param>
            /// <returns>含まれる場合True</returns>
            public bool Contains(int codePoint) => Low <= codePoint && codePoint <= High;

            /// <summary>
            /// text内の各文字についてCodepoint値を検証し、Range内であれば文字列長を、そうでなければそのIndexを返す。
            /// </summary>
            /// <param name="text">The text.</param>
            /// <returns>Range外のコードポイントの文字位置Index</returns>
            public int ContainingLength(string text)
            {
                // NOTE: サロゲート文字対応。char.IsSurrogatePair でサロゲートペアを判定し、2文字分進める
                for (int i = 0; i < text.Length; i += char.IsSurrogatePair(text, i) ? 2 : 1)
                {
                    // NOTE: char.ConvertToUtf32は、指定したインデックス位置の文字がサロゲートペアの場合は
                    // 2つ分のコードポイントを組み合わせて1つのUnicodeスカラー値（コードポイント値）を返します。
                    // そうでない場合は、その1文字のコードポイント値を返します。
                    // TODO: StringInfoを使うよりもパフォーマンスが良いらしいが定かではない。
                    int codePoint = char.ConvertToUtf32(text, i);
                    if (!this.Contains(codePoint))
                    {
                        return i;
                    }
                }
                return text.Length;
            }
        }

        /// <summary>
        /// Returns the set of the category types of the character (Unicode code point).
        /// </summary>
        /// <param name="codePoint">the code point value of the character</param>
        /// <returns>the set of the category types of the character. If nothing, return CategoryType.Default</returns>
        public IEnumerable<CategoryType> GetCategoryTypes(int codePoint)
        {
            // 与えられたコードポイントに対して逆引きで現在のCharacterCategoryに該当するCategoryTypeを収集する。
            HashSet<CategoryType> categories = new HashSet<CategoryType>();
            foreach (Range range in rangeList)
            {
                if (range.Contains(codePoint))
                {
                    categories.UnionWith(range.Categories);
                }
            }

            // codePointに対応するCategoryTypeが存在しない場合は、Defaultを追加する。
            if (categories.Count == 0)
            {
                categories.Add(CategoryType.Default);
            }

            return categories;
        }

        ///// <summary>
        ///// Reads the definitions of the character categories from the file which is
        ///// specified by <paramref name="charDef"/>. If <paramref name="charDef"/> is <see langword="null"/>, uses the
        ///// default definitions.
        ///// </summary>
        ///// <param name="charDef">the file of the definitions of character categories.</param>
        ///// <exception cref="IOException">if the definition file is not available.</exception>
        ///// <remarks>
        ///// The following is the format of definitions.
        ///// <code>
        ///// 0x0020 SPACE              # a white space
        ///// 0x0041..0x005A ALPHA      # Latin alphabets
        ///// 0x4E00 KANJINUMERIC KANJI # Kanji numeric and Kanji
        ///// </code>
        ///// <para>Lines that do not start with "0x" are ignored.</para>
        ///// </remarks>
        //[Obsolete("Use Load(Config.Resource) instead. Will be removed with 1.0 release.")]
        //public void ReadCharacterDefinition(string charDef)
        //{
        //    using Stream stream = charDef != null ? File.OpenRead(charDef) : typeof(CharacterCategory).Assembly.GetManifestResourceStream("char.def");
        //    ReadCharacterDefinition(stream);
        //}

        public void ReadCharacterDefinition(Stream stream)
        {
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string line;
            int lineNumber = 0;
            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;

                // コメント行、空行は無視
                if (line.StartsWith("#") || PATTERN_EMPTY_OR_SPACES.IsMatch(line))
                {
                    continue;
                }

                // 空白区切り文字列として分割
                string[] cols = PATTERN_SPACES.Split(line);
                if (cols.Length < 2)
                {
                    throw new ArgumentException($"invalid format at line {lineNumber}");
                }

                // 16進数の範囲指定がある行のみ処理
                if (cols[0].StartsWith("0x"))
                {
                    // cols[0] からLowとHighを取り出す。
                    string[] r = PATTERN_DOUBLE_PERIODS.Split(cols[0]);
                    int low = int.Parse(r[0], System.Globalization.NumberStyles.HexNumber);
                    int high = r.Length > 1 ? int.Parse(r[1], System.Globalization.NumberStyles.HexNumber) : low;

                    if (low > high)
                    {
                        throw new ArgumentException($"invalid range at line {lineNumber}");
                    }

                    // 定義読み込み。
                    HashSet<CategoryType> categories = new HashSet<CategoryType>();
                    for (int i = 1; i < cols.Length; i++)
                    {
                        // カラムに対してもコメントが有効
                        if (cols[i].Trim().StartsWith("#"))
                        {
                            break;
                        }

                        if (Enum.TryParse(cols[i], out CategoryType type))
                        {
                            categories.Add(type);
                        }
                        else
                        {
                            throw new ArgumentException($"{cols[i]} is invalid type at line {lineNumber}");
                        }
                    }

                    rangeList.Add(new Range(low, high, categories));
                }
            }
        }

        // TODO: Config実装後にコメントインする。
        //public static CharacterCategory Load(Config.Resource<CharacterCategory> resource)
        //{
        //    return resource.Consume(res =>
        //    {
        //        CharacterCategory result = new CharacterCategory();
        //        using Stream stream = res.AsStream();
        //        result.ReadCharacterDefinition(stream);
        //        return result;
        //    });
        //}

        public static CharacterCategory Load(Stream stream)
        {
            CharacterCategory result = new CharacterCategory();
            result.ReadCharacterDefinition(stream);
            return result;
        }
    }
}
