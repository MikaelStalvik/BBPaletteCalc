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
            _vm.UpdateUi += () =>
            {
                ColorsStackPanel.Children.Clear();
                for (var i = 0; i < 16; i++)
                {
                    var color = _vm.GetRgb(i);
                    var r = new Rectangle {Fill = new SolidColorBrush(color), Width = 24, Height = 24};
                    r.ToolTip = _vm.Get12BitRgb(i);
                    ColorsStackPanel.Children.Add(r);
                }
            };
            _vm.UpdateGradient += colors =>
            {
                GeneratedColorsStackPanel.Children.Clear();
                foreach (var color in colors)
                {
                    var r = new Rectangle { Fill = new SolidColorBrush(color), Width = 24, Height = 24 };
                    r.ToolTip = Helpers.ConvertFromRgbTo12Bit(color);
                    GeneratedColorsStackPanel.Children.Add(r);
                }
            };
            _vm.UpdatePreviewFade += colors =>
            {
                PreviewPanel.Children.Clear();
                for (var y = 0; y < 16; y++)
                {
                    for (var x = 0; x < 16; x++)
                    {
                        var ofs = x + y * 16;
                        var r = new Rectangle {Fill = new SolidColorBrush(colors[ofs]), Width = 24, Height = 24};
                        
                        //r.ToolTip = _vm.Get12BitRgb(i);
                        PreviewPanel.Children.Add(r);
                    }
                }
            };
        }
    }
}
