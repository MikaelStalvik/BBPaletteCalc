using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace StPalCalc
{
    public static class Helpers
    {
        public static Color FromStString(string source)
        {
            var asHex = Convert.ToInt32(source, 16);
            var bits = GetBits((ushort)asHex);
            var r = GetRValue(bits);
            var g = GetGValue(bits);
            var b = GetBValue(bits);
            r = RemapFrom12BitToRgb(r);
            g = RemapFrom12BitToRgb(g);
            b = RemapFrom12BitToRgb(b);

            return Color.FromRgb(r, g, b);
        }
        public static Color StColorFromInt(int source)
        {
            var bits = GetBits((ushort)source);
            var r = GetRValue(bits);
            var g = GetGValue(bits);
            var b = GetBValue(bits);
            r = RemapFrom12BitToRgb(r);
            g = RemapFrom12BitToRgb(g);
            b = RemapFrom12BitToRgb(b);

            return Color.FromRgb(r, g, b);
        }

        public static List<byte> GetBits(ushort v)
        {
            var data = new List<byte>();
            for (var b = 1; b <= 16; b++)
            {
                var bitNumber = b;
                var bit = (v & (1 << bitNumber - 1)) != 0;
                data.Add(bit ? (byte)1 : (byte)0);
            }
            //data.Reverse();
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

        private static byte RemapStColor(byte b)
        {
            switch (b)
            {
                case 0: return 0;
                case 8: return 1;
                case 1: return 2;
                case 9: return 3;
                case 2: return 4;
                case 10: return 5;
                case 3: return 6;
                case 11: return 7;
                case 4: return 8;
                case 12: return 9;
                case 5: return 10;
                case 13: return 11;
                case 6: return 12;
                case 14: return 13;
                case 7: return 14;
                case 15: return 15;
                default:
                    return b;
            }
        }

        private static byte RemapToStColor(byte b)
        {
            switch (b)
            {
                case 0: return 0;
                case 1: return 8;
                case 2: return 1;
                case 3: return 9;
                case 4: return 2;
                case 5: return 10;
                case 6: return 3;
                case 7: return 11;
                case 8: return 4;
                case 9: return 12;
                case 10: return 5;
                case 11: return 13;
                case 12: return 6;
                case 13: return 14;
                case 14: return 7;
                case 15: return 15;
                default:
                    return b;
            }
        }

        public static byte GetRValue(List<byte> data)
        {
            var b1 = data[8];
            var b2 = data[9];
            var b3 = data[10];
            var b4 = data[11];
            return RemapStColor((byte)(b4 * 8 + b3 * 4 + b2 * 2 + b1));
        }
        public static byte GetGValue(List<byte> data)
        {
            var b1 = data[4];
            var b2 = data[5];
            var b3 = data[6];
            var b4 = data[7];
            return RemapStColor((byte)(b4 * 8 + b3 * 4 + b2 * 2 + b1));
        }
        public static byte GetBValue(List<byte> data)
        {
            var b1 = data[0];
            var b2 = data[1];
            var b3 = data[2];
            var b4 = data[3];
            return RemapStColor((byte)(b4 * 8 + b3 * 4 + b2 * 2 + b1));
        }

        public static string ConvertFromRgbTo12Bit(Color color, bool skipDollar = false)
        {
            var r = RemapToStColor((byte)(color.R / 16));
            var g = RemapToStColor((byte)(color.G / 16));
            var b = RemapToStColor((byte)(color.B / 16));
            if (skipDollar)
                return $"{r:X}{g:X}{b:X}";
            return $"${r:X}{g:X}{b:X}";
        }
        public static byte RemapFrom12BitToRgb(byte source)
        {
            return (byte)(source * 16);
        }
        public static Color ToColor(byte r, byte g, byte b)
        {
            return Color.FromRgb((byte)(r * 16), (byte)(g * 16), (byte)(b * 16));
        }

        public static IEnumerable<Color> GetGradients(Color start, Color end, int steps, int adder = 0)
        {
            var stepR = ((end.R - start.R) / (steps - 1));
            var stepG = ((end.G - start.G) / (steps - 1));
            var stepB = ((end.B - start.B) / (steps - 1));

            for (var i = 0; i < steps; i++)
            {
                yield return Color.FromRgb(
                    (byte)(start.R + (stepR * i) + adder),
                    (byte)(start.G + (stepG * i) + adder),
                    (byte)(start.B + (stepB * i) + adder)
                );
            }
        }
    }
}
