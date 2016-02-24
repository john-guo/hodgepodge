using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OBB
{
    public class CyberSpace : InputScript
    {
        public override void Go(Process proc)
        {
            new Task(() =>
            {
                proc.WaitForInputIdle();
                SendKeys.SendWait("{Enter}");
                proc.WaitForInputIdle();
                SendKeys.SendWait(" ");
            }).Start();
        }
    }
}
