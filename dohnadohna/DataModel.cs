using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace dohnadohna
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class IniNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public IniNameAttribute(string name)
        {
            Name = name;
        }
    }

    public class ContentData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string Content { get; set; }
    }

    public abstract class DataModel : INotifyPropertyChanged
    {
        protected const int MaxProfilesCount = 3;
        public event PropertyChangedEventHandler PropertyChanged;

        [IniName("画像")]
        public string Image { get; set; }

        [IniName("名字")]
        public string Name { get; set; }

        [IniName("个人资料")]
        public string[] _Profiles 
        { 
            get
            {
                return Profiles.Select(item => item.Content).ToArray();
            }
            set
            {
                Profiles = value.Select(item => new ContentData { Content = item }).ToArray();
            }
        }

        public ContentData[] Profiles { get; set; }

        public abstract List<string> Attributes { get; set; }

        protected DataModel()
        {
            Profiles = new ContentData[MaxProfilesCount];
            for (int i = 0; i < MaxProfilesCount; ++i)
            {
                Profiles[i] = new ContentData() { Content = "" };
            }
        }
    }

    public class StaffModel : DataModel
    {
        [IniName("容貌")]
        public string Appearance { get; set; }

        [IniName("技巧")]
        public string Skill { get; set; }

        [IniName("精神")]
        public string Spirit { get; set; }

        [IniName("属性")]
        public override List<string> Attributes { get; set; }

        [IniName("处女")]
        public string _IsVirgin 
        { 
            get
            {
                return IsVirgin ? "1" : "0";
            }
            set
            {
                IsVirgin = value == "1";
            }
        }
        public bool IsVirgin { get; set; }

        [IniName("语音")]
        public string Voice { get; set; }
    }

    public class CustomerModel : DataModel
    {
        public const string CustomerUniqueName = "种类";

        [IniName(CustomerUniqueName)]
        public string Type { get => "顾客"; set { } }

        [IniName("收入")]
        public string Income { get; set; }

        [IniName("礼物")]
        public string Present { get; set; }

        [IniName("目标")]
        public override List<string> Attributes { get; set; }
    }

    public class Level
    {
        public string Value { get; set; }
        public string Name { get; set; }
    }


}
