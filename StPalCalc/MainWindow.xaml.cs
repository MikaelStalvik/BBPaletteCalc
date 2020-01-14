using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;
using StPalCalc.iff;
using StPalCalc.PictureFormats;

namespace StPalCalc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm;

            // Todo: support more than 16 colors

            DataTypesCombo.ItemsSource = _vm.DataTypes;
            _vm.StartColor = "700";
            _vm.EndColor = "770";
            _vm.UpdateUiAction += () =>
            {
                ColorsStackPanel1.Children.Clear();
                for (var i = 0; i < 16; i++)
                {
                    var color = _vm.ActivePicture.ActivePalette[i];
                    var r = new Rectangle
                    {
                        Fill = new SolidColorBrush(color),
                        Width = 24,
                        Height = 24,
                        ToolTip = Helpers.ConvertFromRgbTo12Bit(color)
                    };
                    var btn = new Button {Content = r, Tag = i};
                    btn.Click += (sender, args) =>
                    {
                        var b = (Button) sender;
                        var index = (int) b.Tag;
                        var pc = ColorPickerWindow.PickColor(
                            _vm.ActivePicture.ActivePalette[index]);
                        if (pc != null)
                        {
                            var stColor = Helpers.ConvertFromRgbTo12Bit(pc.Value, true);
                            _vm.SetPaletteValue((ushort) Convert.ToInt32(stColor, 16), index);
                            _vm.UpdatePictureAction.Invoke(PictureType.Picture1);
                        }
                    };
                    ColorsStackPanel1.Children.Add(btn);
                }

                HueSlider_OnValueChanged(null, null);
            };
            _vm.UpdateGradientAction += colors =>
            {
                GeneratedColorsStackPanel.Children.Clear();
                foreach (var color in colors)
                {
                    var r = new Rectangle
                    {
                        Fill = new SolidColorBrush(color),
                        Width = 24,
                        Height = 24,
                        ToolTip = Helpers.ConvertFromRgbTo12Bit(color)
                    };
                    GeneratedColorsStackPanel.Children.Add(r);
                }
            };
            _vm.UpdatePreviewFadeAction += colors =>
            {
                PreviewPanel.Children.Clear();
                for (var y = 0; y < 16; y++)
                {
                    for (var x = 0; x < 16; x++)
                    {
                        var ofs = x + y * 16;
                        var r = new Rectangle { Fill = new SolidColorBrush(colors[ofs]), Width = 24, Height = 24 };
                        PreviewPanel.Children.Add(r);
                    }
                }
            };
            _vm.UpdateGradientPreviewAction += () =>
            {
                GradientPreviewPanel.Children.Clear();
                foreach (var item in _vm.GradientItems)
                {
                    var sp = new StackPanel();
                    sp.Width = 48;
                    sp.Height = 2;
                    sp.Orientation = Orientation.Horizontal;
                    var mc = item == _vm.SelectedGradientItem ? Colors.Red : Colors.White;
                    var marker = new Rectangle { Fill = new SolidColorBrush(mc), Width = 16, Height = 2};
                    sp.Children.Add(marker);
                    var r = new Rectangle { Fill = new SolidColorBrush(item.Color), Width = 24, Height = 2 };
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
                        _vm.PreviewPicture.RenderWithRasters(PreviewImage, _vm.GradientItems.ToList(), _vm.RasterColorIndex);
                        break;
                }
            };
            _vm.UpdateHueColorsAction += colors =>
            {
                HueColorsStackPanel1.Children.Clear();
                foreach (var color in colors)
                {
                    var r = new Rectangle
                    {
                        Fill = new SolidColorBrush(color),
                        Width = 24,
                        Height = 24,
                    };
                    HueColorsStackPanel1.Children.Add(r);
                }

                _vm.ActivePicture?.Render(Image1, _vm.HuePalette);
                _vm.RawPalette = Helpers.RgbPaletteTo12BitString(_vm.HuePalette);
            };
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = (ListBox)sender;
            var item = (GradientItem)lb.SelectedItem;
            if (item == null) return;
            _vm.SelectedGradientItem = item;
            ColorCanvas.SelectedColor = item.Color;
            GradientText.Text = Helpers.ConvertFromRgbTo12Bit(item.Color, true);
            var selectedItems = lb.SelectedItems.Cast<GradientItem>().ToList();
            var min = selectedItems.Min(x => x.Index);
            var max = selectedItems.Max(x => x.Index);
            _vm.StartGradientIndex = min;
            _vm.EndGradientIndex = max;
            _vm.SelectedGradientItems = selectedItems;
        }

        private void ColorCanvas_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            GradientText.Text = Helpers.ConvertFromRgbTo12Bit(ColorCanvas.SelectedColor.Value);
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
    }
}
