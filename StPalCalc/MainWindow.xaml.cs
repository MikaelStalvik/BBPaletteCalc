﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;

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
            DataTypesCombo.ItemsSource = _vm.DataTypes;
            _vm.StartColor = "700";
            _vm.EndColor = "770";
            _vm.UpdateUiAction += () =>
            {
                ColorsStackPanel1.Children.Clear();
                ColorsStackPanel2.Children.Clear();
                for (var i = 0; i < 16; i++)
                {
                    var color = _vm.GetRgbFromPalette1(i);
                    var r = new Rectangle
                    {
                        Fill = new SolidColorBrush(color),
                        Width = 24,
                        Height = 24,
                        ToolTip = _vm.Get12BitRgbFromPalette1(i)
                    };
                    ColorsStackPanel1.Children.Add(r);
                    color = _vm.GetRgbFromPalette2(i);
                    r = new Rectangle
                    {
                        Fill = new SolidColorBrush(color),
                        Width = 24,
                        Height = 24,
                        ToolTip = _vm.Get12BitRgbFromPalette2(i)
                    };
                    ColorsStackPanel2.Children.Add(r);
                }
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
                    var r = new Rectangle { Fill = new SolidColorBrush(item.Color), Width = 24, Height = 2 };
                    GradientPreviewPanel.Children.Add(r);
                }
            };
            _vm.RebindAction += () => { GradientListBox.ItemsSource = _vm.GradientItems; };
            _vm.UpdatePictureAction += index =>
            {
                if (index == 0) _vm.RenderPi1(_vm.ActiveFilename, Image1, true);
                if (index == 1) _vm.RenderPi1(_vm.ActiveFilename2, Image2, false);
            };
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = (ListBox)sender;
            var item = (GradientItem)lb.SelectedItem;
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
    }
}
