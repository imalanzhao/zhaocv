using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core
{
    public partial class ZImage<TDepth> : IImage where TDepth : unmanaged
    {
        private bool _disposed = false;
        internal const int RedIndex = 0;
        internal const int GreenIndex = 1;
        internal const int BlueIndex = 2;

        private TDepth[][,] _data;
        public int Rows { get; private set;  }

        public int Cols { get; private set; } 

        public int Channels { get; private set; }

        public int Width { get { return Cols; } }

        public int Height { get { return Rows; } }

        public Depth Depth { get; private set; }

        public Color Color { get; private set; }
        private ZImage(Color color, TDepth[][,] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            int channel = data.Length;
            if (channel == 0) throw new ArgumentOutOfRangeException(nameof(data), "No channel data");

            if(color == Color.Gray)
            {
                if(channel != 1) throw new ArgumentException($"Grayscale image can't have more than 1 channel data");
                Rows = data[0].GetLength(0);
                Cols = data[0].GetLength(1);
                Channels = 1;
            }
            else if(color == Color.RGB )
            {
                if(channel != 3) throw new ArgumentException($"RGB image mus have 3 channels data");

                int rowsR = data[0].GetLength(0);
                int colsR = data[0].GetLength(1);

                int rowsG = data[1].GetLength(0);
                int colsG = data[1].GetLength(1);

                int rowsB = data[2].GetLength(0);
                int colsB = data[2].GetLength(1);

                if(rowsR != rowsG || rowsR != rowsB)
                {
                    throw new ArgumentException($"The rows of channel data not equal each other");
                }

                if (colsR != colsG || colsR != colsB)
                {
                    throw new ArgumentException($"The columns of channel data not equal each other");
                }

                Rows = rowsR;
                Cols = colsR;
                Channels = 3;
            }

            var type = typeof(TDepth);
            if (type == typeof(byte)) Depth = Depth.U8;
            else if (type == typeof(UInt16)) Depth = Depth.UInt16;
            else if (type == typeof(Int16)) Depth = Depth.Int16;
            else if (type == typeof(Int32)) Depth = Depth.Int32;
            else if (type == typeof(UInt32)) Depth = Depth.UInt32;
            else if (type == typeof(float)) Depth = Depth.Float;
            else if (type == typeof(double)) Depth = Depth.Double;
            else throw new NotSupportedException($"Not supported depth type {type}");

            _data = data;
        }

        public void Dispose()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ZImage<TDepth>));

            _data = null;
            _disposed = true;
        }
    }
}
