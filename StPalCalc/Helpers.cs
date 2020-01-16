using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace BBPalCalc
{
    public enum PlatformTypes
    {
        AtariSte,
        Amiga,
        AtariSt
    };
    public static class Helpers
    {
        public static  PlatformTypes ActivePlatformType { get; set; }

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
        //public static Color FromStString(string source)
        //{
        //    int asHex;
        //    try
        //    {
        //        asHex = Convert.ToInt32(source, 16);
        //    }
        //    catch
        //    {
        //        asHex = 0;
        //    }

        //    var bits = GetBits((ushort)asHex);
        //    var r = GetRValue(bits);
        //    var g = GetGValue(bits);
        //    var b = GetBValue(bits);
        //    r = RemapFrom12BitToRgb(r);
        //    g = RemapFrom12BitToRgb(g);
        //    b = RemapFrom12BitToRgb(b);

        //    return Color.FromRgb(r, g, b);
        //}
        //public static Color StColorFromInt(int source)
        //{
        //    var bits = GetBits((ushort)source);
        //    var r = GetRValue(bits);
        //    var g = GetGValue(bits);
        //    var b = GetBValue(bits);
        //    r = RemapFrom12BitToRgb(r);
        //    g = RemapFrom12BitToRgb(g);
        //    b = RemapFrom12BitToRgb(b);

        //    return Color.FromRgb(r, g, b);
        //}

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

        //private static byte RemapSteColor(byte b)
        //{
        //    switch (b)
        //    {
        //        case 0: return 0;
        //        case 8: return 1;
        //        case 1: return 2;
        //        case 9: return 3;
        //        case 2: return 4;
        //        case 10: return 5;
        //        case 3: return 6;
        //        case 11: return 7;
        //        case 4: return 8;
        //        case 12: return 9;
        //        case 5: return 10;
        //        case 13: return 11;
        //        case 6: return 12;
        //        case 14: return 13;
        //        case 7: return 14;
        //        case 15: return 15;
        //        default:
        //            return b;
        //    }
        //}

        //private static byte RemapToSteColor(byte b)
        //{
        //    switch (b)
        //    {
        //        case 0: return 0;
        //        case 1: return 8;
        //        case 2: return 1;
        //        case 3: return 9;
        //        case 4: return 2;
        //        case 5: return 10;
        //        case 6: return 3;
        //        case 7: return 11;
        //        case 8: return 4;
        //        case 9: return 12;
        //        case 10: return 5;
        //        case 11: return 13;
        //        case 12: return 6;
        //        case 13: return 14;
        //        case 14: return 7;
        //        case 15: return 15;
        //        default:
        //            return b;
        //    }
        //}

        //public static byte GetRValue(List<byte> data)
        //{
        //    var b1 = data[8];
        //    var b2 = data[9];
        //    var b3 = data[10];
        //    var b4 = data[11];
        //    switch (ActivePlatformType)
        //    {
        //        case PlatformTypes.Amiga:
        //        case PlatformTypes.AtariSte:
        //            return RemapSteColor((byte)(b4 * 8 + b3 * 4 + b2 * 2 + b1));
        //            //return (byte) (b4 * 8 + b3 * 4 + b2 * 2 + b1);
        //        case PlatformTypes.AtariSt:
        //            return (byte)(b3 * 4 + b2 * 2 + b1);
        //        default:
        //            return 0;
        //    }
        //}
        //public static byte GetGValue(List<byte> data)
        //{
        //    var b1 = data[4];
        //    var b2 = data[5];
        //    var b3 = data[6];
        //    var b4 = data[7];
        //    switch (ActivePlatformType)
        //    {
        //        case PlatformTypes.AtariSte:
        //        case PlatformTypes.Amiga:
        //            return RemapSteColor((byte)(b4 * 8 + b3 * 4 + b2 * 2 + b1));
        //            //return (byte)(b4 * 8 + b3 * 4 + b2 * 2 + b1);
        //        case PlatformTypes.AtariSt:
        //            return (byte)(b3 * 4 + b2 * 2 + b1);
        //        default:
        //            return 0;
        //    }
        //}
        //public static byte GetBValue(List<byte> data)
        //{
        //    var b1 = data[0];
        //    var b2 = data[1];
        //    var b3 = data[2];
        //    var b4 = data[3];
        //    switch (ActivePlatformType)
        //    {
        //        case PlatformTypes.AtariSte:
        //        case PlatformTypes.Amiga:
        //            return RemapSteColor((byte)(b4 * 8 + b3 * 4 + b2 * 2 + b1));
        //            //return (byte)(b4 * 8 + b3 * 4 + b2 * 2 + b1);
        //        case PlatformTypes.AtariSt:
        //            return (byte)(b3 * 4 + b2 * 2 + b1);
        //        default:
        //            return 0;
        //    }
        //}

        //public static string ConvertFromRgbTo12Bit(Color color, bool skipDollar = false)
        //{
        //    byte r = 0;
        //    byte g = 0;
        //    byte b = 0;
        //    switch (ActivePlatform)
        //    {
        //        case PlatformTypes.AtariSte:
        //            r = RemapToSteColor((byte)(color.R / 16));
        //            g = RemapToSteColor((byte)(color.G / 16));
        //            b = RemapToSteColor((byte)(color.B / 16));
        //            break;
        //        case PlatformTypes.Amiga:
        //            r = (byte)(color.R / 16);
        //            g = (byte)(color.G / 16);
        //            b = (byte)(color.B / 16);
        //            break;
        //        case PlatformTypes.AtariSt:
        //            r = (byte)(color.R / 32);
        //            g = (byte)(color.G / 32);
        //            b = (byte)(color.B / 32);
        //            break;
        //    }
        //    return skipDollar ? $"{r:X}{g:X}{b:X}" : $"${r:X}{g:X}{b:X}";
        //}
        private static byte RemapFrom12BitToRgb(byte source)
        {
            var scaledCol = ((source + 1) * ScaleFactor) - 1;
            if (scaledCol < 0) scaledCol = 0;
            if (scaledCol > 255) scaledCol = 255;
            return (byte)scaledCol;
        }

        private static int ScaleFactor => ActivePlatformType == PlatformTypes.AtariSt ? 32 : 16;

        private static void GetScaledRgbPartsFromColor(Color color, out byte r, out byte g, out byte b)
        {
            r = (byte)(color.R / ScaleFactor);
            g = (byte)(color.G / ScaleFactor);
            b = (byte)(color.B / ScaleFactor);
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
                var nr = RemapFrom12BitToRgb((byte)(sr + stepR * i));
                var ng = RemapFrom12BitToRgb((byte)(sg + stepG * i));
                var nb = RemapFrom12BitToRgb((byte)(sb + stepB * i));
                yield return Color.FromRgb(nr, ng, nb);
            }
        }

        //public static int GetHexValue(char c)
        //{
        //    switch (c)
        //    {
        //        case '0': return 0;
        //        case '1': return 1;
        //        case '2': return 2;
        //        case '3': return 3;
        //        case '4': return 4;
        //        case '5': return 5;
        //        case '6': return 6;
        //        case '7': return 7;
        //        case '8': return 8;
        //        case '9': return 9;
        //        case 'a': return 10;
        //        case 'b': return 11;
        //        case 'c': return 12;
        //        case 'd': return 13;
        //        case 'e': return 14;
        //        case 'f': return 15;
        //        default:
        //            return 0;
        //    }
        //}

        //public static (byte, byte, byte) SplitStringToRgb(string source)
        //{
        //    if (source.Length != 3) return (0, 0, 0);
        //    var s = source.ToLowerInvariant();
        //    var r = GetHexValue(s[0]);
        //    var g = GetHexValue(s[1]);
        //    var b = GetHexValue(s[2]);
        //    return ((byte)r, (byte)g, (byte)b);
        //}

        //public static Color ColorFromString(string data)
        //{
        //    if (data.Length != 3) return Colors.Red;
        //    var asLow = data.ToLowerInvariant();
        //    var r = GetHexValue(asLow[0]);
        //    var g = GetHexValue(asLow[1]);
        //    var b = GetHexValue(asLow[2]);
        //    if (ActivePlatformType == PlatformTypes.AtariSte)
        //    {
        //        r = RemapSteColor((byte)r);
        //        g = RemapSteColor((byte)g);
        //        b = RemapSteColor((byte)b);
        //    }
        //    return Color.FromRgb((byte)(r * ScaleFactor), (byte)(g * ScaleFactor), (byte)(b * ScaleFactor));
        //}

        public static IGlobals Globals => App.GetGlobal();
    }
}
