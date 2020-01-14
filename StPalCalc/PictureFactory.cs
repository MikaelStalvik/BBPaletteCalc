using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using StPalCalc.PictureFormats;

namespace StPalCalc
{
    public static class PictureFactory
    {
        public static IPicture ReadPicture(string filename)
        {
            var ext = Path.GetExtension(filename).ToLowerInvariant();
            if (ext == ".pi1") return new Pi1Picture();
            if (ext == ".iff") return new IffPicture();
            return null;
        }
    }
}
