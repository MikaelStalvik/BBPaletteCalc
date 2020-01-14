using System;

namespace StPalCalc.iff
{
    public class BitReader
    {
        public int Position = 0;
        private byte[] source;

        public BitReader(byte[] source)
        {
            this.source = source;
        }

        public int Length { get { return source.Length * 8; } }

        /// <summary>
        /// Reads bitCount bits from stream and return as UInt32
        /// </summary>
        public UInt32 Read(int bitCount)
        {
            UInt32 res = 0;

            for (int i = 0; i < bitCount; i++)
            {
                var bit = Read();
                if (bit < 0)
                    throw new Exception("Can not read needed count of the bits");
                unchecked
                {
                    res = (res << 1) | (UInt32)bit;
                }
            }

            return res;
        }

        /// <summary>
        /// Reads one bit from stream
        /// </summary>
        public int Read()
        {
            if (Position >= Length) return -1;
            var i = Position / 8;
            var j = Position % 8;
            Position++;
            return (source[i] >> (7 - j)) & 0x01;
        }

        /// <summary>
        /// Moves Position
        /// </summary>
        public bool Seek(int shift)
        {
            Position += shift;
            if (Position < 0)
            {
                Position = 0;
                return false;
            }

            if (Position >= Length)
            {
                Position = Length - 1;
                return false;
            }

            return true;
        }
    }
}
