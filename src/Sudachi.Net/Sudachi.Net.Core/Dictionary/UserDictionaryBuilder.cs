using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sudachi.Net.Core.Dictionary.Build;

namespace Sudachi.Net.Core.Dictionary
{
    /**
     * A user dictionary building tool. This class provide the converter from the
     * source file in the CSV format to the binary format.
     */

    public class UserDictionaryBuilder
    {
        private static void PrintUsage()
        {
            Console.WriteLine("usage: UserDictionaryBuilder -o file -s file [-d description] files...");
            Console.WriteLine("\t-o file\toutput to file");
            Console.WriteLine("\t-s file\tsystem dictionary");
            Console.WriteLine("\t-d description\tcomment");
        }

        /**
         * Builds the user dictionary.
         * <p>
         * This tool requires three arguments.
         * <ol start="0">
         * <li>{@code -o file} the path of the output file</li>
         * <li>{@code -s file} the path of the system dictionary</li>
         * <li>{@code -d string} (optional) the description which is embedded in the
         * dictionary</li>
         * <li>the paths of the source file in the CSV format</li>
         * </ol>
         *
         * @param args
         *            options and input filenames
         * @throws IOException
         *             if IO or parsing is failed
         */

        public static void Main(string[] args)
        {
            string description = "";
            string outputPath = null;
            string sysDictPath = null;

            int i;
            for (i = 0; i < args.Length; i++)
            {
                if (args[i] == "-o" && i + 1 < args.Length)
                {
                    outputPath = args[++i];
                }
                else if (args[i] == "-s" && i + 1 < args.Length)
                {
                    sysDictPath = args[++i];
                }
                else if (args[i] == "-d" && i + 1 < args.Length)
                {
                    description = args[++i];
                }
                else if (args[i] == "-h")
                {
                    PrintUsage();
                    return;
                }
                else
                {
                    break;
                }
            }

            if (args.Length <= i || outputPath == null || sysDictPath == null)
            {
                PrintUsage();
                return;
            }

            List<string> lexiconPaths = args.Skip(i).ToList();

            using (BinaryDictionary system = new BinaryDictionary(sysDictPath))
            {
                DicBuilder.User builder = DicBuilder.User(system).Description(description)
                    .Progress(new Progress(20, new DictionaryBuilder.StderrProgress()));

                foreach (string lexicon in lexiconPaths)
                {
                    builder.Lexicon(lexicon);
                }

                using (FileStream stream = File.Open(outputPath, FileMode.Create))
                {
                    builder.Build(stream);
                }
            }
        }
    }
}
