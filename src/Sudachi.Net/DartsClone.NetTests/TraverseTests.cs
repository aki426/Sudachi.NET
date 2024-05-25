using System.Text;
using DartsClone.Net;
using DartsClone.Net.Details;
using FluentAssertions;
using Xunit;

namespace DartsClone.NetTests
{
    public class TraverseTests
    {
        private DoubleArray _dic;

        private static readonly byte[] A = Encoding.UTF8.GetBytes("a");
        private static readonly byte[] AB = Encoding.UTF8.GetBytes("ab");
        private static readonly byte[] AC = Encoding.UTF8.GetBytes("ac");
        private static readonly byte[] B = Encoding.UTF8.GetBytes("b");
        private static readonly byte[] C = Encoding.UTF8.GetBytes("c");
        private static readonly byte[] CD = Encoding.UTF8.GetBytes("cd");

        public TraverseTests()
        {
            byte[][] keys = { A, AB, CD };
            _dic = DoubleArrayBuilder.Build(keys, null, null);
        }

        [Fact]
        public void TraverseTest()
        {
            var r = _dic.Traverse(A, 0, 0);
            r.Result.Should().BeGreaterOrEqualTo(0);
            r.Offset.Should().Be(1);

            var r2 = _dic.Traverse(B, 0, r.NodePosition);
            r2.Result.Should().BeGreaterOrEqualTo(0);
            r2.Offset.Should().Be(1);

            var r3 = _dic.Traverse(AB, 1, r.NodePosition);
            r3.Result.Should().BeGreaterOrEqualTo(0);
            r3.Offset.Should().Be(2);
        }

        [Fact]
        public void TraverseNotMatchWithEndTest()
        {
            var r = _dic.Traverse(C, 0, 0);
            r.Result.Should().Be(-1);
            r.Offset.Should().Be(1);

            var r2 = _dic.Traverse(CD, 1, r.NodePosition);
            r2.Result.Should().BeGreaterOrEqualTo(0);
            r2.Offset.Should().Be(2);
        }

        [Fact]
        public void TraverseNotMatchMiddleTest()
        {
            var r = _dic.Traverse(AC, 0, 0);
            r.Result.Should().Be(-2);
            r.Offset.Should().Be(1);

            var r2 = _dic.Traverse(B, 0, r.NodePosition);
            r2.Result.Should().BeGreaterOrEqualTo(0);
            r2.Offset.Should().Be(1);
        }

        [Fact]
        public void TraverseWithLengthTest()
        {
            var r = _dic.Traverse(AB, 0, 1, 0);
            r.Result.Should().BeGreaterOrEqualTo(0);
            r.Offset.Should().Be(1);
        }
    }
}
