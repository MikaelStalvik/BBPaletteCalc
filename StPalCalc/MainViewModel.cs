using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Microsoft.Win32;

namespace StPalCalc
{
    public class MainViewModel : BaseViewModel
    {
        public Action UpdateUiAction { get; set; }
        public Action<List<Color>> UpdateGradientAction { get; set; }
        public Action<Color[]> UpdatePreviewFadeAction { get; set; }

        private int _usePicture;

        public int UsePicture
        {
            get => _usePicture;
            set
            {
                _usePicture = value;
                OnPropertyChanged();
            }
        }
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
        private ushort[] _rawPalette1 = new ushort[16];
        private ushort[] _rawPalette2 = new ushort[16];

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

        private string _rawPaletteString2;
        public string RawPalette2
        {
            get => _rawPaletteString2;
            set
            {
                _rawPaletteString2 = value;
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
        private string _activeFilename2;
        public string ActiveFilename2
        {
            get => _activeFilename2;
            set
            {
                _activeFilename2 = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand<string> OpenPic1Command { get; set; }
        public DelegateCommand<string> OpenPic2Command { get; set; }
        public DelegateCommand<string> GenerateCommand { get; set; }
        public DelegateCommand<string> FadeToBlackCommand { get; set; }
        public DelegateCommand<string> FadeToWhiteCommand { get; set; }


        public string Get12BitRgbFromPalette1(int index)
        {
            return $"${_rawPalette1[index]:X2}";
        }
        public string Get12BitRgbFromPalette2(int index)
        {
            return $"${_rawPalette2[index]:X2}";
        }
        public Color GetRgbFromPalette1(int index)
        {
            var data = Helpers.GetBits(_rawPalette1[index]);
            return Helpers.ToColor(Helpers.GetRValue(data), Helpers.GetGValue(data), Helpers.GetBValue(data));
        }
        public Color GetRgbFromPalette2(int index)
        {
            var data = Helpers.GetBits(_rawPalette2[index]);
            return Helpers.ToColor(Helpers.GetRValue(data), Helpers.GetGValue(data), Helpers.GetBValue(data));
        }



        public MainViewModel()
        {
            OpenPic1Command = new DelegateCommand<string>(_activeFilename =>
            {
                var dlg = new OpenFileDialog {DefaultExt = ".pi1", Filter = "PI1 files|*.pi1;"};
                dlg.ShowDialog();
                if (!string.IsNullOrEmpty(dlg.FileName))
                {
                    ActiveFilename = dlg.FileName;
                    RawPalette = ReadPalette(ActiveFilename, ref _rawPalette1);
                    UpdateUiAction?.Invoke();
                }
            });
            OpenPic2Command = new DelegateCommand<string>(_activeFilename =>
            {
                var dlg = new OpenFileDialog {DefaultExt = ".pi1", Filter = "PI1 files|*.pi1;"};
                dlg.ShowDialog();
                if (!string.IsNullOrEmpty(dlg.FileName))
                {
                    ActiveFilename2 = dlg.FileName;
                    RawPalette2 = ReadPalette(ActiveFilename2, ref _rawPalette2);
                    UpdateUiAction?.Invoke();
                }
            });
            GenerateCommand = new DelegateCommand<string>(s =>
            {
                var startColor = Helpers.FromStString(_startColor);
                var endColor = Helpers.FromStString(_endColor);
                var data = Helpers.GetGradients(startColor, endColor, 16);

                var sb = new StringBuilder();
                foreach (var color in data)
                {
                    sb.Append(Helpers.ConvertFromRgbTo12Bit(color));
                    sb.Append(",");
                }

                GeneratedPalette = sb.ToString();
                UpdateGradientAction?.Invoke(data.ToList());
            });
            FadeToBlackCommand = new DelegateCommand<string>(b =>
            {
                var generatedColors = new Color[16 * 16];
                var stColors = new string[16 * 16];
                for (var i = 0; i < 16; i++)
                {
                    var c = UsePicture == 0 ? _rawPalette1[i].ToString("X2") : _rawPalette2[i].ToString("X2");
                    var startColor = Helpers.FromStString(c);
                    var endColor = Helpers.FromStString("0");
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
                UpdatePreviewFadeAction?.Invoke(generatedColors);
            });
            FadeToWhiteCommand = new DelegateCommand<string>(b =>
            {
                var generatedColors = new Color[16 * 16];
                var stColors = new string[16 * 16];
                for (var i = 0; i < 16; i++)
                {
                    var c = UsePicture == 0 ? _rawPalette1[i].ToString("X2") : _rawPalette2[i].ToString("X2");
                    var startColor = Helpers.FromStString(c);
                    var endColor = Helpers.FromStString("777");
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
                UpdatePreviewFadeAction?.Invoke(generatedColors);
            });
        }

        private string ReadPalette(string filename, ref ushort[] target)
        {
            using (var fs = new FileStream(filename, FileMode.Open))
            {
                fs.Position = 2; // skip resolution
                for (var i = 0; i < 16; i++)
                {
                    var b1 = fs.ReadByte();
                    var b2 = fs.ReadByte();
                    var v = (ushort)(b2 + b1 * 256);
                    target[i] = v;
                }
            }

            var sb = new StringBuilder();
            for (var i = 0; i < 16; i++)
            {
                sb.Append("$" + target[i].ToString("X2"));
                if (i != 15) sb.Append(",");
            }

            return sb.ToString();
        }
    }
}
