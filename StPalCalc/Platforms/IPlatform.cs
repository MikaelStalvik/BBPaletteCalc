using System.Windows.Media;

namespace BBPalCalc.Platforms
{
    public interface IPlatform
    {
        Color ToRgb(ushort source);
        string ColorToString(Color source);
        byte RemapFromLowerDepth(byte color);
    }
}
