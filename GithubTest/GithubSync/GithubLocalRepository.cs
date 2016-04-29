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
        public string LocalPath { get; set; }

        public IList<GithubReference> References { get; private set; } = new List<GithubReference>();

        public IList<RepositoryContent> Contents { get; set; }

        public bool IsMapping
        {
            get
            {
                return !string.IsNullOrWhiteSpace(LocalPath);
            }
        }
    }
}
