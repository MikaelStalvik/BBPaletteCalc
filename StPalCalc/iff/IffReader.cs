using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media;

namespace BBPalCalc.iff
{
    public class IffReader
    {
        public byte[] JPEG { get; private set; }
        public Color[] CMAP { get; private set; }
        public BMHD BMHD { get; private set; }
        public VERS VERS { get; private set; }

        public byte[] PixelData { get; private set; }
        public (int, int) GetDimensions => (BMHD.W, BMHD.H);

        public void Parse(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            using (BinaryReader reader = new BigEndianBinaryReader(stream))
            {
                // read the FORM header that identifies the document as an IFF file
                var buffer = reader.ReadBytes(4);
                if (Encoding.ASCII.GetString(buffer) != "FORM")
                    throw new InvalidDataException("Form header is not found.");

                // the next value is the size of all the data in the FORM chunk
                // We don't actually need this value, but we have to read it
                // regardless to advance the stream
                reader.ReadInt32();

                // read either the PBM or ILBM header that identifies this document as an image file
                buffer = reader.ReadBytes(4);
                var header = Encoding.ASCII.GetString(buffer);
                if (header != "PBM " && header != "ILBM")
                    throw new InvalidDataException("Bitmap header is not found.");

                while (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
                {
                    var chunkLength = reader.ReadInt32();
                    var chunkName = Encoding.ASCII.GetString(buffer);
                    //Console.WriteLine(chunkName + " size: " + chunkLength);

                    ParseChunk(reader, chunkName, chunkLength);

                    // chunks always contain an even number of bytes even if the recorded length is odd
                    // if the length is odd, then there's a padding byte in the file - just read and discard
                    if (chunkLength % 2 != 0)
                        reader.ReadByte();
                }
            }
        }
        protected virtual void ParseChunk(BinaryReader reader, string chunkName, int chunkLength)
        {
            switch (chunkName)
            {
                case "JPEG":
                    ReadJPEG(reader, chunkLength);
                    break;
                case "CMAP":
                    ReadCMAP(reader, chunkLength);
                    break;
                case "BMHD":
                    ReadBMHD(reader, chunkLength);
                    break;
                case "VERS":
                    ReadVERS(reader, chunkLength);
                    break;
                case "BODY":
                    ReadBODY(reader, chunkLength);
                    break;
                default:
                    SkipChunk(reader, chunkLength);
                    break;
            }
        }
        private void ReadBODY(BinaryReader reader, int chunkLength)
        {
            var bytesPerRow = 2 * ((BMHD.W + 15) / 16);
            var bytes = reader.ReadBytes(chunkLength);
            if (BMHD.Compression == Compression.ByteRun1)
                bytes = Decompress(bytes);

            PixelData = new byte[BMHD.W * BMHD.H];

            //scan lines
            for (int iLine = 0; iLine < BMHD.H; iLine++)
            {
                var linePixels = new UInt32[BMHD.W];
                //scan planes
                for (int iPlane = 0; iPlane < BMHD.NPlanes; iPlane++)
                {
                    var bits = new BitReader(bytes);
                    bits.Seek((iPlane + BMHD.NPlanes * iLine) * bytesPerRow * 8);
                    for (int iPixel = 0; iPixel < BMHD.W; iPixel++)
                    {
                        var bit = bits.Read();
                        linePixels[iPixel] = (UInt32) (linePixels[iPixel] | (bit << (iPlane)));
                    }
                }

                //copy pixels to bitmap
                for (int i = 0; i < linePixels.Length; i++)
                {
                    var iColor = linePixels[i];
                    var ofs = i + iLine * BMHD.W;
                    PixelData[ofs] = (byte)iColor;  //ColorIndexToColor(iColor);
                    //wr[i, iLine] = ColorIndexToColor(iColor);
                }
            }
        }
        protected virtual Color ColorIndexToColor(UInt32 colorIndex)
        {
            if (CMAP == null)
                throw new InvalidDataException("CMAP chunk is required");
            return CMAP[colorIndex];
        }

        private byte[] Decompress(byte[] bytes)
        {
            var res = new List<Byte>();
            for (int i = 0; i < bytes.Length; i++)
            {
                var n = (sbyte)bytes[i];

                if (n >= 0 && n <= 127)
                {
                    /* copy the next N+1 bytes literally */
                    for (int j = 0; j < n + 1; j++)
                    {
                        i++;
                        res.Add(bytes[i]);
                    }
                    continue;
                }

                if (n >= -127 && n <= -1)
                {
                    /* repeat the next byte N+1 times */
                    for (int j = 0; j < -n + 1; j++)
                        res.Add(bytes[i + 1]);
                    i++;
                    continue;
                }


                if (n == -128)
                {
                    /* skip it, presumably it's padding */
                    continue;
                }
            }

            return res.ToArray();
        }

        private void ReadVERS(BinaryReader reader, int chunkLength)
        {
            VERS = new VERS();
            reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadUInt32();
            var buf = reader.ReadBytes(chunkLength - 4 * 4);
            VERS.Desc = Encoding.ASCII.GetString(buf);
        }

        private void ReadBMHD(BinaryReader reader, int chunkLength)
        {
            BMHD = new BMHD();
            BMHD.W = reader.ReadUInt16();
            BMHD.H = reader.ReadUInt16();
            BMHD.X = reader.ReadInt16();
            BMHD.Y = reader.ReadInt16();
            BMHD.NPlanes = reader.ReadByte();
            BMHD.Masking = (Masking)reader.ReadByte();
            BMHD.Compression = (Compression)reader.ReadByte();
            BMHD.Pad1 = reader.ReadByte();
            BMHD.TransparentColor = reader.ReadUInt16();
            BMHD.XAspect = reader.ReadByte();
            BMHD.YAspect = reader.ReadByte();
            BMHD.PageWidth = reader.ReadInt16();
            BMHD.PageHeight = reader.ReadInt16();
        }

        private void ReadJPEG(BinaryReader reader, int chunkLength)
        {
            JPEG = new byte[chunkLength];
            reader.Read(JPEG, 0, chunkLength);
        }

        private void ReadCMAP(BinaryReader reader, int chunkLength)
        {
            CMAP = new Color[chunkLength / 3];
            for (var i = 0; i < chunkLength / 3; i++)
            {
                int r;
                int g;
                int b;

                r = reader.ReadByte();
                g = reader.ReadByte();
                b = reader.ReadByte();

                CMAP[i] = Color.FromRgb((byte)r, (byte)g, (byte)b);
            }
        }

        private static void SkipChunk(BinaryReader reader, int chunkLength)
        {
            // some other LBM chunk, skip it
            if (reader.BaseStream.CanSeek)
            {
                reader.BaseStream.Seek(chunkLength, SeekOrigin.Current);
            }
            else
            {
                for (var i = 0; i < chunkLength; i++)
                    reader.ReadByte();
            }
        }


    }
}
