using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace dohnadohna
{
    public class SelectionItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Func<SelectionItem[]> GetCollection;

        //private string item;
        //public string Item 
        //{
        //    get => item;
        //    set
        //    {
        //        item = value;
        //        OnPropertyChanged();
        //    }
        //}
        public string Item { get; set; }


        private bool isSelected;
        public bool IsSelected 
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged();

                foreach (var item in GetCollection?.Invoke())
                {
                    item.OnPropertyChanged("IsEnabled");
                }
            }
        }

        public bool IsEnabled 
        { 
            get
            {
                var items = GetCollection?.Invoke();
                if (items == null)
                {
                    return IsSelected;
                }

                return IsSelected || items.Count(item => item.IsSelected) < 3;
            }
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
