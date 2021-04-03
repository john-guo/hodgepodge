using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace dohnadohna
{
    public class CustomerViewModel : BaseViewModel<CustomerModel>
    {
        public string[] Presents { get; set; }

        public CustomerViewModel()
        {
            var items = (SelectionItem[])Application.Current.FindResource("attributes");
            var presents = (string[])Application.Current.FindResource("presents");
            Presents = presents.Union(items.Select(item => item.Item)).ToArray();
        }
    }
}
