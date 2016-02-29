using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;

namespace GithubSync
{
    public class GithubCommit : GithubItem
    {
        public override string Mode
        {
            get
            {
                return Constants.TreeMode.Commit;
            }
        }

        public override TreeType Type
        {
            get
            {
                return TreeType.Commit;
            }
        }

        public IList<GithubItem> Items { get; private set; } = new List<GithubItem>();
    }
}
