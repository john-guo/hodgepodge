using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletScreen
{
    public abstract class BulletSource : IBulletSource
    {
        protected BulletSource()
        {
        }

        public abstract IEnumerable<BulletMessage> GetMessage();
        public abstract void Initialize();
    }
}
