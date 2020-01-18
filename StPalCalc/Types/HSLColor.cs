using System;
using System.Drawing;

namespace BBPalCalc.Types
{
    public class HSLColor
    {
        // Private data members below are on scale 0-1
        // They are scaled for use externally based on scale
        private double _hue = 1.0;
        private double _saturation = 1.0;
        private double _luminosity = 1.0;

        private const double Scale = 240.0;

        public double Hue
        {
            get => _hue * Scale;
            set => _hue = CheckRange(value / Scale);
        }
        public double Saturation
        {
            get => _saturation * Scale;
            set => _saturation = CheckRange(value / Scale);
        }
        public double Luminosity
        {
            get => _luminosity * Scale;
            set => _luminosity = CheckRange(value / Scale);
        }

        private double CheckRange(double value)
        {
            if (value < 0.0)
                value = 0.0;
            else if (value > 1.0)
                value = 1.0;
            return value;
        }

        public override string ToString()
        {
            return String.Format("H: {0:#0.##} S: {1:#0.##} L: {2:#0.##}", Hue, Saturation, Luminosity);
        }

        public string ToRGBString()
        {
            var color = (Color)this;
            return String.Format("R: {0:#0.##} G: {1:#0.##} B: {2:#0.##}", color.R, color.G, color.B);
        }

        public void RgbParts(out byte red, out byte green, out byte blue)
        {
            var color = (Color)this;
            red = color.R;
            green = color.G;
            blue = color.B;
        }
        #region Casts to/from System.Drawing.Color
        public static implicit operator Color(HSLColor hslColor)
        {
            double r = 0, g = 0, b = 0;
            if (hslColor._luminosity != 0.0)
            {
                if (hslColor._saturation == 0.0)
                    r = g = b = hslColor._luminosity;
                else
                {
                    var temp2 = GetTemp2(hslColor);
                    var temp1 = 2.0 * hslColor._luminosity - temp2;

                    r = GetColorComponent(temp1, temp2, hslColor._hue + 1.0 / 3.0);
                    g = GetColorComponent(temp1, temp2, hslColor._hue);
                    b = GetColorComponent(temp1, temp2, hslColor._hue - 1.0 / 3.0);
                }
            }
            return Color.FromArgb((byte)(255 * r), (byte)(255 * g), (byte)(255 * b));
        }

        private static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            temp3 = MoveIntoRange(temp3);
            if (temp3 < 1.0 / 6.0)
                return temp1 + (temp2 - temp1) * 6.0 * temp3;
            else if (temp3 < 0.5)
                return temp2;
            else if (temp3 < 2.0 / 3.0)
                return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
            else
                return temp1;
        }
        private static double MoveIntoRange(double temp3)
        {
            if (temp3 < 0.0)
                temp3 += 1.0;
            else if (temp3 > 1.0)
                temp3 -= 1.0;
            return temp3;
        }
        private static double GetTemp2(HSLColor hslColor)
        {
            double temp2;
            if (hslColor._luminosity < 0.5)  //<=??
                temp2 = hslColor._luminosity * (1.0 + hslColor._saturation);
            else
                temp2 = hslColor._luminosity + hslColor._saturation - (hslColor._luminosity * hslColor._saturation);
            return temp2;
        }

        public static implicit operator HSLColor(Color color)
        {
            var hslColor = new HSLColor();
            hslColor._hue = color.GetHue() / 360.0; // we store hue as 0-1 as opposed to 0-360 
            hslColor._luminosity = color.GetBrightness();
            hslColor._saturation = color.GetSaturation();
            return hslColor;
        }
        #endregion

        public void SetRGB(int red, int green, int blue)
        {
            var hslColor = (HSLColor)Color.FromArgb(red, green, blue);
            _hue = hslColor._hue;
            _saturation = hslColor._saturation;
            _luminosity = hslColor._luminosity;
        }

        public HSLColor() { }
        public HSLColor(Color color)
        {
            SetRGB(color.R, color.G, color.B);
        }
        public HSLColor(int red, int green, int blue)
        {
            SetRGB(red, green, blue);
        }
        public HSLColor(double hue, double saturation, double luminosity)
        {
            Hue = hue;
            Saturation = saturation;
            Luminosity = luminosity;
        }

    }
}