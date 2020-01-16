using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BBPalCalc.Platforms;

namespace BBPalCalc.PictureFormats
{
    public class Pi1Picture : IPicture
    {
        private readonly byte[] _pixelData = new byte[320*200];
        private int _width { get; set; }
        private int _height { get; set; }
        private ushort[] _original12BitPalette = new ushort[16];
        public Color[] OriginalPalette { get; private set; }
        public Color[] ActivePalette { get; set; }
        public int Colors => 16;
        public string Filename { get; private set; }
        public (int, int) GetDimensions => (_width, _height);
        public bool Load(string filename)
        {
            _width = 320;
            _height = 200;
            Filename = filename;
            ReadPalette(filename, ref _original12BitPalette);
            MapPaletteToRgb();
            ActivePalette = OriginalPalette.ToArray();
            ReadPixels(filename);
            return true;
        }

        private void MapPaletteToRgb()
        {
            // Always use ST to remap colors otherwise it will fail
            var atariPlatform = PlatformFactory.CreatePlatform(PlatformTypes.AtariSte);
            OriginalPalette = new Color[Colors];
            for (var i = 0; i < Colors; i++)
            {
                var stColor = _original12BitPalette[i];
                OriginalPalette[i] = atariPlatform.ToRgb(stColor);
            }
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

        private void ReadPalette(string filename, ref ushort[] target)
        {
            using (var fs = new FileStream(filename, FileMode.Open))
            {
                fs.Position = 2; // skip resolution
                for (var i = 0; i < Colors; i++)
                {
                    var b1 = fs.ReadByte();
                    var b2 = fs.ReadByte();
                    var v = (ushort)(b2 + b1 * 256);
                    target[i] = v;
                }
            }
        }

        private void ReadPixels(string filename)
        {
            using (var fs = new FileStream(Filename, FileMode.Open))
            {
                fs.Position += 34; // skip res and palette
                for (var y = 0; y < _height; y++)
                {
                    var xo = 0;
                    for (var x = 0; x < 20; x++)
                    {
                        // read 4bpl, 16px in each
                        var b1 = fs.ReadByte();
                        var b2 = fs.ReadByte();
                        var v = (ushort)(b2 + b1 * 256);
                        var bpl1 = Helpers.GetBits(v, true);
                        b1 = fs.ReadByte();
                        b2 = fs.ReadByte();
                        v = (ushort)(b2 + b1 * 256);
                        var bpl2 = Helpers.GetBits(v, true);
                        b1 = fs.ReadByte();
                        b2 = fs.ReadByte();
                        v = (ushort)(b2 + b1 * 256);
                        var bpl3 = Helpers.GetBits(v, true);
                        b1 = fs.ReadByte();
                        b2 = fs.ReadByte();
                        v = (ushort)(b2 + b1 * 256);
                        var bpl4 = Helpers.GetBits(v, true);

                        // map pixels
                        for (var p = 0; p < 16; p++)
                        {
                            // get the color based on palette
                            var bv = bpl1[p] + (bpl2[p] * 2) + (bpl3[p] * 3) + (bpl4[p] * 8);
                            var xp = xo + p;
                            _pixelData[xp + y * _width] = (byte)bv;
                        }

                        xo += 16;
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
                        Color outColor;
                        if (bv == maskIndex)
                        {
                            outColor = rasters[y].Color;
                        }
                        else
                        {
                            outColor = ActivePalette[bv];
                        }

                        wbmp.SetPixel(x, y, outColor);
                    }
                }
            }
        }


    }
}
