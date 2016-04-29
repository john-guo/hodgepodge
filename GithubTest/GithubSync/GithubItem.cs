using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GithubSync
{
    public abstract class GithubItem
    {
        public string Name { get; set; }

        public string Sha { get; set; }

        public string Url { get; set; }

        public virtual string Mode { get; set; }

        public virtual TreeType Type { get; set; }
    }
}
