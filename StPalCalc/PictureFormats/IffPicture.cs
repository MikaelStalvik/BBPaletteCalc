using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BBPalCalc.iff;
using BBPalCalc.Interfaces;

namespace BBPalCalc.PictureFormats
{
    public class IffPicture : IPicture
    {
        private byte[] _pixelData;
        private int _width { get; set; }
        private int _height { get; set; }
        private int _colors { get; set; }
        public Color[] OriginalPalette { get; private set; }
        public Color[] ActivePalette { get; set; }
        public string Filename { get; private set; }
        public (int, int) GetDimensions => (_width, _height);
        public int Colors { get; private set; }
        public bool Load(string filename)
        {
            Filename = filename;
            var iff = new IffReader();
            iff.Parse(filename);
            if (iff.CMAP == null)
            {
                MessageBox.Show("Unsupported IFF (24-bit?)");
                return false;
            }
            var (w, h) = iff.GetDimensions;
            _width = w;
            _height = h;
            _colors = iff.CMAP.Length;
            OriginalPalette = new Color[iff.CMAP.Length];
            for (var i = 0; i < iff.CMAP.Length; i++) OriginalPalette[i] = iff.CMAP[i];
            ActivePalette = OriginalPalette.ToArray();
            _pixelData = new byte[_width*_height];
            for (var i = 0; i < iff.PixelData.Length; i++)
            {
                _pixelData[i] = iff.PixelData[i];
            }

            return true;
        }

        public void Render(Image target, Color[] specialPalette = null)
        {
            var palette = specialPalette ?? ActivePalette;
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
                        if (bv == maskIndex && y < rasters.Count)
                        {
                            wbmp.SetPixel(x, y, rasters[y].Color);
                        }
                        else
                        {
                            wbmp.SetPixel(x, y, ActivePalette[bv]);
                        }
                    }
                }
            }
        }
    }
}
