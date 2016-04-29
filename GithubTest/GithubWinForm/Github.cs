using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using System.IO;
using System.Xml.Linq;
using System.Net;

namespace GithubSync
{
    public class Github
    {
        private GitHubClient client;
        private IReadOnlyList<Repository> _cachedRepositoris;
        private Dictionary<string, GithubLocalRepository> _repositoris;

        public string WorkSpace { get; set; }

        public string GetPath(string name)
        {
            return Path.Combine(WorkSpace, this[name].Name);
        }

        public GithubLocalRepository this[string name]
        {
            get
            {
                return _repositoris[name];
            }
        }

        private string Login
        {
            get
            {
                return client.Credentials.Login;
            }
        }

        public Github(string username, string password)
        {
            client = new GitHubClient(new ProductHeaderValue(Constants.AppName));
            SetCredentials(username, password);

            _repositoris = new Dictionary<string, GithubLocalRepository>();
        }

        public void SetCredentials(string username, string password)
        {
            client.Credentials = new Credentials(username, password);
        }

        private async Task<IReadOnlyList<RepositoryContent>> GetContants(string name, string path = Constants.Root)
        {
            try
            {
                return await client.Repository.Content.GetAllContents(Login, name, path);
            }
            catch
            {
                return null;
            }
        }

        public async Task<IReadOnlyList<Repository>> GetRepositoris()
        {
            try
            {
                if (_cachedRepositoris == null)
                {
                    _cachedRepositoris = await client.Repository.GetAllForCurrent();

                    foreach (var rep in _cachedRepositoris)
                    {
                        var repository = new GithubLocalRepository()
                        {
                            Name = rep.Name,
                        };

                        _repositoris.Add(rep.Name, repository);
                    }
                }

                return _cachedRepositoris;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> RemoveRepository(string name)
        {
            try
            {
                await client.Repository.Delete(Login, name);
                _cachedRepositoris = _cachedRepositoris.Where(r => r.Name != name).ToList();
                _repositoris.Remove(name);

                return true;
            }
            catch
            {
                return false;
            }

        }

        public async Task<bool> NewRepository(string name)
        {
            try
            {

                var nr = new NewRepository(name);
                var rep = await client.Repository.Create(nr);
                _cachedRepositoris = _cachedRepositoris.Union(new[] { rep }).ToList();

                var repository = new GithubLocalRepository()
                {
                    Name = rep.Name,
                };

                _repositoris.Add(rep.Name, repository);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IList<GithubItem>> GetItems(string name, string path = Constants.Root)
        {
            List<GithubItem> items = new List<GithubItem>();

            var contants = await GetContants(name, path);
            if (contants == null)
                return null;
            foreach (var item in contants)
            {
                var gitem = new GithubItem()
                {
                    Name = item.Name,
                    Sha = item.Sha,
                    Size = item.Size,
                    Url = item.DownloadUrl,
                    Encoding = item.Encoding,
                    Target = item.Target,
                    Type = item.Type,
                    Path = item.Path,
                    Content = item.Content,
                    EncodedContent = item.EncodedContent
                };

                items.Add(gitem);
            }

            return items;
        }

        public async Task<bool> SyncAll(string name)
        {
            var rep = _repositoris[name];
            if (!rep.IsDirty)
                return true;

            rep.Items = await GetItems(name);

            if (rep.Items == null)
            {
                rep.IsDirty = false;
                return true;
            }


            foreach (var item in rep.Items)
            {
                await DownloadItems(name, item);
            }

            rep.IsDirty = false;
            return true;
        }

        private async Task<bool> DownloadItems(string name, GithubItem item)
        {
            if (item.IsFile)
                return true;

            item.Items = await GetItems(name, item.Path);
            foreach (var i in item.Items)
            {
                await DownloadItems(name, i);

            }

            return true;
        }

        private void RecursionAction(IList<GithubItem> items, Action<GithubItem> action, Func<GithubItem, bool> predicate)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                action(item);
                if (predicate(item))
                    RecursionAction(item.Items, action, predicate);
            }
        }

        public void RecursionAction(IList<GithubItem> items, Action<GithubItem> action)
        {
            RecursionAction(items, action, item => !item.IsFile);
        }

        public TaskTest.JobScheduler SaveContents(string name)
        {
            var scheduler = new TaskTest.JobScheduler();

            var rep = _repositoris[name];
            RecursionAction(rep.Items, item =>
            {
                try
                {
                    var repRoot = Path.Combine(WorkSpace, rep.Name);

                    if (!Directory.Exists(repRoot))
                    {
                        Directory.CreateDirectory(repRoot);
                    }

                    var fullpath = Path.Combine(repRoot, item.Path);

                    if (item.IsFile)
                    {
                        scheduler.PenddingJob(job =>
                        {
                            var web = new WebClient();
                            web.DownloadFile(item.Url, fullpath);
                        });
                    }
                    else
                    {
                        Directory.CreateDirectory(fullpath);
                    }
                }
                catch (Exception ex)
                {
                    ;
                }
            });

            return scheduler;
        }


    }
}
