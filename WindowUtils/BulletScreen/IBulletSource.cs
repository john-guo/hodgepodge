using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletScreen
{
    public class BulletMessage
    {
        public string Content { get; set; }
        public int Delay { get; set; }
    }

    public interface IBulletSource
    {
        void Start();
        void Stop();
        void Pause();
        IEnumerable<BulletMessage> GetMessage();
    }
}
