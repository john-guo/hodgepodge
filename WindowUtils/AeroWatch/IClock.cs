using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroWatch
{
    public interface IClock
    {
        Size GetSize();
        void Draw(DateTime time);
    }

    public interface IClock<T> : IClock
    {
        void Initialize(T obj, string fileName);
    }
}
