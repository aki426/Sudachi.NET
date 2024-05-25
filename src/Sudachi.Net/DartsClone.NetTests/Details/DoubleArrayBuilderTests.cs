using Xunit;
using Xunit.Abstractions;

namespace DartsClone.Net.Details.Tests
{
    public class DoubleArrayBuilderTests
    {
        private ITestOutputHelper _output;

        public DoubleArrayBuilderTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact()]
        public void UPPER_MASKTest()
        {
            _output.WriteLine($"0xFF << 21 : {0xFF << 21}");
            _output.WriteLine($"UPPER_MASK : {DoubleArrayBuilder.UPPER_MASK}");
        }
    }
}
