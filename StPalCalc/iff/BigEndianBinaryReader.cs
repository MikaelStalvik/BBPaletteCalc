using System;
using System.IO;

namespace BBPalCalc.iff
{
    class BigEndianBinaryReader : BinaryReader
    {
        private byte[] a16 = new byte[2];
        private byte[] a32 = new byte[4];
        private byte[] a64 = new byte[8];

        public BigEndianBinaryReader(Stream input) : base(input)
        {
        }

        public override int ReadInt32()
        {
            a32 = base.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(a32);
            return BitConverter.ToInt32(a32, 0);
        }

        public override Int16 ReadInt16()
        {
            a16 = base.ReadBytes(2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(a16);
            return BitConverter.ToInt16(a16, 0);
        }

        public override UInt16 ReadUInt16()
        {
            a16 = base.ReadBytes(2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(a16);
            return BitConverter.ToUInt16(a16, 0);
        }

        public override Int64 ReadInt64()
        {
            a64 = base.ReadBytes(8);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(a64);
            return BitConverter.ToInt64(a64, 0);
        }

        public override UInt32 ReadUInt32()
        {
            a32 = base.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(a32);
            return BitConverter.ToUInt32(a32, 0);
        }
    }
}
