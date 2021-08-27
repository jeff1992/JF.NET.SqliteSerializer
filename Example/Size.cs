using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example
{
    [Serializable]
    public struct Size
    {
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
        public int Width;
        public int Height;
    }
}
