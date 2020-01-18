using System.Collections.Generic;
using System.Windows.Media;
using BBPalCalc.Util;

namespace BBPalCalc.Platforms
{
    public class AtariStPlatform : IPlatform
    {
        private byte ScaleColor(byte source)
        {
            var scaledCol = ((source + 1) * 32) - 1;
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
            return (byte)(b3 * 4 + b2 * 2 + b1);
        }

        private byte GetGValue(List<byte> data)
        {
            var b1 = data[4];
            var b2 = data[5];
            var b3 = data[6];
            var b4 = data[7];
            return (byte)(b3 * 4 + b2 * 2 + b1);
        }

        private byte GetBValue(List<byte> data)
        {
            var b1 = data[0];
            var b2 = data[1];
            var b3 = data[2];
            var b4 = data[3];
            return (byte)(b3 * 4 + b2 * 2 + b1);
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
            var r = source.R / 32;
            var g = source.G / 32;
            var b = source.B / 32;
            r = ((byte)r);
            g = ((byte)g);
            b = ((byte)b);
            return $"{r:X}{g:X}{b:X}";
        }

        public byte RemapFromLowerDepth(byte color)
        {
            switch (color)
            {
                case 0: return 0;
                case 1: return 36;
                case 2: return 36 * 2;
                case 3: return 36 * 3;
                case 4: return 36 * 4;
                case 5: return 36 * 5;
                case 6: return 36 * 6;
                case 7: return 36 * 7;
                default:
                    return 0;
            }
        }

        public int ScaleFactor => 32;
    }
}
