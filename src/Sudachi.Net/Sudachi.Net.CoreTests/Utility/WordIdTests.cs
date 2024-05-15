using FluentAssertions;
using Sudachi.Net.Core.Utility;
using Xunit;

namespace Sudachi.Net.CoreTests.Utility
{
    public class WordIdTests
    {
        [Fact()]
        public void MakeTest()
        {
            WordId.Make(0, 0).Should().Be(0);
            WordId.Make(0, 1).Should().Be(1);
            WordId.Make(0, 5).Should().Be(5);
            // 0b0001_0000_0000_0000_0000_0000_0000_0101
            WordId.Make(1, 5).Should().Be(268435456 + 5);
            WordId.Make(1, 5).Should().Be((1 << 28) + 5);

            // 0b1000 is minus
            // 0b1000_0000_0000_0000_0000_0000_0000_0101 = -2147483643
            WordId.Make(8, 5).Should().Be(-2147483643);

            // error
            Action action = () => WordId.Make(0, WordId.MAX_WORD_ID + 1);
            action.Should().Throw<IndexOutOfRangeException>();

            action = () => WordId.Make(WordId.MAX_DIC_ID + 1, 0);
            action.Should().Throw<IndexOutOfRangeException>();

            action = () => WordId.Make(-1, 0);
            action.Should().Throw<IndexOutOfRangeException>();

            action = () => WordId.Make(0, -1);
            action.Should().Throw<IndexOutOfRangeException>();
        }

        [Fact()]
        public void DicTest()
        {
            // 12 = 0b1100
            // 51612312 = 0b0000_0011_0001_0011_1000_1010_1001_1000
            WordId.Dic(WordId.Make(12, 51612312)).Should().Be(12);

            // 14 = 0b1110
            // 51612312 = 0b0000_0011_0001_0011_1000_1010_1001_1000
            WordId.Dic(WordId.Make(14, 51612312)).Should().Be(14);

            // 14 = 0b1110
            // 268435455 = 0x0FFF_FFFF
            WordId.Dic(WordId.Make(14, 268435455)).Should().Be(14);

            // MAX
            WordId.Dic(WordId.Make(WordId.MAX_DIC_ID, WordId.MAX_WORD_ID)).Should().Be(WordId.MAX_DIC_ID);
        }

        [Fact()]
        public void WordTest()
        {
            // 12 = 0b1100
            // 51612312 = 0b0000_0011_0001_0011_1000_1010_1001_1000
            WordId.Word(WordId.Make(12, 51612312)).Should().Be(51612312);

            // 14 = 0b1110
            // 51612312 = 0b0000_0011_0001_0011_1000_1010_1001_1000
            WordId.Word(WordId.Make(14, 51612312)).Should().Be(51612312);

            // 14 = 0b1110
            // 268435455 = 0x0FFF_FFFF
            WordId.Word(WordId.Make(14, 268435455)).Should().Be(268435455);

            // MAX
            WordId.Word(WordId.Make(WordId.MAX_DIC_ID, WordId.MAX_WORD_ID)).Should().Be(WordId.MAX_WORD_ID);
        }

        [Fact()]
        public void DicIdMaskTest()
        {
            WordId.DicIdMask(8).Should().Be(8 << 28);
            WordId.DicIdMask(8).Should().Be(-2147483648);
        }

        [Fact()]
        public void ApplyMaskTest()
        {
            WordId.ApplyMask(5, WordId.DicIdMask(8)).Should().Be(-2147483643);
        }
    }
}