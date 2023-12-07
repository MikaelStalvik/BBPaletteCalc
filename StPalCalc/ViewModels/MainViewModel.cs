using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BBPalCalc.Interfaces;
using BBPalCalc.PictureFormats;
using BBPalCalc.Types;
using BBPalCalc.Util;
using BmpSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace BBPalCalc.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        //private const int RASTER_NUM = 256;

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
                SelectedGradientColor = _selectedGradientItem != null ? Helpers.Globals.ActivePlatform.ColorToString(SelectedGradientItem.Color) : string.Empty;
            }
        }
        public List<GradientItem> SelectedGradientItems { get; set; }

        public void UpdateSelectedGradientColor(Color color)
        {
            if (SelectedGradientItem != null)
            {
                SelectedGradientItem.Color = color;
                SelectedGradientColor = Helpers.Globals.ActivePlatform.ColorToString(SelectedGradientItem.Color);
                GradientItems[SelectedGradientItem.Index].Color = SelectedGradientItem.Color;
                UpdateGradientPreviewAction?.Invoke();
                RebindAction?.Invoke();
            }
        }

        public void SetNewSelectedGradientColor(string data)
        {
            if (SelectedGradientItem == null) return;
            var inp = data.Replace("$", string.Empty);
            UpdateSelectedGradientColor(Helpers.Globals.ActivePlatform.ToRgb(inp.ToHex()));
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

        private bool _pictureLoaded;
        public bool PictureLoaded
        {
            get => _pictureLoaded;
            set { _pictureLoaded = value; OnPropertyChanged(); }
        }
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
            "Amiga",
            "Atari St"
        };
        private int _selectedPlatform;
        public int SelectedPlatform
        {
            get => _selectedPlatform;
            set
            {
                _selectedPlatform = value;
                OnPropertyChanged();
            }
        }

        public readonly ObservableCollection<string> NumberOfRasters = new ObservableCollection<string>
        {
            "200",
            "256",
            "312"
        };
        private int _numRasters;
        public int NumRasters
        {
            get => _numRasters;
            set { _numRasters = value; OnPropertyChanged(); }
        }

        private int _fadeSteps;
        public int FadeSteps
        {
            get => _fadeSteps;
            set
            {
                _fadeSteps = value;
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

        private string _pictureProperties;
        public string PictureProperties
        {
            get => _pictureProperties;
            set { _pictureProperties = value; OnPropertyChanged(); }
        }

        public DelegateCommand<string> OpenPictureCommand { get; set; }
        public DelegateCommand<string> GenerateGradientCommand { get; set; }
        public DelegateCommand<string> FadeToColorCommand { get; set; }
        public DelegateCommand<string> FadeFromColorCommand { get; set; }
        public DelegateCommand<string> FadeToBlackCommand { get; set; }
        public DelegateCommand<string> FadeFromBlackCommand { get; set; }
        public DelegateCommand<string> FadeFromPaletteToHueCommand { get; set; }
        public DelegateCommand<string> FadeToWhiteCommand { get; set; }
        public DelegateCommand<string> FadeFromWhiteCommand { get; set; }
        public DelegateCommand<string> ChangeGradientColorCommand { get; set; }
        public DelegateCommand<string> CopyPreviousCommand { get; set; }
        public DelegateCommand<string> CopyNextCommand { get; set; }
        public DelegateCommand<string> GenerateGradientRasterCommand { get; set; }
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
        public DelegateCommand<string> UpdatePaletteCommand { get; set; }
        public DelegateCommand<string> SavePngCommand { get; set; }
        public DelegateCommand<string> SavePi1Command { get; set; }
        public DelegateCommand<string> ImportPaletteToRasterCommand { get; set; }
        public DelegateCommand<string> DrawCurveCommand { get; set; }
        public DelegateCommand<string> SwapColorsCommand { get; set; }

        public Image CurveImage { get; set; }
        public Image PreviewImage { get; set; }
        private (bool, string) SelectPictureFile()
        {
            var dlg = new OpenFileDialog { DefaultExt = ".pi1", Filter = "All files|*.iff;*.pi1|PI1 files|*.pi1|IFF files|*.iff" };
            if (dlg.ShowDialog() == true)
            {
                return (true, dlg.FileName);
            }
            return (false, string.Empty);
        }

        private void SaveUsingEncoder(FrameworkElement visual, string fileName, BitmapEncoder encoder)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            BitmapFrame frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);

            using (var stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }
        public MainViewModel()
        {
            GradientItems = new ObservableCollection<GradientItem>();
            _numRasters = 200;
            for (var i = 0; i < _numRasters; i++)
            {
                GradientItems.Add(new GradientItem { Color = Colors.Black, Index = i});
            }
            SelectedDataType = 1;
            FadeSteps = 16;

            ImportPaletteToRasterCommand = new DelegateCommand<string>(s =>
            {
                var (res, filename) = SelectPictureFile();
                if (res)
                {
                    var pic = PictureFactory.GetPicture(filename);
                    for (var i = 0; i < pic.ActivePalette.Length; i++)
                    {
                        GradientItems[i].Color = pic.ActivePalette[i];
                    }
                    UpdateGradientPreviewAction?.Invoke();
                }
            });
            SavePngCommand = new DelegateCommand<string>(s =>
            {
                if (PreviewImage == null) return;
                var dlg = new SaveFileDialog {DefaultExt = ".png"};
                if (dlg.ShowDialog() == true)
                {
                    var encoder = new PngBitmapEncoder();
                    SaveUsingEncoder(PreviewImage, dlg.FileName, encoder);
                }
            });
            SavePi1Command = new DelegateCommand<string>(s =>
            {
                var dlg = new SaveFileDialog { DefaultExt = ".bmp" };
                if (dlg.ShowDialog() == true)
                {
                    var palette = Helpers.PaletteTo12BitPalette(ActivePicture.OriginalPalette);
                    var (w, h) = ActivePicture.GetDimensions;
                    Pi1Picture.Save(dlg.FileName, palette, ActivePicture.Pixels, w, h);
                }
            });
            UpdatePaletteCommand = new DelegateCommand<string>(s =>
            {
                if (ActivePicture == null) return;
                var cleaned = s.Replace("$", "");
                var list = cleaned.Split(",");
                var i = 0;
                foreach (var item in list)
                {
                    ActivePicture.ActivePalette[i++] = Helpers.Globals.ActivePlatform.ToRgb(item.ToHex());// ColorFromString(item);
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
                sb.AppendLine($"; {GradientItems.Count} items");
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

                    lineBuilder.Append("$");
                    lineBuilder.Append(Helpers.Globals.ActivePlatform.ColorToString(item.Color));
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
                        startColor = Helpers.Globals.ActivePlatform.ToRgb(StartColor.ToHex());
                        break;
                    case 1:
                        startColor = Helpers.Globals.ActivePlatform.ToRgb(EndColor.ToHex());
                        break;
                    case 2:
                        startColor = Helpers.Globals.ActivePlatform.ToRgb(FadeToColor.ToHex());
                        break;
                }
                var c = ColorPickerWindow.PickColor(startColor);
                if (c != null)
                {
                    var stColor = Helpers.Globals.ActivePlatform.ColorToString(c.Value);
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
                        if (i == _numRasters) break;
                    }

                    if (GradientItems.Count < _numRasters)
                    {
                        var diff = _numRasters - GradientItems.Count;
                        for (i = 0; i < diff; i++)
                        {
                            GradientItems.Add(new GradientItem { Color = Colors.Black, Index = GradientItems.Count});
                        }
                    }
                    UpdateGradientPreviewAction?.Invoke();
                    RebindAction?.Invoke();
                    if (PreviewImage != null)
                    {
                        RefreshRasterPreviewImageCommand.Execute(null);
                    }
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
            GenerateGradientRasterCommand = new DelegateCommand<string>(s =>
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
                    (var w, var h) = ActivePicture.GetDimensions;
                    PictureProperties = $"{w}*{h} pixels, {ActivePicture.ActivePalette.Length} colors";
                    PictureLoaded = true;
                }
            });
            GenerateGradientCommand = new DelegateCommand<string>(s =>
            {
                var startColor = Helpers.Globals.ActivePlatform.ToRgb(_startColor.ToHex());
                var endColor = Helpers.Globals.ActivePlatform.ToRgb(_endColor.ToHex());
                var data = Helpers.GetGradients(startColor, endColor, FadeSteps).ToList();

                var sb = new StringBuilder();
                var iterator = 0;
                foreach (var color in data)
                {
                    sb.Append("$" + Helpers.Globals.ActivePlatform.ColorToString(color));
                    if (iterator < data.Count-1) sb.Append(",");
                    iterator++;
                }

                GeneratedPalette = sb.ToString();
                UpdateGradientAction?.Invoke(data.ToList());
            });
            FadeToColorCommand = new DelegateCommand<string>(b =>
            {
                if (ActivePicture?.ActivePalette == null) return;
                GenerateFade(ActivePicture.ActivePalette, FadeToColor);
            });
            FadeFromColorCommand = new DelegateCommand<string>(b =>
            {
                if (ActivePicture?.ActivePalette == null) return;
                var palette = new List<Color>();
                var color =  Helpers.Globals.ActivePlatform.ToRgb(FadeToColor.ToHex());
                for (var i = 0; i < 16; i++) palette.Add(color);
                GenerateFade(palette.ToArray(), ActivePicture.ActivePalette);
            });
            FadeToBlackCommand = new DelegateCommand<string>(b =>
            {
                if (ActivePicture?.ActivePalette == null) return;
                GenerateFade(ActivePicture.ActivePalette, "0");
            });
            FadeFromBlackCommand = new DelegateCommand<string>(b =>
            {
                if (ActivePicture?.ActivePalette == null) return;
                var blackPalette = new List<Color>();
                for (var i = 0; i < 16; i++) blackPalette.Add(Color.FromRgb(0,0,0));
                GenerateFade(blackPalette.ToArray(), ActivePicture.ActivePalette);
            });
            FadeToWhiteCommand = new DelegateCommand<string>(b =>
            {
                if (ActivePicture?.ActivePalette == null) return;
                GenerateFade(ActivePicture.ActivePalette, "FFF");
            });
            FadeFromWhiteCommand = new DelegateCommand<string>(b =>
            {
                if (ActivePicture?.ActivePalette == null) return;
                var whitePalette = new List<Color>();
                for (var i = 0; i < 16; i++) whitePalette.Add(Color.FromRgb(255, 255, 255));
                GenerateFade(whitePalette.ToArray(), ActivePicture.ActivePalette);
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
            SwapColorsCommand = new DelegateCommand<string>(s =>
            {
                ActivePicture.SwapColors(6, 0);
                UpdatePictureAction?.Invoke(PictureType.Picture1);
                //UpdateUiAction?.Invoke(true);
                ActivePaletteString = Helpers.RgbPaletteTo12BitString(ActivePicture.ActivePalette);
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
                                colorData.Add(Helpers.Globals.ActivePlatform.ToRgb(cleanedItem.ToHex()));
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
                        if (i == GradientItems.Count) break;
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
            DrawCurveCommand = new DelegateCommand<string>(async p =>
            {
                string expression = "23 + 4 * (2 - 1)";
                expression = "80 + System.Math.Sin(System.Math.PI)* 55";
                var wbmp = BitmapFactory.New(320, 200);
                CurveImage.Source = wbmp;
                const double RAD = Math.PI / 256.0;

                using (wbmp.GetBitmapContext())
                {
                    wbmp.Clear(Colors.Black);
                    for (var i = 0; i < 512; i++)
                    {
                        expression = $"80 + System.Math.Sin(i * 0.25 * System.Math.PI) * 55";
                        var res = await GetExpressionResult(expression, i);

                        var s = 80 + Math.Sin(i * 0.25 * Math.PI * RAD) * 55;
                        var c = 120 + Math.Cos(i * 0.25 * Math.PI * RAD) * Math.Sin(i * 0.75 * Math.PI * RAD) * 120;
                        var iSin = (int)s;
                        var iCos = (int)c;
                        wbmp.SetPixel(iCos, iSin, Colors.White);
                    }


                    /*
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
                    }*/
                }
            });
        }

        private async Task<double> GetExpressionResult(string expression, int iterator)
        {
            try
            {
                // Parse and evaluate the expression
                var result = await EvaluateExpressionAsync(expression, iterator);
                return (double)result;
                // Display the result
                //Console.WriteLine($"Result of expression '{expression}': {result}");
            }
            catch (CompilationErrorException ex)
            {
                // Handle compilation errors
                Console.WriteLine($"Compilation error: {string.Join(Environment.NewLine, ex.Diagnostics)}");
                return -1;
            }
        }
        private static async Task<object> EvaluateExpressionAsync(string expression, int iterator)
        {
            //Assembly[] assemblies = {
            //    System.Reflection.Assembly.Load("System.Math"),
            //    System.Reflection.Assembly.GetExecutingAssembly()
            //};

            const double RAD256 = Math.PI / 256.0;
            var globals = new { i = iterator, RAD = RAD256};

            // Options for the script
            var options = ScriptOptions.Default
                .AddImports("System.Math")
                .WithImports("System");
            //.AddReferences(assemblies);
            //.WithReferences(AppDomain.CurrentDomain.GetAssemblies())
            //.WithImports("System");

            // Evaluate the expression
            var result = await CSharpScript.EvaluateAsync(expression, options, globals);

            return result;
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
            var generatedColors = new Color[fromPalette.Length * _fadeSteps];
            var stColors = new string[fromPalette.Length * _fadeSteps];
            for (var i = 0; i < fromPalette.Length; i++)
            {
                var startColor = fromPalette[i];
                var endColor = Helpers.Globals.ActivePlatform.ToRgb(endColorStr.ToHex());
                var data = Helpers.GetGradients(startColor, endColor, _fadeSteps).ToList();

                for (var j = 0; j < _fadeSteps; j++)
                {
                    var ofs = i + j * fromPalette.Length;
                    generatedColors[ofs] = data[j];
                    stColors[ofs] = Helpers.Globals.ActivePlatform.ColorToString(data[j]);
                }
            }
            var sb = new StringBuilder();
            sb.AppendLine("fade_data:");
            for (var y = 0; y < _fadeSteps; y++)
            {
                sb.Append("\t" + SelectedDataTypePrefix);
                for (var x = 0; x < fromPalette.Length; x++)
                {
                    var ofs = x + y * fromPalette.Length;
                    sb.Append("$");
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
            var generatedColors = new Color[fromPalette.Length * _fadeSteps];
            var stColors = new string[fromPalette.Length * _fadeSteps];
            for (var i = 0; i < fromPalette.Length; i++)
            {
                var startColor = fromPalette[i];
                var endColor = toPalette[i];
                var data = Helpers.GetGradients(startColor, endColor, _fadeSteps).ToList();

                for (var j = 0; j < _fadeSteps; j++)
                {
                    var ofs = i + j * fromPalette.Length;
                    generatedColors[ofs] = data[j];
                    stColors[ofs] = Helpers.Globals.ActivePlatform.ColorToString(data[j]);
                }
            }
            var sb = new StringBuilder();
            for (var y = 0; y < _fadeSteps; y++)
            {
                sb.Append("\t" + SelectedDataTypePrefix);
                for (var x = 0; x < fromPalette.Length; x++)
                {
                    var ofs = x + y * fromPalette.Length;
                    sb.Append("$");
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

        public void ReloadActivePicture()
        {
            ActivePicture = PictureFactory.GetPicture(_activeFilename);
            if (ActivePicture == null) return;
            ActivePaletteString = Helpers.RgbPaletteTo12BitString(ActivePicture.ActivePalette);
            UpdatePictureAction?.Invoke(PictureType.Picture1);
            UpdateUiAction?.Invoke(true);
        }
        public void SetNumberOfRasters(int newValue)
        {
            if (newValue < GradientItems.Count)
            {
                while (GradientItems.Count > newValue)
                {
                    GradientItems.RemoveAt(GradientItems.Count-1);
                }
            }
            else
            {
                while (GradientItems.Count < newValue)
                {
                    GradientItems.Add(new GradientItem { Color = Colors.Black, Index = GradientItems.Count });
                }
            }

            NumRasters = newValue;
        }
    }
}
