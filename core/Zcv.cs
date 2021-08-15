using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace core
{
    public partial class Zcv
    {
        public static byte[][,] Read(string filename)
        {
            Bitmap bitmap = new Bitmap(filename);
            return ToBytes(bitmap);
        }

        public static Bitmap ToBitmap(byte[][,] imgdata)
        {
            int channels = imgdata.Length;
            if(channels == 0) throw new ArgumentException($"{nameof(imgdata)} no data");

            int height = imgdata[0].GetLength(0);
            int width = imgdata[0].GetLength(1);

            PixelFormat pxFormat;
            switch(channels)
            {
                case 1: pxFormat = PixelFormat.Format8bppIndexed; break;
                case 3: pxFormat = PixelFormat.Format24bppRgb; break;
                default:
                    throw new NotSupportedException($"Not supporte number of channel {channels}");
            }

            Bitmap bitmap = new Bitmap(width, height, pxFormat);

            if (pxFormat == PixelFormat.Format8bppIndexed)
            {
                ColorPalette plt = bitmap.Palette;
                for (int x = 0; x < 256; x++)
                {
                    plt.Entries[x] = System.Drawing.Color.FromArgb(x, x, x);
                }
                bitmap.Palette = plt;
            }

            BitmapData bpdata = null;
            try
            {
                Rectangle rect = new Rectangle(0, 0, width, height);
                bpdata = bitmap.LockBits(rect, ImageLockMode.WriteOnly, pxFormat);
                CopyToBitmapData(imgdata, height, width, pxFormat, bpdata);
                return bitmap;
            }
            finally
            {
                if(bpdata != null) bitmap.UnlockBits(bpdata);
            }
        }

        private static unsafe void CopyToBitmapData(byte[][,] imgdata, int rows, int cols, PixelFormat pixelFormat, BitmapData bpdata)
        {
            switch(pixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    byte* ptr = (byte*)bpdata.Scan0;
                    for (int i = 0; i < rows; i++)
                    {
                        fixed (byte* imgp = &imgdata[0][i, 0])
                        {
                            Buffer.MemoryCopy(imgp, ptr, cols, cols);
                            ptr += cols;
                        }
                    }
                    break;
                case PixelFormat.Format24bppRgb:
                    throw new NotImplementedException();
                default:
                    throw new NotSupportedException($"Not supported pixel format {pixelFormat}");
            }
        }

        private static byte[][,] ToBytes(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            int width = bitmap.Width;
            int height = bitmap.Height;

            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bpdata = null;
            try
            {
                bpdata = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
                switch (bitmap.PixelFormat)
                {
                    case PixelFormat.Format24bppRgb:
                        return ReadBytes(bpdata);
                    default:
                        throw new NotSupportedException($"Not supported value:{bitmap.PixelFormat}");
                }
            }
            finally
            {
                if(bpdata != null) bitmap.UnlockBits(bpdata);
            }
        }

        private static unsafe byte[][,] ReadBytes(BitmapData bpdata)
        {
            int stride = bpdata.Stride;
            IntPtr ptr = bpdata.Scan0;

            int rows = bpdata.Height;
            int cols = bpdata.Width;

            byte[,] chr = new byte[rows, cols]; //red channel
            byte[,] chg = new byte[rows, cols]; //green channel
            byte[,] chb = new byte[rows, cols]; //blue channel

            byte[] br = new byte[cols]; 
            byte[] bg = new byte[cols]; 
            byte[] bb = new byte[cols]; 
 
            byte* sp = (byte*)bpdata.Scan0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    byte blue = *(sp);
                    byte green = *(sp + 1);
                    byte red = *(sp + 2);
                    sp += 3;

                    //br[j] = red;
                    //bg[j] = green;
                    //bb[j] = blue;

                    chr[i, j] = red;
                    chg[i, j] = green;
                    chb[i, j] = blue;
                }

                //MemoryCopy(br, chr, i, cols);
                //MemoryCopy(bg, chg, i, cols);
                //MemoryCopy(bb, chb, i, cols);
            }

            return new byte[][,] { chr, chg, chb };
        }

        private static unsafe void MemoryCopy(byte[] source, byte[,] dst, int rowIndex, int length)
        {
            fixed (byte* brp = &source[0])
            {
                fixed (byte* chrp = &dst[rowIndex, 0])
                {
                    Buffer.MemoryCopy(brp, chrp, length, length);
                }
            }
        }
    }
}
