using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GithubSync
{
    public class AppInfo
    {
        public static Lazy<AppInfo> Instance = new Lazy<AppInfo>();

        public string Username { get; set; }
        public string Password { get; set; }

        public bool IsClosing { get; set; }
    }
}
