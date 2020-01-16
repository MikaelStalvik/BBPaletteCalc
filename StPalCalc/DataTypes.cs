using System.Windows.Media;

namespace BBPalCalc
{
    public enum PictureType
    {
        Picture1,
        PreviewPicture,
        Picture1Hue
    }

    public class HslSliderPayload
    {
        public int Hue { get; set; }
        public int Saturation { get; set; }
        public int Lightness { get; set; }

    }
    public class GradientItem : BaseViewModel
    {
        public int Index { get; set; }

        private Color _color;

        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                OnPropertyChanged();
            }
        }
    }
}
