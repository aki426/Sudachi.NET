using DartsClone.Net;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DartsClone.NetTests
{
    public class DoubleArrayTests
    {
        private ITestOutputHelper _output;

        public DoubleArrayTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void BitCalcTest()
        {
            DoubleArray.HasLeaf(0x00000010).Should().BeFalse();
            DoubleArray.HasLeaf(0x00000100).Should().BeTrue();
            DoubleArray.HasLeaf(0x00000200).Should().BeFalse();

            DoubleArray.Value(5).Should().Be(5);
            DoubleArray.Value(-1).Should().Be(2147483647);
            DoubleArray.Value(-2147450879).Should().Be(32769);

            DoubleArray.Label(8).Should().Be(8);
            DoubleArray.Label(255).Should().Be(255);
            DoubleArray.Label(-1).Should().Be(-2147483393);

            DoubleArray.Offset(0x600).Should().Be(0x100);
            DoubleArray.Offset(0x400).Should().Be(0x01);
        }
    }
}
