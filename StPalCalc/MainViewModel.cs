using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace StPalCalc
{
    public class GradientItem : BaseViewModel
    {
        public int Index { get; set; }

        private Color _color;

        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                OnPropertyChanged();
            }
        }
    }
    public class MainViewModel : BaseViewModel
    {
        public Action UpdateUiAction { get; set; }
        public Action<List<Color>> UpdateGradientAction { get; set; }
        public Action<Color[]> UpdatePreviewFadeAction { get; set; }
        public Action UpdateGradientPreviewAction { get; set; }
        public Action RebindAction { get; set; }
        public Action<int> UpdatePictureAction { get; set; }

        private GradientItem ClipboardItem { get; set; }
        public int StartGradientIndex { get; set; }
        public int EndGradientIndex { get; set; }
        private GradientItem _selectedGradientItem;
        public GradientItem SelectedGradientItem
        {
            get => _selectedGradientItem;
            set
            {
                _selectedGradientItem = value;
                OnPropertyChanged();
                if (_selectedGradientItem != null)
                {
                    SelectedGradientColor = Helpers.ConvertFromRgbTo12Bit(SelectedGradientItem.Color);
                }
                else
                {
                    SelectedGradientColor = string.Empty;
                }
            }
        }
        public List<GradientItem> SelectedGradientItems { get; set; }

        public void UpdateSelectedGradientColor(Color color)
        {
            if (SelectedGradientItem == null)
            {
                SelectedGradientItem.Color = color;
                SelectedGradientColor = Helpers.ConvertFromRgbTo12Bit(SelectedGradientItem.Color);
            }
        }

        public void SetNewSelectedGradientColor(string data)
        {
            if (SelectedGradientItem == null) return;
            var inp = data.Replace("$", string.Empty);
            UpdateSelectedGradientColor(Helpers.FromStString(inp));
        }

        public ObservableCollection<GradientItem> GradientItems { get; set; }

        private string _selectedGradientColor;
        public string SelectedGradientColor
        {
            get => _selectedGradientColor;
            set
            {
                _selectedGradientColor = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> DataTypes = new ObservableCollection<string>
        {
            "Byte (dc.b)",
            "Word (dc.w)",
            "Long (dc.l)"
        };

        private int _selectedDataType;
        public int SelectedDataType
        {
            get => _selectedDataType;
            set
            {
                _selectedDataType = value;
                OnPropertyChanged();
            }
        }

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
        public DelegateCommand<string> ChangeGradientColorCommand { get; set; }
        public DelegateCommand<string> CopyPreviousCommand { get; set; }
        public DelegateCommand<string> CopyNextCommand { get; set; }
        public DelegateCommand<string> GenerateGradientCommand { get; set; }
        public DelegateCommand<string> LoadGradientCommand { get; set; }
        public DelegateCommand<string> SaveGradientCommand { get; set; }
        public DelegateCommand<string> CopyItemCommand { get; set; }
        public DelegateCommand<string> PasteItemCommand { get; set; }
        public DelegateCommand<string> ParseAsmGradientCommand { get; set; }

        public MainViewModel()
        {
            GradientItems = new ObservableCollection<GradientItem>();
            for (var i = 0; i < 200; i++)
            {
                GradientItems.Add(new GradientItem { Color = Colors.Black, Index = i});
            }
            CopyItemCommand = new DelegateCommand<string>(_ =>
            {
                ClipboardItem = new GradientItem { Color = SelectedGradientItem.Color };
            });
            PasteItemCommand = new DelegateCommand<string>(_ =>
            {
                SelectedGradientItem.Color = ClipboardItem.Color;
                if (SelectedGradientItems.Count == 1)
                {
                    GradientItems[SelectedGradientItem.Index].Color = ClipboardItem.Color;
                }
                else
                {
                    foreach (var item in SelectedGradientItems)
                    {
                        GradientItems[item.Index].Color = ClipboardItem.Color;
                    }
                }
                UpdateGradientPreviewAction?.Invoke();
                RebindAction?.Invoke();
            });
            LoadGradientCommand = new DelegateCommand<string>(_ =>
            {
                var dlg = new OpenFileDialog { DefaultExt = ".grd", Filter = "Gradient files|*.grd;" };
                dlg.ShowDialog();
                if (!string.IsNullOrEmpty(dlg.FileName))
                {
                    var json = File.ReadAllText(dlg.FileName);
                    var data = JsonConvert.DeserializeObject<List<GradientItem>>(json);
                    GradientItems = new ObservableCollection<GradientItem>();
                    var i = 0;
                    foreach (var item in data)
                    {
                        GradientItems.Add(new GradientItem { Color = item.Color, Index = i });
                        i++;
                    }
                    UpdateGradientPreviewAction?.Invoke();
                    RebindAction?.Invoke();
                }
            });
            SaveGradientCommand = new DelegateCommand<string>(s =>
            {
                var dlg = new SaveFileDialog { DefaultExt = ".grd", Filter = "Gradient files|*.grd;" };
                dlg.ShowDialog();
                if (!string.IsNullOrEmpty(dlg.FileName))
                {
                    var json = JsonConvert.SerializeObject(GradientItems.ToList());
                    File.WriteAllText(dlg.FileName, json);
                }
            });
            GenerateGradientCommand = new DelegateCommand<string>(s =>
            {
                var startColor = GradientItems[StartGradientIndex].Color;
                var endColor = GradientItems[EndGradientIndex].Color;
                var data = Helpers.GetGradients(startColor, endColor, EndGradientIndex-StartGradientIndex).ToList();
                var j = 0;
                for (var i = StartGradientIndex; i < EndGradientIndex; i++)
                {
                    GradientItems[i].Color = data[j];
                    j++;
                }
                UpdateGradientPreviewAction?.Invoke();
            });
            CopyPreviousCommand = new DelegateCommand<string>(s =>
            {
                if (SelectedGradientItem == null) return;
                if (SelectedGradientItem.Index == 0) return;
                var previous = GradientItems[SelectedGradientItem.Index - 1];
                SelectedGradientItem.Color = previous.Color;
                UpdateGradientPreviewAction?.Invoke();
            });

            OpenPic1Command = new DelegateCommand<string>(_ =>
            {
                var dlg = new OpenFileDialog { DefaultExt = ".pi1", Filter = "PI1 files|*.pi1;" };
                dlg.ShowDialog();
                if (!string.IsNullOrEmpty(dlg.FileName))
                {
                    ActiveFilename = dlg.FileName;
                    RawPalette = ReadPalette(ActiveFilename, ref _rawPalette1);
                    UpdateUiAction?.Invoke();
                    UpdatePictureAction?.Invoke(0);
                }
            });
            OpenPic2Command = new DelegateCommand<string>(_ =>
            {
                var dlg = new OpenFileDialog { DefaultExt = ".pi1", Filter = "PI1 files|*.pi1;" };
                dlg.ShowDialog();
                if (!string.IsNullOrEmpty(dlg.FileName))
                {
                    ActiveFilename2 = dlg.FileName;
                    RawPalette2 = ReadPalette(ActiveFilename2, ref _rawPalette2);
                    UpdateUiAction?.Invoke();
                    UpdatePictureAction?.Invoke(1);
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
                GenerateFade(UsePicture == 0 ? _rawPalette1 : _rawPalette2, "0");
            });
            FadeToWhiteCommand = new DelegateCommand<string>(b =>
            {
                GenerateFade(UsePicture == 0 ? _rawPalette1 : _rawPalette2, "FFF");
            });
            ChangeGradientColorCommand = new DelegateCommand<string>(s =>
            {
                UpdateGradientPreviewAction?.Invoke();
            });
            ParseAsmGradientCommand = new DelegateCommand<string>(s =>
            {

                if (Clipboard.ContainsText())
                {
                    var errors = false;
                    var colorData = new List<Color>();
                    var ctext = Clipboard.GetText();
                    var lines = ctext.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var line2 = line.Replace("dc.w", string.Empty, StringComparison.CurrentCultureIgnoreCase).Trim();
                        var lineData = line2.Split(",", StringSplitOptions.None);
                        foreach (var item in lineData)
                        {
                            var cleanedItem = item.Replace("$", "");
                            try
                            {
                                var converted = Convert.ToInt32(cleanedItem, 16);
                                colorData.Add(Helpers.StColorFromInt(converted));
                            }
                            catch
                            {
                                Debug.WriteLine($"Could not parse: {cleanedItem}");
                                errors = true;
                            }
                        }
                    }

                    for (var i = 0; i < colorData.Count; i++)
                    {
                        if (i == 200) break;
                        GradientItems[i].Color = colorData[i];
                    }
                    UpdateGradientPreviewAction?.Invoke();
                    RebindAction?.Invoke();
                    if (errors) MessageBox.Show("Some data could not be parsed");
                }
                else
                {
                    MessageBox.Show("Clipboard does not contain any text");
                }
            });
        }

        private string SelectedDataTypePrefix
        {
            get
            {
                switch (SelectedDataType)
                {
                    case 0: return "dc.b ";
                    case 1: return "dc.w ";
                    case 2: return "dc.l ";
                    default:
                        return "dc.w ";
                }
            }
        }
        private void GenerateFade(ushort[] palette, string endColorStr)
        {
            var generatedColors = new Color[16 * 16];
            var stColors = new string[16 * 16];
            for (var i = 0; i < 16; i++)
            {
                var c = palette[i].ToString("X2");
                var startColor = Helpers.FromStString(c);
                var endColor = Helpers.FromStString(endColorStr);
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
                sb.Append(SelectedDataTypePrefix);
                for (var x = 0; x < 16; x++)
                {
                    var ofs = x + y * 16;
                    sb.Append(stColors[ofs]);
                    if (x < 15) sb.Append(",");
                }
                sb.AppendLine(string.Empty);
            }

            PreviewText = sb.ToString();
            UpdatePreviewFadeAction?.Invoke(generatedColors);
        }

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

        public void RenderPi1(string filename, Image image, bool usePalette1)
        {
            var writeableBmp = BitmapFactory.New(320, 200);
            image.Source = writeableBmp;
            using (writeableBmp.GetBitmapContext())
            {
                using var fs = new FileStream(filename, FileMode.Open);
                fs.Position += 34; // skip res and palette
                for (var y = 0; y < 200; y++)
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
                            var bv = bpl1[p] + (bpl2[p] * 2) + (bpl3[p] * 3) + (bpl4[p]*8);
                            var stColor = usePalette1 ? _rawPalette1[bv] : _rawPalette2[bv];
                            var c = Helpers.FromStString(stColor.ToString("X2"));

                            writeableBmp.SetPixel((xo + p), y, c);
                        }

                        xo += 16;
                    }
                }
            }
        }

    }
}
