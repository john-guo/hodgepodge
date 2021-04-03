using Microsoft.Win32;
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
    public abstract class BaseViewModel<T> : INotifyPropertyChanged where T : DataModel, new() 
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ApplyAttributes()
        {
            if (Data?.Attributes == null || Attributes == null)
                return;
            foreach (var attr in data.Attributes)
            {
                var i = Attributes.Where(item => item.Item == attr).FirstOrDefault();
                if (i != null)
                    i.IsSelected = true;
            }
        }

        private SelectionItem[] attributes;
        public SelectionItem[] Attributes 
        {
            get => attributes;
            set
            {
                attributes = value;
                ApplyAttributes();
                OnPropertyChanged();
            }
        }

        private T data;
        public T Data 
        {
            get => data;
            set
            {
                data = value;
                ApplyAttributes();
                OnPropertyChanged();
            }
        }

        public bool IsShowAttributes { get; set; }

        public DelegateCommand OpenImage { get; }

        public DelegateCommand ShowAttributes { get; }

        public DelegateCommand HideAttributes { get; }

        public DelegateCommand ChangeImage { get; }

        public DelegateCommand Create { get; set; }

        protected BaseViewModel()
        {
            var items = (SelectionItem[])Application.Current.FindResource("attributes");
            Attributes = new SelectionItem[items.Length];
            for (int i = 0; i < items.Length; ++i)
            {
                Attributes[i] = new SelectionItem()
                {
                    Item = items[i].Item,
                    IsSelected = items[i].IsSelected,
                    GetCollection = () => Attributes
                };
            }

            Data = new T();

            OpenImage = new DelegateCommand()
            {
                OnExecute = obj =>
                {
                    var dialog = new OpenFileDialog()
                    {
                        DefaultExt = "png",
                        Filter = "图片|*.jpg;*.png;*.bmp",
                    };
                    if (dialog.ShowDialog() == true)
                    {
                        Data.Image = dialog.FileName;
                    }
                }
            };

            ShowAttributes = new DelegateCommand()
            {
                OnExecute = obj =>
                {
                    IsShowAttributes = true;
                }
            };

            HideAttributes = new DelegateCommand()
            {
                OnExecute = obj =>
                {
                    Data.Attributes = Attributes.Where(item => item.IsSelected).Select(item => item.Item).ToList();
                }
            };

            ChangeImage = new DelegateCommand()
            {
                OnExecute = obj =>
                {
                    var img = new Image(Data.Image);
                    img.ShowDialog();
                    var filename = Data.Image;
                    Data.Image = string.Empty;
                    Data.Image = filename;
                }
            };

            Create = new DelegateCommand()
            {
                OnExecute = obj =>
                {
                    if (string.IsNullOrWhiteSpace(Data.Name))
                    {
                        MessageBox.Show("名字不能为空");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(Data.Image))
                    {
                        MessageBox.Show("图片不能为空");
                        return;
                    }
                    StorageService.Save(Data);
                    MessageBox.Show("保存成功");
                },
                OnCanExecute = obj =>
                {
                    return true;
                    //return !string.IsNullOrWhiteSpace(Data.Name);
                }
            };
        }
    }
}
