using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BBPalCalc.Platforms;
using BBPalCalc.Types;
using BBPalCalc.Util;
using BBPalCalc.ViewModels;

namespace BBPalCalc
{
    /// IFF-LBM reader based on Pavel Torgashows implementation:
    /// https://github.com/PavelTorgashov/IFF-ILBM-Parser
    /// 
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm = new MainViewModel();

        private void RebuildActivePalette()
        {
            if (_vm.ActivePicture == null) return;
            ColorsPaletteControl.Update(_vm.ActivePicture.ActivePalette, (color, index) =>
            {
                _vm.SetPaletteValue(color, index);
                _vm.UpdatePictureAction.Invoke(PictureType.Picture1);
            }, true);
            OriginalColorsPaletteControl.Update(_vm.ActivePicture.OriginalPalette, null, false);
        }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm;

            DataTypesCombo.ItemsSource = _vm.DataTypes;
            PlatformCombo.ItemsSource = _vm.Platforms;
            NumRastersComboBox.ItemsSource = _vm.NumberOfRasters;
            _vm.StartColor = "700";
            _vm.EndColor = "770";
            _vm.UpdateUiAction += updateHue =>
            {
                RebuildActivePalette();
                if (updateHue)
                {
                    HueSlider_OnValueChanged(null, null);
                }
            };
            _vm.UpdateGradientAction += colors =>
            {
                GeneratedGradientPresenter.Update(colors.ToArray(), null, false);
            };
            _vm.UpdatePreviewFadeAction += colors =>
            {
                PreviewPanel.Width = _vm.ActivePicture.ActivePalette.Length * 24;
                PreviewPanel.Children.Clear();
                for (var y = 0; y < _vm.FadeSteps; y++)
                {
                    for (var x = 0; x < _vm.ActivePicture.ActivePalette.Length; x++)
                    {
                        var ofs = x + y * _vm.ActivePicture.ActivePalette.Length;
                        var r = new Rectangle
                        {
                            Fill = new SolidColorBrush(colors[ofs]), 
                            Width = 24, 
                            Height = 24,
                            ToolTip = "$" + Helpers.Globals.ActivePlatform.ColorToString(colors[ofs]) + $"\nIndex: {x}, row: {y}"
                        };
                        PreviewPanel.Children.Add(r);
                    }
                }
            };
            _vm.UpdateGradientPreviewAction += () =>
            {
                const int previewItemHeight = 2;
                GradientPreviewPanel.Children.Clear();
                foreach (var item in _vm.GradientItems)
                {

                    var sp = new StackPanel {Width = 48, Height = previewItemHeight, Orientation = Orientation.Horizontal, SnapsToDevicePixels = true};
                    var mc = item == _vm.SelectedGradientItem ? Colors.Red : Colors.White;
                    var marker = new Rectangle { Fill = new SolidColorBrush(mc), Width = 16, Height = previewItemHeight};
                    sp.Children.Add(marker);

                    // fake quantization/down sampling
                    var asString = Helpers.Globals.ActivePlatform.ColorToString(item.Color);
                    var remappedColor = Helpers.Globals.ActivePlatform.ToRgb(asString.ToHex()); 
                    var r = new Rectangle
                    {
                        Fill = new SolidColorBrush(remappedColor), Width = 24, Height = previewItemHeight, SnapsToDevicePixels = true
                    };
                    sp.Children.Add(r);
                    GradientPreviewPanel.Children.Add(sp);
                }
            };
            _vm.RebindAction += () => { GradientListBox.ItemsSource = _vm.GradientItems; };
            _vm.UpdatePictureAction += pictureType =>
            {
                switch (pictureType)
                {
                    case PictureType.Picture1:
                        _vm.ActivePicture.Render(Image1);
                        break;
                    case PictureType.PreviewPicture:
                        if (_vm.PreviewPicture == null) return;
                        _vm.PreviewPicture.RenderWithRasters(PreviewImage, _vm.GradientItems.ToList(), _vm.RasterColorIndex);
                        (var w, var h) = _vm.PreviewPicture.GetDimensions;
                        PreviewImage.Width = w;
                        PreviewImage.Height = h;
                        break;
                }
            };
            _vm.UpdateHueColorsAction += colors =>
            {
                _vm.ActivePicture?.Render(Image1, _vm.HuePalette);
                _vm.ActivePaletteString = Helpers.RgbPaletteTo12BitString(_vm.HuePalette);
                RebuildActivePalette();
            };
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = (ListBox)sender;
            var item = (GradientItem)lb.SelectedItem;
            if (item == null) return;
            _vm.SelectedGradientItem = item;
            ColorCanvas.SelectedColor = item.Color;
            GradientText.Text = Helpers.Globals.ActivePlatform.ColorToString(item.Color);
            var selectedItems = lb.SelectedItems.Cast<GradientItem>().ToList();
            var min = selectedItems.Min(x => x.Index);
            var max = selectedItems.Max(x => x.Index);
            _vm.StartGradientIndex = min;
            _vm.EndGradientIndex = max;
            _vm.SelectedGradientItems = selectedItems;
        }

        private void ColorCanvas_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            GradientText.Text = Helpers.Globals.ActivePlatform.ColorToString(ColorCanvas.SelectedColor.Value);
            _vm.UpdateSelectedGradientColor(ColorCanvas.SelectedColor.Value);
            _vm.UpdateGradientPreviewAction?.Invoke();
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _vm.SetNewSelectedGradientColor(((TextBox)sender).Text);
        }

        private void DataTypesCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.SelectedDataType = ((ComboBox) sender).SelectedIndex;
        }
        private void PlatformCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.SelectedPlatform = ((ComboBox) sender).SelectedIndex;
            Helpers.Globals.ActivePlatform = PlatformFactory.CreatePlatform((PlatformTypes)_vm.SelectedPlatform);
            if (_vm.ActivePicture != null)
            {
                _vm.ReloadActivePicture();
                _vm.UpdatePaletteCommand.Execute(_vm.ActivePaletteString);
            }
        }

        private void HueSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _vm.AdjustHueCommand.Execute(new HslSliderPayload { Hue = (int)HueSlider.Value, Saturation = (int)SaturationSlider.Value, Lightness = (int)LightnessSlider.Value});
        }

        private void ResetButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            HueSlider.Value = 0;
            SaturationSlider.Value = 0;
            LightnessSlider.Value = 0;
            _vm.ResetPalette();
        }

        private void NumRastersComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = ((ComboBox) sender);
            switch (combo.SelectedIndex)
            {
                case 0: 
                    _vm.SetNumberOfRasters(200);
                    break;
                case 1:
                    _vm.SetNumberOfRasters(256);
                    break;
                case 2:
                    _vm.SetNumberOfRasters(312);
                    break;
            }
        }
    }
}
