using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using BBPalCalc.Interfaces;

namespace BBPalCalc.Util
{
    public static class Helpers
    {
        public static ushort[] PaletteTo12BitPalette(Color[] palette)
        {
            var res = new ushort[palette.Length];
            for (var i = 0; i < palette.Length; i++)
            {
                res[i] = Globals.ActivePlatform.ToPlatformColor(palette[i]);
            }
            return res;
        }

        public static string RgbPaletteTo12BitString(Color[] palette)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < palette.Length; i++)
            {
                var stColor = Globals.ActivePlatform.ColorToString(palette[i]);
                sb.Append("$");
                sb.Append(stColor);
                if (i < palette.Length-1) sb.Append(",");
            }
            return sb.ToString();
        }

        public static List<byte> GetBits(ushort v, bool reverse = false)
        {
            var data = new List<byte>();
            for (var b = 1; b <= 16; b++)
            {
                var bitNumber = b;
                var bit = (v & (1 << bitNumber - 1)) != 0;
                data.Add(bit ? (byte)1 : (byte)0);
            }
            if (reverse) data.Reverse();
            return data;
        }

        public static string DumpBits(List<byte> data)
        {
            var sb = new StringBuilder();
            foreach (var bit in data)
            {
                sb.Append(bit);
                sb.Append(",");
            }

            return sb.ToString();
        }

        private static void GetScaledRgbPartsFromColor(Color color, out byte r, out byte g, out byte b)
        {
            r = (byte)(color.R / Globals.ActivePlatform.ScaleFactor);
            g = (byte)(color.G / Globals.ActivePlatform.ScaleFactor);
            b = (byte)(color.B / Globals.ActivePlatform.ScaleFactor);
        }
        public static IEnumerable<Color> GetGradients(Color start, Color end, int steps)
        {
            GetScaledRgbPartsFromColor(start, out var r, out var g, out var b);
            var sr = (double)r;
            var sg = (double)g;
            var sb = (double)b;
            GetScaledRgbPartsFromColor(end, out var r1, out var g1, out var b1);
            var er = (double)r1;
            var eg = (double)g1;
            var eb = (double)b1;

            var stepR = (er - sr) / (steps - 1);
            var stepG = (eg - sg) / (steps - 1);
            var stepB = (eb - sb) / (steps - 1);

            for (var i = 0; i < steps; i++)
            {
                var nr = Globals.ActivePlatform.RemapFromLowerDepth((byte)(sr + stepR * i));
                var ng = Globals.ActivePlatform.RemapFromLowerDepth((byte)(sg + stepG * i));
                var nb = Globals.ActivePlatform.RemapFromLowerDepth((byte)(sb + stepB * i));

                yield return Color.FromRgb(nr, ng, nb);
            }
        }

        public static byte GetByte(byte[] data)
        {
            byte res = 0;
            for (var i = 0; i < 8; i++)
            {
                if (data[i] == 1)
                {
                    res |= (byte)(1 << (7 - i));
                }
            }
            return (byte)res;
        }

        public static bool GetBit(this byte b, int bitNumber)
        {
            var ba = new BitArray(new byte[] { b });
            return ba.Get(bitNumber);
        }
        public static byte[] GetBits(this byte b)
        {
            var res = new byte[8];
            for (var i = 0; i < 8; i++)
            {
                var isSet = b.GetBit(i);
                res[i] = isSet ? (byte)1 : (byte)0;
            }
            return res;
        }

        public static IGlobals Globals => App.GetGlobal();
    }
}
