using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;

namespace GithubSync
{
    public class Github
    {
        private GitHubClient client;

        public Github(string username, string password)
        {
            client = new GitHubClient(new ProductHeaderValue(Constants.AppName));
            client.Credentials = new Credentials(username, password);
        }
    }
}
