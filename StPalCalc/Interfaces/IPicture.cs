using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using BBPalCalc.Types;

namespace BBPalCalc.Interfaces
{
    public interface IPicture
    {
        Color[] OriginalPalette { get; }
        Color[] ActivePalette { get; set;  }
        ushort[] PlatformPalette { get; }
        int Colors { get; }
        string Filename { get; }
        (int, int) GetDimensions { get; }
        bool Load(string filename);
        void Render(Image target, Color[] specialPalette = null);
        void RenderWithRasters(Image target, List<GradientItem> rasters, int maskIndex);
        void SwapColors(byte source, byte dest);
        byte[] Pixels { get; }
    }
}
