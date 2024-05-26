using System;
using System.Collections.Generic;
using System.Text;

namespace Sudachi.Net.Core.Dictionary.Build
{
    public interface IWriteDictionary
    {
        void WriteTo(ModelOutput output);
    }
}
