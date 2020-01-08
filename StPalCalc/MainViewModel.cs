using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Microsoft.Win32;

namespace StPalCalc
{
    public class MainViewModel : BaseViewModel
    {
        public Action UpdateUi { get; set; }
        public Action<List<Color>> UpdateGradient { get; set; }
        public Action<Color[]> UpdatePreviewFade { get; set; }

        private string _previewText;
        public string PreviewText
        {
            get => _previewText;
            set { _previewText = value; OnPropertyChanged(); }
        }

        private string _generatedPalette;
        public string GeneratedPalette
        {
            get => _generatedPalette;
            set { _generatedPalette = value; OnPropertyChanged(); }
        }
        private UInt16[] _rawPalette = new UInt16[16];

        private string _startColor;
        public string StartColor
        {
            get => _startColor;
            set { _startColor = value; OnPropertyChanged(); }
        }

        private string _endColor;
        public string EndColor
        {
            get => _endColor;
            set { _endColor = value; OnPropertyChanged(); }
        }


        private string _rawPaletteString;
        public string RawPalette
        {
            get => _rawPaletteString;
            set
            {
                _rawPaletteString = value;
                OnPropertyChanged();
            }
        }

        private string _orgPaletteRgb;
        public string OrgPaletteRgb
        {
            get => _orgPaletteRgb;
            set
            {
                _orgPaletteRgb = value;
                OnPropertyChanged();
            }
        }

        private string _activeFilename;
        public string ActiveFilename
        {
            get => _activeFilename;
            set
            {
                _activeFilename = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand<string> OpenPi1Command { get; set; }
        public DelegateCommand<string> GenerateCommand { get; set; }
        public DelegateCommand<string> FadeToBlackCommand { get; set; }
        public DelegateCommand<string> FadeToWhiteCommand { get; set; }


        public string Get12BitRgb(int index)
        {
            return $"${_rawPalette[index]:X2}";
        }
        public Color GetRgb(int index)
        {
            var data = Helpers.GetBits(_rawPalette[index]);
            return Helpers.ToColor(Helpers.GetRValue(data), Helpers.GetGValue(data), Helpers.GetBValue(data));
        }

        private Color FromStString(string source)
        {
            var asHex = Convert.ToInt32(source, 16);
            var bits = Helpers.GetBits((ushort)asHex);
            var r = Helpers.GetRValue(bits);
            var g = Helpers.GetGValue(bits);
            var b = Helpers.GetBValue(bits);
            r = Helpers.RemapFrom12bitToRgb(r);
            g = Helpers.RemapFrom12bitToRgb(g);
            b = Helpers.RemapFrom12bitToRgb(b);

            return Color.FromRgb(r, g, b);
        }

        public MainViewModel()
        {
            OpenPi1Command = new DelegateCommand<string>(_activeFilename =>
            {
                var dlg = new OpenFileDialog();
                dlg.DefaultExt = ".pi1";
                dlg.Filter = "PI1 files|*.pi1;";
                dlg.ShowDialog();
                if (!string.IsNullOrEmpty(dlg.FileName))
                {
                    ActiveFilename = dlg.FileName;
                    ReadPalette();
                    UpdateUi?.Invoke();
                }
            });
            GenerateCommand = new DelegateCommand<string>(s =>
            {
                var startColor = FromStString(_startColor);
                var endColor = FromStString(_endColor);
                var data = Helpers.GetGradients(startColor, endColor, 16);

                var sb = new StringBuilder();
                foreach (var color in data)
                {
                    sb.Append(Helpers.ConvertFromRgbTo12Bit(color));
                    sb.Append(",");
                }

                GeneratedPalette = sb.ToString();
                UpdateGradient?.Invoke(data.ToList());
            });
            FadeToBlackCommand = new DelegateCommand<string>(b =>
            {
                var generatedColors = new Color[16 * 16];
                var stColors = new string[16 * 16];
                for (var i = 0; i < 16; i++)
                {
                    var c = _rawPalette[i].ToString("X2");
                    var startColor = FromStString(c);
                    var endColor = FromStString("0");
                    var data = Helpers.GetGradients(startColor, endColor, 16).ToList();

                    for (var j = 0; j < 16; j++)
                    {
                        var ofs = i + j * 16;
                        generatedColors[ofs] = data[j];
                        stColors[ofs] = Helpers.ConvertFromRgbTo12Bit(data[j]);
                    }
                }
                var sb = new StringBuilder();
                for (var y = 0; y < 16; y++)
                {
                    sb.Append("dc.l ");
                    for (var x = 0; x < 16; x++)
                    {
                        var ofs = x + y * 16;
                        sb.Append(stColors[ofs]);
                        if (x < 15) sb.Append(",");
                    }

                    sb.AppendLine("");
                }

                PreviewText = sb.ToString();
                UpdatePreviewFade?.Invoke(generatedColors);
            });
            FadeToWhiteCommand = new DelegateCommand<string>(b =>
            {
                var generatedColors = new Color[16 * 16];
                var stColors = new string[16 * 16];
                for (var i = 0; i < 16; i++)
                {
                    var c = _rawPalette[i].ToString("X2");
                    var startColor = FromStString(c);
                    var endColor = FromStString("777");
                    var data = Helpers.GetGradients(startColor, endColor, 16).ToList();

                    for (var j = 0; j < 16; j++)
                    {
                        var ofs = i + j * 16;
                        generatedColors[ofs] = data[j];
                        stColors[ofs] = Helpers.ConvertFromRgbTo12Bit(data[j]);
                    }
                }
                var sb = new StringBuilder();
                for (var y = 0; y < 16; y++)
                {
                    sb.Append("dc.l ");
                    for (var x = 0; x < 16; x++)
                    {
                        var ofs = x + y * 16;
                        sb.Append(stColors[ofs]);
                        if (x < 15) sb.Append(",");
                    }

                    sb.AppendLine("");
                }

                PreviewText = sb.ToString();
                UpdatePreviewFade?.Invoke(generatedColors);
            });
        }

        private void ReadPalette()
        {
            using (var fs = new FileStream(_activeFilename, FileMode.Open))
            {
                fs.Position = 2; // skip resolution
                for (var i = 0; i < 16; i++)
                {
                    var b1 = fs.ReadByte();
                    var b2 = fs.ReadByte();
                    UInt16 v = (UInt16)(b2 + b1 * 256);
                    var b = Helpers.GetBits(v);
                    Debug.WriteLine( Helpers.DumpBits(b) + "\nR" + Helpers.GetRValue(b) + " G" + Helpers.GetGValue(b) + " B" + Helpers.GetBValue(b));
                    //Debug.WriteLine(DumpBits(b));
                    //Debug.WriteLine(v.ToString("X2"));
                    //Debug.WriteLine(GetBits(v));
                    _rawPalette[i] = v;
                }
            }

            var sb = new StringBuilder();
            for (var i = 0; i < 16; i++)
            {
                sb.Append("$" + _rawPalette[i].ToString("X2"));
                if (i != 15) sb.Append(",");
            }
            RawPalette = sb.ToString();

            //sb.Clear();
            //for (var i = 0; i < 16; i++)
            //{
            //    sb.Append("$" + _rawPalette[i].ToString("X2"));
            //    if (i != 15) sb.Append(",");
            //}
            //RawPalette = sb.ToString();
        }
    }
}
