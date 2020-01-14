using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using StPalCalc.iff;

namespace StPalCalc.PictureFormats
{
    public class IffPicture : IPicture
    {
        private byte[] _pixelData;
        private int _width { get; set; }
        private int _height { get; set; }
        public Color[] OriginalPalette { get; private set; }
        public Color[] ActivePalette { get; set; }
        public string Filename { get; private set; }
        public (int, int) GetDimensions => (_width, _height);
        public void Load(string filename)
        {
            Filename = filename;
            var iff = new IffReader();
            iff.Parse(filename);
            var (w, h) = iff.GetDimensions;
            _width = w;
            _height = h;
            OriginalPalette = new Color[iff.CMAP.Length];
            ActivePalette = OriginalPalette.ToArray();
            for (var i = 0; i < iff.CMAP.Length; i++) OriginalPalette[i] = iff.CMAP[i];
            _pixelData = new byte[_width*_height];
            for (var i = 0; i < iff.PixelData.Length; i++)
            {
                _pixelData[i] = iff.PixelData[i];
            }
        }

        public void Render(Image target, Color[] specialPalette = null)
        {
            var palette = specialPalette ?? OriginalPalette;
            var wbmp = BitmapFactory.New(_width, _height);
            target.Source = wbmp;
            using (wbmp.GetBitmapContext())
            {
                for (var y = 0; y < _height; y++)
                {
                    for (var x = 0; x < _width; x++)
                    {
                        var bv = _pixelData[x + y * _width];
                        wbmp.SetPixel(x, y, palette[bv]);
                    }
                }
            }
        }

        public void RenderWithRasters(Image target, List<GradientItem> rasters, int maskIndex)
        {
            var wbmp = BitmapFactory.New(_width, _height);
            target.Source = wbmp;
            using (wbmp.GetBitmapContext())
            {
                for (var y = 0; y < _height; y++)
                {
                    for (var x = 0; x < _width; x++)
                    {
                        var bv = _pixelData[x + y * _width];
                        if (bv == maskIndex)
                        {
                            wbmp.SetPixel(x, y, rasters[y].Color);
                        }
                        else
                        {
                            wbmp.SetPixel(x, y, OriginalPalette[bv]);
                        }
                    }
                }
            }
        }
    }
}
