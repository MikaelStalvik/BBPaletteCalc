using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBPalCalc.Util
{
    public static class RgbHsl
    {
        public static void RgbToHsl(byte r, byte g, byte b, out double h, out double s, out double l)
        {
            double rNorm = r / 255.0;
            double gNorm = g / 255.0;
            double bNorm = b / 255.0;

            double max = Math.Max(rNorm, Math.Max(gNorm, bNorm));
            double min = Math.Min(rNorm, Math.Min(gNorm, bNorm));
            double delta = max - min;

            l = (max + min) / 2.0;

            if (delta == 0)
            {
                h = 0;
                s = 0;
            }
            else
            {
                s = l < 0.5 ? delta / (max + min) : delta / (2.0 - max - min);

                if (rNorm == max)
                {
                    h = (gNorm - bNorm) / delta + (gNorm < bNorm ? 6 : 0);
                }
                else if (gNorm == max)
                {
                    h = (bNorm - rNorm) / delta + 2;
                }
                else
                {
                    h = (rNorm - gNorm) / delta + 4;
                }

                h /= 6.0;
            }
        }

        public static void HslToRgb(double h, double s, double l, out byte r, out byte g, out byte b)
        {
            double rNorm, gNorm, bNorm;

            if (s == 0)
            {
                rNorm = gNorm = bNorm = l;
            }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;

                rNorm = HueToRgb(p, q, h + 1.0 / 3.0);
                gNorm = HueToRgb(p, q, h);
                bNorm = HueToRgb(p, q, h - 1.0 / 3.0);
            }

            r = (byte)Math.Round(rNorm * 255);
            g = (byte)Math.Round(gNorm * 255);
            b = (byte)Math.Round(bNorm * 255);
        }

        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2.0) return q;
            if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
            return p;
        }
    }
}
