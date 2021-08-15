using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core
{
    public partial class ZImage<TDepth> : IImage where TDepth : unmanaged
    {
        internal static ZImage<byte> Read(string filename, bool grayscale)
        {
            Bitmap bitmap = new Bitmap(filename);
            return FromBitmap(bitmap, grayscale);
        }

        internal static ZImage<byte> FromBitmap(Bitmap bitmap, bool grayscale)
        {
            byte[][,] data = ToBytes(bitmap);

            Color color;
            switch (bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    {
                        color = grayscale ? Color.Gray : Color.RGB;
                        if (grayscale)
                        {
                            data = ToGray(data);
                        }
                        break;
                    }
                case PixelFormat.Format8bppIndexed:
                    color = Color.Gray; break;
                default:
                    throw new NotSupportedException($"Not supported value:{bitmap.PixelFormat}");
            }
            
            return new ZImage<byte>(color, data);
        }

        public void ToCsv(string filename)
        {
            int rows = Rows;
            int cols = Cols;
            int channels = Channels;

            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                using (var writer = new StreamWriter(fs, Encoding.UTF8))
                {
                    StringBuilder sb = new StringBuilder();
                    for (int z = 0; z < channels; z++)
                    {
                        for (int i = 0; i < rows; i++)
                        {
                            sb.Clear();
                            for (int j = 0; j < cols; j++)
                            {
                                sb.Append(_data[RedIndex][i,j]).Append(',');
                            }
                            writer.WriteLine(sb);
                        }

                        writer.WriteLine();
                        writer.WriteLine();
                        writer.WriteLine();
                    }
                }
            }
        }

        private static byte[][,] ToGray(byte[][,] data)
        {
            int channels = data.Length;
            if (channels != 3)
            {
                throw new ArgumentException($"{nameof(data)} should be a rbg image");
            }

            int rows = data[0].GetLength(0);
            int cols = data[0].GetLength(1);

            byte[,] grayImgData = new byte[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int z = 0; z < cols; z++)
                {
                    //Grayscale  = 0.299R + 0.587G + 0.114B
                    double gray = 0.299 * data[0][i, z] + 0.587 * data[1][i, z] + 0.11 * data[2][i, z];
                    grayImgData[i, z] = (byte)Math.Ceiling(gray);
                }
            }

            return new byte[][,] { grayImgData };
        }

        public Bitmap ToBitmap()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ZImage<TDepth>));

            if (Depth != Depth.U8)
                throw new NotSupportedException($"Not supported depth {Depth}. Only {Depth.U8} is supported.");

            PixelFormat pxFormat;
            switch(Channels)
            {
                case 1: pxFormat = PixelFormat.Format8bppIndexed; break;
                case 3: pxFormat = PixelFormat.Format24bppRgb; break;
                default:
                    throw new NotSupportedException($"Not supporte number of channel {Channels}");
            }

            Bitmap bitmap = new Bitmap(Width, Height, pxFormat);

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
                Rectangle rect = new Rectangle(0, 0, Width, Height);
                bpdata = bitmap.LockBits(rect, ImageLockMode.WriteOnly, pxFormat);
                CopyToBitmapData(_data, Rows, Cols, pxFormat, bpdata);
                return bitmap;
            }
            finally
            {
                if(bpdata != null) bitmap.UnlockBits(bpdata);
            }
        }

        private static unsafe void CopyToBitmapData(TDepth[][,] imgdata, int rows, int cols, PixelFormat pixelFormat, BitmapData bpdata)
        {
            switch(pixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    {
                        byte* ptr = (byte*)bpdata.Scan0;
                        byte[] buffer = new byte[cols];
                        fixed (byte* bp = &buffer[0])
                        {
                            for (int i = 0; i < rows; i++)
                            {
                                fixed (void* imgp = &imgdata[0][i, 0])
                                {
                                    Buffer.MemoryCopy(imgp, bp, cols, cols);
                                    Buffer.MemoryCopy(bp, ptr, cols, cols);
                                    ptr += bpdata.Stride;
                                }
                            }
                        }
                        break;
                    }
                case PixelFormat.Format24bppRgb:
                    {
                        byte[] redBuf = new byte[cols];
                        byte[] greenBuf = new byte[cols];
                        byte[] blueBuf = new byte[cols];

                        byte* ptr = (byte*)bpdata.Scan0;
                        int stride = bpdata.Stride;

                        for (int i = 0; i < rows; i++)
                        {
                            ReadLine(imgdata[RedIndex], i, redBuf);
                            ReadLine(imgdata[GreenIndex], i, greenBuf);
                            ReadLine(imgdata[BlueIndex], i, blueBuf);

                            for (int j = 0; j < cols; j++)
                            {
                                int bias = j * 3;
                                *(ptr + bias) = blueBuf[j];
                                *(ptr + bias + 1) = greenBuf[j];
                                *(ptr + bias + 2) = redBuf[j];

                            }
                            
                            ptr += stride;
                        }
                        break;
                    }
                default:
                    throw new NotSupportedException($"Not supported pixel format {pixelFormat}");
            }
        }


        private static unsafe void ReadLine(TDepth[,] channeldata, int lineIndex, byte[] buffer)
        {
            int length = buffer.Length;
            fixed (byte* bp = &buffer[0])
            {
                fixed (void* cp = &channeldata[lineIndex, 0])
                {
                    Buffer.MemoryCopy(cp, bp, length, length);
                }
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
                        return ReadRgbBytes(bpdata);
                    case PixelFormat.Format8bppIndexed:
                        {
                            var palette = new byte[256];
                            var paletteLength = Math.Min(256, bitmap.Palette.Entries.Length);
                            for (int i = 0; i < paletteLength; i++)
                            {
                                palette[i] = bitmap.Palette.Entries[i].R;
                            }
                            return ReadGrayBytes(bpdata, palette);
                        }
                    default:
                        throw new NotSupportedException($"Not supported value:{bitmap.PixelFormat}");
                }
            }
            finally
            {
                if(bpdata != null) bitmap.UnlockBits(bpdata);
            }
        }

        private static unsafe byte[][,] ReadGrayBytes(BitmapData bpdata, byte[] palette)
        {
            int rows = bpdata.Height;
            int cols = bpdata.Width;

            byte[,] chr = new byte[rows, cols]; //red channel

            byte* sp = (byte*)bpdata.Scan0;
            int stride = bpdata.Stride;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    chr[i, j] = palette[(*(sp + j))];
                }
                sp += stride;
            }


            //byte[] buffer = new byte[stride];

            //fixed (byte* bp = &buffer[0])
            //{
            //    for (int i = 0; i < rows; i++)
            //    {
            //        //for (int j = 0; j < cols; j++)
            //        //{
            //        //    chr[i, j] = palette[(*sp)];
            //        //    sp += 1;
            //        //}

            //        Buffer.MemoryCopy(sp, bp, stride, stride);
            //        for (int z = 0; z < buffer.Length; z++)
            //        {
            //            buffer[z] = palette[buffer[z]];
            //        }

            //        fixed(byte* chrp = &chr[i, 0])
            //        {
            //            Buffer.MemoryCopy(bp, chrp, stride, stride);
            //        }
            //    }
            //}

            return new byte[][,] { chr };
        }

        private static unsafe byte[][,] ReadRgbBytes(BitmapData bpdata)
        {
            int rows = bpdata.Height;
            int cols = bpdata.Width;

            byte[,] chr = new byte[rows, cols]; //red channel
            byte[,] chg = new byte[rows, cols]; //green channel
            byte[,] chb = new byte[rows, cols]; //blue channel

            byte[] br = new byte[cols]; 
            byte[] bg = new byte[cols]; 
            byte[] bb = new byte[cols]; 
 
            byte* sp = (byte*)bpdata.Scan0;
            int stride = bpdata.Stride;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    int bias = j * 3;
                    byte blue = *(sp + bias);
                    byte green = *(sp + bias + 1);
                    byte red = *(sp + bias + 2);

                    chr[i, j] = red;
                    chg[i, j] = green;
                    chb[i, j] = blue;
                }
                sp += stride;
            }

            return new byte[][,] { chr, chg, chb };
        }
    }
}
