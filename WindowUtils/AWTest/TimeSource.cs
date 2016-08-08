using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AWTest
{
    public class TimeSource : INotifyPropertyChanged
    {
        public TimeSource()
        {
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        public string NowTime
        {
            get
            {
                return DateTime.Now.ToLongTimeString();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            PropertyChanged(this, new PropertyChangedEventArgs("NowTime"));
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
