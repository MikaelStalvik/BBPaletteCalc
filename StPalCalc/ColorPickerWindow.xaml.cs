using System.Windows;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace StPalCalc
{
    /// <summary>
    /// Interaction logic for ColorPickerWindow.xaml
    /// </summary>
    public partial class ColorPickerWindow : Window
    {
        private Color SelectedColor { get; set; }
        public ColorPickerWindow()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private void ColorCanvas_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            SelectedColor = ((ColorCanvas) sender).SelectedColor.Value;
        }

        public static Color? PickColor(Color startColor)
        {
            var dlg = new ColorPickerWindow();
            dlg.ColorCanvas.SelectedColor = startColor;
            if (dlg.ShowDialog() == true)
            {
                return dlg.SelectedColor;
            }

            return null;
        }

        private void CancelButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OkButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
