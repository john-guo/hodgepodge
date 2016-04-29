using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;

namespace GithubSync
{
    public class GithubTree : GithubPath
    {
        public override TreeType Type
        {
            get
            {
                return TreeType.Tree;
            }
        }

        public override string Mode
        {
            get
            {
                return Constants.TreeMode.Directory;
            }
        }

        public IList<GithubItem> Items { get; private set; } = new List<GithubItem>();
    }
}
