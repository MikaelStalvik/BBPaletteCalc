using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;

namespace StPalCalc.Platforms
{
    public class AtariStePlatform : IPlatform
    {
        private byte RemapFromPlatformColor(byte source)
        {
            switch (source)
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
                    return source;
            }
        }

        private byte RemapToPlatformColor(byte source)
        {
            switch (source)
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
                    return source;
            }
        }

        private byte ScaleColor(byte source)
        {
            var scaledCol = ((source + 1) * 16) - 1;
            if (scaledCol < 0) scaledCol = 0;
            if (scaledCol > 255) scaledCol = 255;
            return (byte)scaledCol;
        }

        private byte GetRValue(List<byte> data)
        {
            var b1 = data[8];
            var b2 = data[9];
            var b3 = data[10];
            var b4 = data[11];
            return RemapFromPlatformColor((byte)(b4 * 8 + b3 * 4 + b2 * 2 + b1));
        }

        private byte GetGValue(List<byte> data)
        {
            var b1 = data[4];
            var b2 = data[5];
            var b3 = data[6];
            var b4 = data[7];
            return RemapFromPlatformColor((byte)(b4 * 8 + b3 * 4 + b2 * 2 + b1));
        }

        private byte GetBValue(List<byte> data)
        {
            var b1 = data[0];
            var b2 = data[1];
            var b3 = data[2];
            var b4 = data[3];
            return RemapFromPlatformColor((byte)(b4 * 8 + b3 * 4 + b2 * 2 + b1));
        }

        public Color ToRgb(ushort source)
        {
            var bits = Helpers.GetBits(source);
            var r = GetRValue(bits);
            var g = GetGValue(bits);
            var b = GetBValue(bits);
            r = ScaleColor(r);
            g = ScaleColor(g);
            b = ScaleColor(b);

            return Color.FromRgb(r, g, b);
        }

        public string ColorToString(Color source)
        {
            var r = source.R/16;
            var g = source.G/16;
            var b = source.B/16;
            r = RemapToPlatformColor((byte)r);
            g = RemapToPlatformColor((byte)g);
            b = RemapToPlatformColor((byte)b);
            return $"{r:X}{g:X}{b:X}";
        }

        public byte RemapFromLowerDepth(byte color)
        {
            switch (color)
            {
                case 0: return 0;
                case 1: return 17;
                case 2: return 17 * 2;
                case 3: return 17 * 3;
                case 4: return 17 * 4;
                case 5: return 17 * 5;
                case 6: return 17 * 6;
                case 7: return 17 * 7;
                case 8: return 17 * 8;
                case 9: return 17 * 9;
                case 10: return 17 * 10;
                case 11: return 17 * 11;
                case 12: return 17 * 12;
                case 13: return 17 * 13;
                case 14: return 17 * 14;
                case 15: return 17 * 15;
                default:
                    return 0;
            }
        }
    }
}
