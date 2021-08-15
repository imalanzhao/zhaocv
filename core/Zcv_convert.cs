using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core
{
    public partial class Zcv
    {
        public byte[][,] ToGrayScale(byte[][,] color)
        {
            int channels = color.Length;
            if(channels != 3)
            {
                throw new ArgumentException($"{nameof(color)} should be a rbg image");
            }

            int rows = color[0].GetLength(0);
            int cols = color[0].GetLength(1);

            byte[,] grayImgData = new byte[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int z = 0; z < cols; z++)
                {
                    //Grayscale  = 0.299R + 0.587G + 0.114B
                    double gray = 0.299 * color[0][i, z] + 0.587 * color[1][i, z] + 0.11 * color[2][i, z];
                    grayImgData[i, z] = (byte)Math.Ceiling(gray);
                }
            }

            return new byte[][,] { grayImgData };
        }
    }
}
