using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletScreen
{
    public abstract class BulletSource : IBulletSource
    {
        private bool played;
        protected long playTime;

        protected BulletSource()
        {
            playTime = 0;
        }

        public abstract IEnumerable<BulletMessage> GetMessage();

        public virtual void Pause()
        {
            played = false;
        }

        public virtual void Start()
        {
            played = true;
        }

        public virtual void Stop()
        {
            played = false;
            playTime = 0;
        }
    }
}
