using System;
using System.Text.RegularExpressions;

namespace Sudachi.Net.Core.SentDetect
{
    /// <summary>A sentence boundary detector.</summary>
    public class SentenceDetector
    {
        /// <summary>A checher for words that cross boundaries.</summary>
        public interface INonBreakChecker
        {
            /// <summary>
            /// Returns whether there is a word that crosses the boundary.
            /// </summary>
            /// <param name="eos">the index of the detected boundary.</param>
            /// <returns>if, and only if there is a word that crosses the boundary</returns>
            bool HasNonBreakWord(int eos);
        }

        private static readonly string PERIODS = "。？！♪…\\?\\!";
        private static readonly string DOT = "\\.．";
        private static readonly string CDOT = "・";
        private static readonly string COMMA = ",，、";
        private static readonly string BR_TAG = "(<br>|<BR>){2,}";
        private static readonly string ALPHABET_OR_NUMBER = "a-zA-Z0-9ａ-ｚＡ-Ｚ０-９〇一二三四五六七八九十百千万億兆";

        private static readonly Regex SENTENCE_BREAKER_PATTERN =
            new Regex("([" + PERIODS + "]|" + CDOT + "{3,}+|((?<!([" + ALPHABET_OR_NUMBER + "]))[" + DOT + "](?!(["
                        + ALPHABET_OR_NUMBER + COMMA + "]))))([" + DOT + PERIODS + "])*|" + BR_TAG);

        private static readonly string OPEN_PARENTHESIS = "\\(\\{｛\\[（「【『［≪〔“";
        private static readonly string CLOSE_PARENTHESIS = "\\)\\}\\]）」｝】』］〕≫”";

        private static readonly string ITEMIZE_HEADER = "([" + ALPHABET_OR_NUMBER + "])" + "([" + DOT + "])";
        private static readonly Regex ITEMIZE_HEADER_PATTERN = new Regex(ITEMIZE_HEADER);

        /// <summary>the default maximum length of a sentence</summary>
        public static readonly int DEFAULT_LIMIT = 4096;

        private int limit;

        /// <summary>
        /// Initializes a new instance of the <see cref="SentenceDetector"/> class.
        /// </summary>
        public SentenceDetector() : this(-1) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SentenceDetector"/> class with length limitation of sentence.
        /// </summary>
        /// <param name="limit">the maximum length of a sentence.</param>
        public SentenceDetector(int limit)
        {
            this.limit = (limit > 0) ? limit : DEFAULT_LIMIT;
        }

        private static readonly Regex Spaces = new Regex(".+\\s+");

        /// <summary>
        /// Returns the index of the detected end of the sentence.
        /// </summary>
        /// <remarks>
        /// If <paramref name="checker"/> is not <c>null</c>, it is used to determine if there is a
        /// word that crosses the detected boundary, and if so, the next boundary is
        /// returned.
        ///
        /// If there is no boundary, it returns a relatively harmless boundary as a
        /// negative value.
        /// </remarks>
        /// <param name="input">text</param>
        /// <param name="checker">a checker for words that cross boundaries</param>
        /// <returns>the index of the end of the sentence</returns>
        public int GetEos(string input, INonBreakChecker checker)
        {
            if (input.Length == 0)
            {
                return 0;
            }

            string s = (input.Length > limit) ? input.Substring(0, limit) : input;
            Match matcher = SENTENCE_BREAKER_PATTERN.Match(s);
            while (matcher.Success)
            {
                int eos = matcher.Index + matcher.Length;
                if (ParenthesisLevel(s.Substring(0, eos)) == 0)
                {
                    if (eos < s.Length)
                    {
                        eos += ProhibitedBOS(s.Substring(eos));
                    }
                    if (ITEMIZE_HEADER_PATTERN.IsMatch(s.Substring(0, eos)))
                    {
                        matcher = matcher.NextMatch();
                        continue;
                    }
                    if (eos < s.Length && IsContinuousPhrase(s, eos))
                    {
                        matcher = matcher.NextMatch();
                        continue;
                    }
                    if (checker != null && checker.HasNonBreakWord(eos))
                    {
                        matcher = matcher.NextMatch();
                        continue;
                    }

                    return eos;
                }
                matcher = matcher.NextMatch();
            }

            if (input.Length > limit)
            {
                //Regex spaces = new Regex(".+\\s+");
                Match m = Spaces.Match(s);
                if (m.Success)
                {
                    return -m.Index - m.Length;
                }
            }

            return -Math.Min(input.Length, limit);
        }

        private static readonly Regex PARENTHESIS_PATTERN =
            new Regex("([" + OPEN_PARENTHESIS + "])|([" + CLOSE_PARENTHESIS + "])");

        private int ParenthesisLevel(string s)
        {
            MatchCollection matches = PARENTHESIS_PATTERN.Matches(s);
            int level = 0;
            foreach (Match match in matches)
            {
                if (match.Groups[1].Success) // open
                {
                    level++;
                }
                else
                {
                    level--;
                }
                if (level < 0)
                {
                    level = 0;
                }
            }
            return level;
        }

        private static readonly Regex PROHIBITED_BOS_PATTERN =
            new Regex("\\A([" + CLOSE_PARENTHESIS + COMMA + PERIODS + "])+");

        private int ProhibitedBOS(string s)
        {
            Match m = PROHIBITED_BOS_PATTERN.Match(s);
            // NOTE: マッチしたらマッチ文字列の終端の次の文字のオフセット位置を返す。
            return m.Success ? m.Index + m.Length : 0;
        }

        private static readonly Regex QUOTE_MARKER_PATTERN =
            new Regex("(！|？|\\!|\\?|[" + CLOSE_PARENTHESIS + "])(と|っ|です)");

        private static readonly Regex EOS_ITEMIZE_HEADER_PATTERN =
            new Regex(ITEMIZE_HEADER + "\\z");

        private bool IsContinuousPhrase(string s, int eos)
        {
            Match m = QUOTE_MARKER_PATTERN.Match(s, eos - 1);
            if (m.Success && m.Index == eos - 1)
            {
                return true;
            }

            char c = s[eos];
            return (c == 'と' || c == 'や' || c == 'の') && EOS_ITEMIZE_HEADER_PATTERN.IsMatch(s.Substring(0, eos));
        }
    }
}
