using BBPalCalc.Platforms;

namespace BBPalCalc.Interfaces
{
    public interface IGlobals
    {
        IPlatform ActivePlatform { get; set; }
    }

    public class Globals : IGlobals
    {
        public IPlatform ActivePlatform { get; set; }
    }
}
