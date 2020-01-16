using System;
using System.Collections.Generic;
using System.Text;
using StPalCalc.Platforms;

namespace StPalCalc
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
