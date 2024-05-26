using System;

namespace Sudachi.Net.Core.Dictionary.Build
{
    public class InputFileException : ArgumentException
    {
        public InputFileException(int line, string s, Exception cause)
            : base($"line:{line} {s}", cause)
        {
        }
    }
}
