using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace StPalCalc
{
    public interface IPicture
    {
        string Filename { get; }
        (int, int) GetDimensions { get; }
        void Load(string filename);
        void Render(Image target);
        void RenderWithRasters(Image target, List<GradientItem> rasters, int maskIndex);
    }
}
