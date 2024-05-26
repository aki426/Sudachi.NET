using System;
using System.Collections.Generic;
using System.Text;
using Sudachi.Net.Core.Dictionary.Lexicon;

namespace Sudachi.Net.Core.Dictionary
{
    /**
      * Marks access into dictionary internals
      */

    public interface IDictionaryAccess
    {
        /**
         * Gets current Lexicon.
         *
         * @return Lexicon implementation
         */

        ILexicon GetLexicon();

        /**
         * Gets current grammar.
         *
         * @return current Grammar
         */

        GrammarImpl GetGrammar();
    }
}
