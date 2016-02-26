using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GithubSync
{
    public class GitFile
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public string Sha { get; set; }

        public int Size { get; set; }

        public Uri Url { get; set; }

        public Octokit.EncodingType Encoding { get; set; }
    }
}
