using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroWatch
{
    public class BulletScreen : TransparentForm
    {
        internal BulletManager bm;

        public BulletScreen()
        {
            bm = new BulletManager(ClientRectangle);
        }

        protected override void OnDraw(Graphics canvas)
        {
            bm.Update(canvas);   
        }

        protected override Rectangle OnResizeCanvas()
        {
            bm.Bounds = ClientRectangle;
            return ClientRectangle;
        }

        protected override void OnCanvasCreated(Graphics canvas)
        {
            bm.UpdateLayout(canvas);
        }
    }
}
