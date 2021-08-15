using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core
{
    public static class ZImage
    {

        public static ZImage<byte> Read(string filename, bool asGrayscale = false)
        {
            return ZImage<byte>.Read(filename, asGrayscale);
        }

        public static ZImage<byte> FromBitmap(Bitmap bitmap, bool asGrayscale = false)
        {
            return ZImage<byte>.FromBitmap(bitmap, asGrayscale);
        }
    }
}
