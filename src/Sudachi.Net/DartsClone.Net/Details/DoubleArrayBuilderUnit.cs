namespace DartsClone.Net.Details
{
    /// <summary>
    /// DoubleArrayのBuild時ユニット1つ分を表すクラス。
    /// NOTE: 便宜上状態変更可能なクラスとした。
    /// </summary>
    internal class DoubleArrayBuilderUnit
    {
        public int Unit;

        /// <summary>実際的には下位9ビット目を0にするかしないかのフラグ。Trueの時1、Falseの時0。</summary>
        public void SetHasLeaf(bool value)
        {
            if (value)
            {
                // 256 = 0x100 = 0b0001_0000_0000
                Unit = this.Unit | 0x100;
            }
            else
            {
                // -257 = FFFF FFFF FFFF FEFF = 下から9ビット目のみ0の数。
                Unit = this.Unit & ~0x100;
            }
        }

        // intに設定可能な最小値 -2^31を保持する定数
        // 符号付きintの最小値は0x8000_0000つまり-2,147,483,648
        // TODO: すべからく負の数になってしまうがこれは右シフト演算を考慮しての対応？
        public void SetValue(int value) => Unit = value | int.MinValue;

        public void SetLabel(byte value) => Unit = (int)((Unit & ~0xFF) | (uint)value);

        public void SetOffset(int offset)
        {
            // (1 << 31) | (1 << 8) | 0xFF = 0x8000_01FF = ~0x7FFF_FE00
            int temp = (int)((uint)this.Unit & 0x8000_01FF);
            // 1 << 21 = 2097152 = 0x20_0000
            if (offset < 0x200000)
            {
                // 9ビット目より上に持っていく。
                Unit = temp | offset << 10;
            }
            else
            {
                // 512 = 0x200 = 0b0010_0000_0000
                // C#のビット演算子はシフト演算子よりも優先順位が低い。これはJAVAも同じ。
                // つまり2ビット左シフトしてからOR演算子で10桁目のみを1にする。
                // これはvalueの値の8ビット目を10ビット目にずらしてUnitとORを取る動き。
                Unit = temp | offset << 2 | 512;
            }
        }
    }

    //public record DoubleArrayBuilderUnit
    //{
    //    public int Unit { get; init; }

    //    /// <summary>実際的には下位9ビット目を0にするかしないかのフラグ。Trueの時1、Falseの時0。</summary>
    //    public DoubleArrayBuilderUnit SetHasLeaf(bool value)
    //    {
    //        if (value)
    //        {
    //            // 256 = 0x100 = 0b0001_0000_0000
    //            return new DoubleArrayBuilderUnit { Unit = this.Unit | 0x100 };
    //        }
    //        else
    //        {
    //            // -257 = FFFF FFFF FFFF FEFF = 下から9ビット目のみ0の数。
    //            return new DoubleArrayBuilderUnit { Unit = this.Unit & ~0x100 };
    //        }
    //    }

    //    // intに設定可能な最小値 -2^31を保持する定数
    //    // 符号付きintの最小値は0x8000_0000つまり-2,147,483,648
    //    // TODO: すべからく負の数になってしまうがこれは右シフト演算を考慮しての対応？
    //    public DoubleArrayBuilderUnit SetValue(int value)
    //    {
    //        return new DoubleArrayBuilderUnit { Unit = value | int.MinValue };
    //    }

    //    public DoubleArrayBuilderUnit SetLabel(byte value)
    //    {
    //        // -256 = FFFF FFFF FFFF FF00
    //        return new DoubleArrayBuilderUnit { Unit = (int)((Unit & ~0xFF) | (uint)value) };
    //    }

    //    public DoubleArrayBuilderUnit SetOffset(int value)
    //    {
    //        // -2147483137 = 0x8000_01FF = ~0x7FFF_FE00
    //        int temp = (int)((uint)this.Unit & 0x8000_01FF);
    //        // 2097152 = 0x20_0000
    //        if (value < 0x200000)
    //        {
    //            // 9ビット目より上に持っていく。

    //            return new DoubleArrayBuilderUnit { Unit = temp | value << 10 };
    //        }
    //        else
    //        {
    //            // 512 = 0x200 = 0b0010_0000_0000
    //            // C#のビット演算子はシフト演算子よりも優先順位が低い。これはJAVAも同じ。
    //            // つまり2ビット左シフトしてからOR演算子で10桁目のみを1にする。
    //            // これはvalueの値の8ビット目を10ビット目にずらしてUnitとORを取る動き。
    //            return new DoubleArrayBuilderUnit { Unit = temp | value << 2 | 512 };
    //        }
    //    }
    //}
}
