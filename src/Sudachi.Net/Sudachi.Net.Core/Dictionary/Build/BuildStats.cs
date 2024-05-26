using System.Collections.Generic;

namespace Sudachi.Net.Core.Dictionary.Build
{
    public class BuildStats
    {
        public IReadOnlyList<ModelOutput.Part> Inputs { get; }
        public IReadOnlyList<ModelOutput.Part> Parts { get; }

        public BuildStats(IReadOnlyList<ModelOutput.Part> inputs, IReadOnlyList<ModelOutput.Part> parts)
        {
            Inputs = inputs;
            Parts = parts;
        }
    }
}
