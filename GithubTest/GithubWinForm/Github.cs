using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using System.IO;
using System.Xml.Linq;

namespace GithubSync
{
    public class Github
    {
        private GitHubClient client;
        private IReadOnlyList<Repository> _cachedRepositoris;
        private Dictionary<string, GithubLocalRepository> _repositoris;

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

        public bool IsMapping(string name)
        {
            GithubLocalRepository repository;
            if (!_repositoris.TryGetValue(name, out repository))
                return false;

            return repository.IsMapping;
        }

        public bool Mapping(string name, string localPath)
        {

            if (!Directory.Exists(localPath))
            {
                try
                {
                    Directory.CreateDirectory(localPath);
                }
                catch
                {
                    return false;
                }
            }

            GithubLocalRepository repository;
            if (!_repositoris.TryGetValue(name, out repository))
            {
                repository = new GithubLocalRepository()
                {
                    Name = name,
                    LocalPath = localPath
                };

                _repositoris.Add(name, repository);
            }
            else
            {
                repository.LocalPath = localPath;
            }

            return true;
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

        public bool Unmapping(string name)
        {
            GithubLocalRepository repository;
            if (!_repositoris.TryGetValue(name, out repository))
                return false;

            repository.LocalPath = string.Empty;
            return true;
        }

        public XDocument SaveLocal()
        {
            XDocument doc = new XDocument();
            foreach (var pair in _repositoris)
            {
                if (!pair.Value.IsMapping)
                    continue;

                doc.Add(new XElement(XName.Get("Entry"),
                    new XAttribute(XName.Get("name"), pair.Value.Name),
                    new XAttribute(XName.Get("path"), pair.Value.LocalPath)
                    ));
            }

            return doc;
        }

        public bool LoadLocal(XDocument doc)
        {
            if (doc == null)
                return false;

            var query = from e in doc.Elements(XName.Get("Entry"))
                        select new
                        {
                            Name = e.Attributes("name").First().Value,
                            Path = e.Attributes("path").First().Value
                        };

            try
            {
                foreach (var item in query)
                {
                    _repositoris[item.Name] =
                        new GithubLocalRepository()
                        {
                            Name = item.Name,
                            LocalPath = item.Path
                        };
                }
                return true;
            }
            catch
            {
                return false;
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
                Unmapping(name);
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
            _repositoris[name].Items = await GetItems(name);

            if (_repositoris[name].Items == null)
                return true;


            foreach (var item in _repositoris[name].Items)
            {
                await DownloadItems(name, item);
            }

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

        public void SaveContents(string name)
        {
            var rep = _repositoris[name];
            RecursionAction(rep.Items, item =>
            {
                try
                {
                    var fullpath = Path.Combine(rep.LocalPath, item.Path);

                    if (item.IsFile)
                    {
                        if (item.IsTextFile)
                        {
                            File.WriteAllText(fullpath, item.TextContent);
                        }
                        else
                        {
                            File.WriteAllBytes(fullpath, item.BinaryContent);
                        }
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
        }
    }
}
