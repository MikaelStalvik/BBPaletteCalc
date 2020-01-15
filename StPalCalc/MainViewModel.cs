using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using Newtonsoft.Json;


namespace StPalCalc
{
    public class MainViewModel : BaseViewModel
    {
        public Action<bool> UpdateUiAction { get; set; }
        public Action<List<Color>> UpdateGradientAction { get; set; }
        public Action<Color[]> UpdatePreviewFadeAction { get; set; }
        public Action UpdateGradientPreviewAction { get; set; }
        public Action RebindAction { get; set; }
        public Action<PictureType> UpdatePictureAction { get; set; }
        public Action<List<Color>> UpdateHueColorsAction { get; set; }

        public IPicture PreviewPicture { get; set; }
        public IPicture ActivePicture { get; set; }
        public Color[] HuePalette { get; private set; }

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

        public readonly ObservableCollection<string> DataTypes = new ObservableCollection<string>
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

        public readonly ObservableCollection<string> Platforms = new ObservableCollection<string>
        {
            "Atari Ste",
            "Amiga"
        };
        private int _selectedPlatform;
        public int SelectedPlatform
        {
            get => _selectedPlatform;
            set
            {
                _selectedPlatform = value;
                Helpers.ActivePlatform = (PlatformTypes) _selectedPlatform;
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


        private string _activePaletteString;
        public string ActivePaletteString
        {
            get => _activePaletteString;
            set
            {
                _activePaletteString = value;
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

        public DelegateCommand<string> OpenPictureCommand { get; set; }
        public DelegateCommand<string> GenerateCommand { get; set; }
        public DelegateCommand<string> FadeToColorCommand { get; set; }
        public DelegateCommand<string> FadeToBlackCommand { get; set; }
        public DelegateCommand<string> FadeFromPaletteToHueCommand { get; set; }
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
        public DelegateCommand<string> UpdatePalette1Command { get; set; }

        private (bool, string) SelectPictureFile()
        {
            var dlg = new OpenFileDialog { DefaultExt = ".pi1", Filter = "All files|*.iff;*.pi1|PI1 files|*.pi1|IFF files|*.iff" };
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
                if (ActivePicture == null) return;
                var cleaned = s.Replace("$", "");
                var list = cleaned.Split(",");
                var i = 0;
                foreach (var color in list)
                {
                    var us = (ushort)Convert.ToInt32(color, 16);
                    ActivePicture.ActivePalette[i++] = Helpers.StColorFromInt(us);
                }
                UpdateUiAction?.Invoke(false);
                UpdatePictureAction?.Invoke(PictureType.Picture1);
            });
            AdjustHueCommand = new DelegateCommand<HslSliderPayload>(payload =>
            {
                if (ActivePicture == null) return;
                var newColors = new List<Color>();
                foreach (var color in ActivePicture.OriginalPalette)
                {
                    var hsl = new HSLColor(color.R, color.G, color.B);
                    hsl.Hue += payload.Hue;
                    hsl.Saturation += payload.Saturation;
                    hsl.Luminosity += payload.Lightness;
                    hsl.RgbParts(out var r, out var g, out var b);
                    var nc = Color.FromRgb(r, g, b);
                    newColors.Add(nc);
                }
                HuePalette = newColors.ToArray();
                ActivePicture.ActivePalette = newColors.ToArray();
                UpdateHueColorsAction?.Invoke(newColors);
            });
            GenerateRastersCommand = new DelegateCommand<string>(_ =>
            {
                var sb = new StringBuilder();
                var lineBuilder = new StringBuilder();
                sb.AppendLine("; 200 items");
                sb.AppendLine("raster_data:");

                lineBuilder.Append("\t" + SelectedDataTypePrefix + " ");

                var index = 0;
                foreach (var item in GradientItems)
                {
                    var isNewLine = ((index % 9) == 0) && index != 0;
                    if (isNewLine)
                    {
                        var line = lineBuilder.ToString();
                        sb.AppendLine(line.Remove(line.Length - 1));
                        lineBuilder.Clear();
                        lineBuilder.Append("\t" + SelectedDataTypePrefix + " ");
                    }

                    lineBuilder.Append(Helpers.ConvertFromRgbTo12Bit(item.Color));
                    lineBuilder.Append(",");
                    index++;
                }

                var line2 = lineBuilder.ToString();
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
                    PreviewPicture = PictureFactory.GetPicture(filename);
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
                    ActivePicture = PictureFactory.GetPicture(filename);
                    if (ActivePicture == null) return;
                    ActiveFilename = filename;
                    ActivePaletteString = Helpers.RgbPaletteTo12BitString(ActivePicture.ActivePalette);
                    UpdatePictureAction?.Invoke(PictureType.Picture1);
                    UpdateUiAction?.Invoke(true);
                }
            });
            GenerateCommand = new DelegateCommand<string>(s =>
            {
                var startColor = Helpers.FromStString(_startColor);
                var endColor = Helpers.FromStString(_endColor);
                var data = Helpers.GetGradients(startColor, endColor, Constants.GRADIENT_STEPS).ToList();

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
                if (ActivePicture?.ActivePalette == null) return;
                GenerateFade(ActivePicture.ActivePalette, FadeToColor);
            });
            FadeToBlackCommand = new DelegateCommand<string>(b =>
            {
                if (ActivePicture?.ActivePalette == null) return;
                GenerateFade(ActivePicture.ActivePalette, "0");
            });
            FadeToWhiteCommand = new DelegateCommand<string>(b =>
            {
                if (ActivePicture?.ActivePalette == null) return;
                GenerateFade(ActivePicture.ActivePalette, "FFF");
            });
            FadeFromPaletteToHueCommand = new DelegateCommand<string>(_ =>
            {
                if (ActivePicture?.OriginalPalette == null || HuePalette == null) return;
                GenerateFade(ActivePicture.OriginalPalette, ActivePicture.ActivePalette);
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
                    var clipboardText = Clipboard.GetText();
                    var lines = clipboardText.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
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
        private void GenerateFade(Color[] fromPalette, string endColorStr)
        {
            var generatedColors = new Color[fromPalette.Length * Constants.FADE_STEPS]; // 16 * 16
            var stColors = new string[fromPalette.Length * Constants.FADE_STEPS]; // 16 * 16
            for (var i = 0; i < fromPalette.Length; i++)
            {
                var startColor = fromPalette[i];
                var endColor = Helpers.FromStString(endColorStr);
                var data = Helpers.GetGradients(startColor, endColor, Constants.FADE_STEPS).ToList();

                for (var j = 0; j < Constants.FADE_STEPS; j++)
                {
                    var ofs = i + j * fromPalette.Length;
                    generatedColors[ofs] = data[j];
                    stColors[ofs] = Helpers.ConvertFromRgbTo12Bit(data[j]);
                }
            }
            var sb = new StringBuilder();
            for (var y = 0; y < Constants.FADE_STEPS; y++)
            {
                sb.Append("\t" + SelectedDataTypePrefix);
                for (var x = 0; x < fromPalette.Length; x++)
                {
                    var ofs = x + y * fromPalette.Length;
                    sb.Append(stColors[ofs]);
                    if (x < fromPalette.Length-1) sb.Append(",");
                }
                sb.AppendLine(string.Empty);
            }

            PreviewText = sb.ToString();
            UpdatePreviewFadeAction?.Invoke(generatedColors);
        }
        private void GenerateFade(Color[] fromPalette, Color[] toPalette)
        {
            var generatedColors = new Color[fromPalette.Length * Constants.FADE_STEPS];
            var stColors = new string[fromPalette.Length * Constants.FADE_STEPS];
            for (var i = 0; i < fromPalette.Length; i++)
            {
                var startColor = fromPalette[i];
                var endColor = toPalette[i];
                var data = Helpers.GetGradients(startColor, endColor, Constants.FADE_STEPS).ToList();

                for (var j = 0; j < Constants.FADE_STEPS; j++)
                {
                    var ofs = i + j * fromPalette.Length;
                    generatedColors[ofs] = data[j];
                    stColors[ofs] = Helpers.ConvertFromRgbTo12Bit(data[j]);
                }
            }
            var sb = new StringBuilder();
            for (var y = 0; y < Constants.FADE_STEPS; y++)
            {
                sb.Append("\t" + SelectedDataTypePrefix);
                for (var x = 0; x < fromPalette.Length; x++)
                {
                    var ofs = x + y * fromPalette.Length;
                    sb.Append(stColors[ofs]);
                    if (x < fromPalette.Length-1) sb.Append(",");
                }
                sb.AppendLine(string.Empty);
            }

            PreviewText = sb.ToString();
            UpdatePreviewFadeAction?.Invoke(generatedColors);
        }

        public void SetPaletteValue(Color color, int index)
        {
            ActivePicture.ActivePalette[index] = color;
            ActivePaletteString = Helpers.RgbPaletteTo12BitString(ActivePicture.ActivePalette);
            UpdateUiAction?.Invoke(false);
        }

        public void ResetPalette()
        {
            if (ActivePicture == null) return;
            ActivePicture.ActivePalette = ActivePicture.OriginalPalette.ToArray();
            ActivePaletteString = Helpers.RgbPaletteTo12BitString(ActivePicture.ActivePalette);
            UpdateUiAction?.Invoke(true);
            UpdatePictureAction?.Invoke(PictureType.Picture1);
        }

        public void UpdateCurrentPicture()
        {
            if (ActivePicture == null) return;
            ActivePaletteString = Helpers.RgbPaletteTo12BitString(ActivePicture.ActivePalette);
            UpdatePictureAction?.Invoke(PictureType.Picture1);
            UpdateUiAction?.Invoke(true);
        }

    }
}
