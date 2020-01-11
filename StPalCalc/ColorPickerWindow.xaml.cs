using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

        public static Color? PickColor()
        {
            var dlg = new ColorPickerWindow();
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
