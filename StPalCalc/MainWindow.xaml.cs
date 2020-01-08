using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace StPalCalc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _vm = new MainViewModel();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm;
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
                        Fill = new SolidColorBrush(color), Width = 24, Height = 24, ToolTip = _vm.Get12BitRgbFromPalette1(i)
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
                        var r = new Rectangle {Fill = new SolidColorBrush(colors[ofs]), Width = 24, Height = 24};
                        PreviewPanel.Children.Add(r);
                    }
                }
            };
        }
    }
}
