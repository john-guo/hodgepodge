using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GithubSync
{
    public sealed class Constants
    {
        public const string AppName = "GithubSync";
        public const string CryptoCfg = "crypto.cfg";
        public const string MasterRef = @"refs/heads/master";
        public const string Root = @"/";

        public sealed class TreeMode
        {
            public const string Text = "100644";
            public const string Binary = "100755";
            public const string Directory = "040000";
            public const string Commit = "160000";
            public const string SymbolLink = "120000";
        }
    }
}
