using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core
{
    public interface IImage : IDisposable
    {
        int Rows { get; }
        int Cols { get; }
        int Channels { get; }
        int Width { get; }
        int Height { get; }
        Depth Depth { get; }
        Color Color { get; }
    }
}
