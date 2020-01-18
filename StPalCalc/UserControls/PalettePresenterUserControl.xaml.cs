using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BBPalCalc.Util;

namespace BBPalCalc.UserControls
{
    /// <summary>
    /// Interaction logic for PalettePresenterUserControl.xaml
    /// </summary>
    public partial class PalettePresenterUserControl : UserControl
    {
        public PalettePresenterUserControl()
        {
            InitializeComponent();
        }

        public void Update(Color[] palette, Action<Color, int> updateColorAction, bool clickable)
        {
            ColorsWrapPanel.Children.Clear();
            for (var i = 0; i < palette.Length; i++)
            {
                var color = palette[i];
                var r = new Rectangle
                {
                    Fill = new SolidColorBrush(color),
                    Width = 24,
                    Height = 24,
                    ToolTip = "$" + Helpers.Globals.ActivePlatform.ColorToString(color) + $"\nR:{color.R} G:{color.G} B:{color.B}\nIndex: {i}"
                };
                if (clickable)
                {
                    var btn = new Button {Content = r, Tag = i};
                    btn.Click += (sender, args) =>
                    {
                        var b = (Button) sender;
                        var index = (int) b.Tag;
                        var pc = ColorPickerWindow.PickColor(palette[index]);
                        if (pc != null)
                        {
                            updateColorAction?.Invoke(pc.Value, index);
                        }
                    };
                    ColorsWrapPanel.Children.Add(btn);
                }
                else
                {
                    ColorsWrapPanel.Children.Add(r);
                }
            }
        }
    }
}
