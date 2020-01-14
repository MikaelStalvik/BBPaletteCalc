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
using StPalCalc.PictureFormats;

namespace StPalCalc
{
    public class MainViewModel : BaseViewModel
    {
        public Action UpdateUiAction { get; set; }
        public Action<List<Color>> UpdateGradientAction { get; set; }
        public Action<Color[]> UpdatePreviewFadeAction { get; set; }
        public Action UpdateGradientPreviewAction { get; set; }
        public Action RebindAction { get; set; }
        public Action<PictureType> UpdatePictureAction { get; set; }
        public Action<List<Color>> UpdateHueColorsAction { get; set; }

        public IPicture PreviewPicture { get; set; }

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
                SelectedGradientColor = _selectedGradientItem != null ? Helpers.ConvertFromRgbTo12Bit(SelectedGradientItem.Color) : string.Empty;
            }
        }
        public List<GradientItem> SelectedGradientItems { get; set; }

        public void UpdateSelectedGradientColor(Color color)
        {
            if (SelectedGradientItem != null)
            {
                SelectedGradientItem.Color = color;
                SelectedGradientColor = Helpers.ConvertFromRgbTo12Bit(SelectedGradientItem.Color);
                GradientItems[SelectedGradientItem.Index].Color = SelectedGradientItem.Color;
                UpdateGradientPreviewAction?.Invoke();
                RebindAction?.Invoke();
            }
        }

        public void SetNewSelectedGradientColor(string data)
        {
            if (SelectedGradientItem == null) return;
            var inp = data.Replace("$", string.Empty);
            UpdateSelectedGradientColor(Helpers.FromStString(inp));
        }

        public ObservableCollection<GradientItem> GradientItems { get; private set; }

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

        private string _fadeToColor;
        public string FadeToColor
        {
            get => _fadeToColor;
            set { _fadeToColor = value; OnPropertyChanged(); }
        }

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

        private int _rasterIndexColor;

        public int RasterColorIndex
        {
            get => _rasterIndexColor;
            set
            {
                _rasterIndexColor = value;
                OnPropertyChanged();
                RefreshRasterPreviewImageCommand.Execute("");
            }
        }

        //public string PreviewFilename { get; set; }
        //public ushort[] PreviewPalette = new ushort[16];
        private ushort[] _rawPaletteOrg = new ushort[16];
        private ushort[] _rawPalette = new ushort[16];
        private ushort[] _rawPaletteHue = new ushort[16];

        public DelegateCommand<string> OpenPictureCommand { get; set; }
        public DelegateCommand<string> GenerateCommand { get; set; }
        public DelegateCommand<string> FadeToColorCommand { get; set; }
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
        public DelegateCommand<string> DeleteGradientItemCommand { get; set; }
        public DelegateCommand<string> ParseAsmGradientCommand { get; set; }
        public DelegateCommand<int> PickColorCommand { get; set; }
        public DelegateCommand<string> LoadPreviewImageCommand { get; set; }
        public DelegateCommand<string> RefreshRasterPreviewImageCommand { get; set; }
        public DelegateCommand<string> GenerateRastersCommand { get; set; }
        public DelegateCommand<HslSliderPayload> AdjustHueCommand { get; set; }
        public DelegateCommand<string> FadeFromPaletteToHueCommand { get; set; }
        public DelegateCommand<string> UpdatePalette1Command { get; set; }

        private (bool, string) SelectPictureFile()
        {
            //in a filter pattern with a semicolon. Example: \"Image files (*.bmp, *.jpg)|*.bmp;*.jpg|All files (*.*)|*.*\"'
            var dlg = new OpenFileDialog { DefaultExt = ".pi1", Filter = "PI1 files|*.pi1|IFF files|*.iff" };
            if (dlg.ShowDialog() == true)
            {
                return (true, dlg.FileName);
            }
            return (false, string.Empty);
        }

        public MainViewModel()
        {
            GradientItems = new ObservableCollection<GradientItem>();
            for (var i = 0; i < 200; i++)
            {
                GradientItems.Add(new GradientItem { Color = Colors.Black, Index = i});
            }
            SelectedDataType = 1;

            UpdatePalette1Command = new DelegateCommand<string>(s =>
            {
                var cleaned = s.Replace("$", "");
                var list = cleaned.Split(",");
                var i = 0;
                foreach (var color in list)
                {
                    var us = (ushort) Convert.ToInt32(color, 16);
                    _rawPalette[i] = us;
                    i++;
                }
                UpdateUiAction?.Invoke();
                UpdatePictureAction?.Invoke(PictureType.Picture1);
            });
            AdjustHueCommand = new DelegateCommand<HslSliderPayload>(payload =>
            {
                var newColors = new List<Color>();
                var index = 0;
                foreach (var uc in _rawPalette)
                {
                    var col = Helpers.FromStString(uc.ToString("X2"));
                    var hsl = new HSLColor(col.R, col.G, col.B);
                    hsl.Hue += payload.Hue;
                    hsl.Saturation += payload.Saturation;
                    hsl.Luminosity += payload.Lightness;
                    hsl.RgbParts(out var r, out var g, out var b);
                    var nc = Color.FromRgb(r, g, b);
                    newColors.Add(nc);
                    _rawPaletteHue[index] = (ushort)Convert.ToInt32(Helpers.ConvertFromRgbTo12Bit(nc, true), 16);
                    index++;
                }
                UpdateHueColorsAction?.Invoke(newColors);
            });
            GenerateRastersCommand = new DelegateCommand<string>(_ =>
            {
                var sb = new StringBuilder();
                var lineb = new StringBuilder();
                sb.AppendLine("; 200 items");
                sb.AppendLine("raster_data:");

                lineb.Append("\t" + SelectedDataTypePrefix + " ");

                var index = 0;
                foreach (var item in GradientItems)
                {
                    var isNewLine = ((index % 9) == 0) && index != 0;
                    if (isNewLine)
                    {
                        var line = lineb.ToString();
                        sb.AppendLine(line.Remove(line.Length - 1));
                        lineb.Clear();
                        lineb.Append("\t" + SelectedDataTypePrefix + " ");
                    }

                    lineb.Append(Helpers.ConvertFromRgbTo12Bit(item.Color));
                    lineb.Append(",");
                    index++;
                }

                var line2 = lineb.ToString();
                sb.AppendLine(line2.Remove(line2.Length - 1));

                PreviewText = sb.ToString();
            });
            DeleteGradientItemCommand = new DelegateCommand<string>( _ => {
                if (SelectedGradientItem != null)
                {
                    GradientItems.Remove(SelectedGradientItem);
                    GradientItems.Add(new GradientItem { Index = GradientItems.Count-1, Color = Colors.Black});
                    ReindexGradientItems();
                    SelectedGradientItem = null;
                }
            });
            RefreshRasterPreviewImageCommand = new DelegateCommand<string>(_ =>
            {
                UpdatePictureAction?.Invoke(PictureType.PreviewPicture);
            });
            LoadPreviewImageCommand = new DelegateCommand<string>(_ =>
            {
                var (res, filename) = SelectPictureFile();
                if (res)
                {
                    PreviewPicture = PictureFactory.ReadPicture(filename);
                    PreviewPicture.Load(filename);
                    UpdatePictureAction?.Invoke(PictureType.PreviewPicture); 
                }
            });
            PickColorCommand = new DelegateCommand<int>(index =>
            {
                var startColor = Colors.Red;
                switch (index)
                {
                    case 0: 
                        startColor = Helpers.FromStString(StartColor);
                        break;
                    case 1:
                        startColor = Helpers.FromStString(EndColor);
                        break;
                    case 2:
                        startColor = Helpers.FromStString(FadeToColor);
                        break;
                }
                var c = ColorPickerWindow.PickColor(startColor);
                if (c != null)
                {
                    var stColor = Helpers.ConvertFromRgbTo12Bit(c.Value, true);
                    switch (index)
                    {
                        case 0: 
                            StartColor = stColor;
                            break;
                        case 1:
                            EndColor = stColor;
                            break;
                        case 2:
                            FadeToColor = stColor;
                            break;
                    }
                }
            });
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

            OpenPictureCommand = new DelegateCommand<string>(_ =>
            {
                var (res, filename) = SelectPictureFile();
                if (res)
                {
                    ActiveFilename = filename;
                    RawPalette = ReadPalette(ActiveFilename, ref _rawPaletteOrg);
                    ReadPalette(ActiveFilename, ref _rawPalette);
                    UpdateUiAction?.Invoke();
                    UpdatePictureAction?.Invoke(PictureType.Picture1);
                }
            });
            GenerateCommand = new DelegateCommand<string>(s =>
            {
                var startColor = Helpers.FromStString(_startColor);
                var endColor = Helpers.FromStString(_endColor);
                var data = Helpers.GetGradients(startColor, endColor, 16).ToList();

                var sb = new StringBuilder();
                foreach (var color in data)
                {
                    sb.Append(Helpers.ConvertFromRgbTo12Bit(color));
                    sb.Append(",");
                }

                GeneratedPalette = sb.ToString();
                UpdateGradientAction?.Invoke(data.ToList());
            });
            FadeToColorCommand = new DelegateCommand<string>(b =>
            {
                GenerateFade(_rawPalette, FadeToColor);
            });
            FadeToBlackCommand = new DelegateCommand<string>(b =>
            {
                GenerateFade(_rawPalette, "0");
            });
            FadeToWhiteCommand = new DelegateCommand<string>(b =>
            {
                GenerateFade(_rawPalette, "FFF");
            });
            FadeFromPaletteToHueCommand = new DelegateCommand<string>(_ =>
            {
                GenerateFade(_rawPalette,_rawPaletteHue);
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
                        var lineData = line2.Split(",");
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

        private void ReindexGradientItems()
        {
            for (var i = 0; i < GradientItems.Count; i++)
            {
                GradientItems[i].Index = i;
            }
            UpdateGradientPreviewAction?.Invoke();
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
                sb.Append("\t" + SelectedDataTypePrefix);
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
        private void GenerateFade(ushort[] palette, ushort[] endPalette)
        {
            var generatedColors = new Color[16 * 16];
            var stColors = new string[16 * 16];
            for (var i = 0; i < 16; i++)
            {
                var c = palette[i].ToString("X2");
                var startColor = Helpers.FromStString(c);
                c = endPalette[i].ToString("X2");
                var endColor = Helpers.FromStString(c);
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
                sb.Append("\t" + SelectedDataTypePrefix);
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

        public void SetPaletteValue(ushort stColor, int index)
        {
            _rawPalette[index] = stColor;
            RawPalette = UpdateRawPalette(_rawPalette);
            UpdateUiAction?.Invoke();
        }
        public string Get12BitRgbFromPalette1(int index)
        {
            return $"${_rawPalette[index]:X2}";
        }
        public Color GetRgbFromPalette1(int index)
        {
            var data = Helpers.GetBits(_rawPalette[index]);
            return Helpers.ToColor(Helpers.GetRValue(data), Helpers.GetGValue(data), Helpers.GetBValue(data));
        }

        private string UpdateRawPalette(ushort[] target)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < 16; i++)
            {
                sb.Append("$" + target[i].ToString("X2"));
                if (i != 15) sb.Append(",");
            }
            return sb.ToString();
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
            return UpdateRawPalette(target);
        }

        public void RenderPi1(string filename, Image image, PictureType pictureType, bool useRaster = false)
        {
            byte[] pd = new byte[320*200];
            var wbmp = BitmapFactory.New(320, 200);
            image.Source = wbmp;
            using (wbmp.GetBitmapContext())
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
                            ushort stColor = 0;
                            Color outCol;

                            if (useRaster && bv == _rasterIndexColor)
                            {
                                outCol = GradientItems[y].Color;
                            }
                            else
                            {
                                switch (pictureType)
                                {
                                    case PictureType.Picture1:
                                        stColor = _rawPalette[bv];
                                        break;
                                    //case PictureType.PreviewPicture:
                                    //    stColor = PreviewPalette[bv];
                                    //    break;
                                    case PictureType.Picture1Hue:
                                        stColor = _rawPaletteHue[bv];
                                        break;
                                }
                                outCol = Helpers.FromStString(stColor.ToString("X2"));
                            }

                            wbmp.SetPixel((xo + p), y, outCol);
                            pd[(xo + p) + (y * 200)] = (byte)bv;
                        }

                        xo += 16;
                    }
                }
            }
        }

        public void ResetPalette()
        {
            for (var i = 0; i < _rawPaletteOrg.Length; i++)
            {
                _rawPalette[i] = _rawPaletteOrg[i];
                RawPalette = UpdateRawPalette(_rawPalette);
                UpdateUiAction?.Invoke();
                UpdatePictureAction?.Invoke(PictureType.Picture1);
            }
        }

    }
}
