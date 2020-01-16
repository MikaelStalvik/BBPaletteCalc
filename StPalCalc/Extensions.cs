using System;

namespace StPalCalc
{
    public static class Extensions
    {
        public static ushort ToHex(this string data)
        {
            return (ushort) Convert.ToInt32(data, 16);
        }
    }
}
