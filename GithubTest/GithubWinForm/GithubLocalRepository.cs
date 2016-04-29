using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GithubSync
{
    public class GithubLocalRepository
    {
        public string Name { get; set; }
        public bool IsDirty { get; set; } = true;

        public IList<GithubItem> Items { get; set; }
    }
}
