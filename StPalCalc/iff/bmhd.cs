using System;

namespace StPalCalc.iff
{
    public class BMHD
    {
        public UInt16 W, H;             /* raster width & height in pixels      */
        public Int16 X, Y;              /* pixel position for this image        */
        public byte NPlanes;            /* # source bitplanes                   */
        public Masking Masking;
        public Compression Compression;
        public byte Pad1;               /* unused; ignore on read, write as 0   */
        public UInt16 TransparentColor; /* transparent "color number" (sort of) */
        public byte XAspect, YAspect;   /* pixel aspect, a ratio width : height */
        public Int16 PageWidth, PageHeight; /* source "page" size in pixels    */
    }

    public enum Masking : byte
    {
        None = 0,
        HasMask = 1,
        HasTransparentColor = 2,
        Lasso = 3
    }

    public enum Compression : byte
    {
        None = 0,
        ByteRun1 = 1
    }
}
