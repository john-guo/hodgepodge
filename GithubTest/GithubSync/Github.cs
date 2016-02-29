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
        private Dictionary<string, Dictionary<Reference, IList<GithubFile>>> _cachedFiles;
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

        public async Task<bool> InitializeReferences(string name)
        {
            GithubLocalRepository repository;
            if (!_repositoris.TryGetValue(name, out repository))
                return false;

            try
            {
                var refs = await client.Git.Reference.GetAll(Login, name);
                foreach (var r in refs)
                {
                    var reference = new GithubReference()
                    {
                        Name = r.Ref,
                        Sha = r.Object.Sha,
                        Url = r.Url
                    };
                    repository.References.Add(reference);
                }
            }
            catch
            {
                var r = await client.Git.Reference.Create(Login, name,
                        new NewReference(Constants.MasterRef, Crypto.GetSHA1(name)));
                var reference = new GithubReference()
                {
                    Name = r.Ref,
                    Sha = r.Object.Sha,
                    Url = r.Url
                };
                repository.References.Add(reference);
            }

            return true;
        }


        public async Task<IReadOnlyList<RepositoryContent>> GetContants(string name, string path = Constants.Root)
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
                    _repositoris.Add(
                        item.Name,
                        new GithubLocalRepository()
                        {
                            Name = item.Name,
                            LocalPath = item.Path
                        }
                    );
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

        public async Task<bool> SyncLocal(string name)
        {
            GithubLocalRepository repository;
            if (!_repositoris.TryGetValue(name, out repository))
                return false;

            try
            {
                foreach (var r in repository.References)
                {
                    var commit = await client.Git.Commit.Get(Login, name, r.Sha);

                    var root = new GithubTree()
                    {
                        Name = commit.Label,
                        Sha = commit.Tree.Sha,
                        Url = commit.Tree.Url,
                        Path = Constants.Root
                    };

                    r.Root = root;

                    var tree = await client.Git.Tree.GetRecursive(Login, name, root.Sha);

                    var list = new List<GithubFile>();

                    foreach (var item in tree.Tree)
                    {
                        GithubItem subItem = null;

                        switch (item.Type)
                        {
                            case TreeType.Blob:
                                var blob = await client.Git.Blob.Get(Login, name, item.Sha);
                                subItem = new GithubFile()
                                {
                                    Name = Path.GetFileName(item.Path),
                                    Path = item.Path,
                                    Size = blob.Size,
                                    Sha = blob.Sha,
                                    Url = item.Url.AbsoluteUri,
                                    Encoding = blob.Encoding
                                };
                                break;
                            case TreeType.Tree:
                                subItem = new GithubTree()
                                {
                                    Name = Path.GetDirectoryName(item.Path),
                                    Path = item.Path,
                                    Sha = item.Sha,
                                    Url = item.Url.ToString()
                                };
                                break;
                            case TreeType.Commit:
                                subItem = new GithubCommit()
                                {
                                    Name = item.Path,
                                    Sha = item.Sha
                                };
                                break;
                        }

                        root.Items.Add(subItem);
                    }

                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreateDirectory(string name, string path)
        {
            GithubLocalRepository repository;
            if (!_repositoris.TryGetValue(name, out repository))
                return false;

            try
            {
                var fullPath = Path.Combine(repository.LocalPath, path);
                Directory.CreateDirectory(fullPath);

                var tree = new GithubTree()
                {
                    Name = path,
                    Path = path,
                };

                var nt = new NewTree();
                nt.Tree.Add(new NewTreeItem()
                {
                    Path = path,
                    Mode = tree.Mode,
                    Type = tree.Type
                });

                var subtree = await client.Git.Tree.Create(Login, name, nt);

                tree.Sha = subtree.Sha;
                tree.Url = subtree.Url.ToString();
                repository.References.First(r => r.Name == name).Root.Items.Add(tree);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IDictionary<Reference, IList<GithubFile>>> GetFiles(string name)
        {
            if (_cachedFiles == null)
                _cachedFiles = new Dictionary<string, Dictionary<Reference, IList<GithubFile>>>();

            if (_cachedFiles.ContainsKey(name))
                return _cachedFiles[name];

            var ret = new Dictionary<Reference, IList<GithubFile>>();

            try
            {
                var refs = await client.Git.Reference.GetAll(Login, name);
                foreach (var r in refs)
                {
                    var commit = await client.Git.Commit.Get(Login, name, r.Object.Sha);

                    var tree = await client.Git.Tree.GetRecursive(Login, name, commit.Tree.Sha);

                    var list = new List<GithubFile>();

                    foreach (var item in tree.Tree)
                    {
                        if (item.Type != TreeType.Blob)
                            continue;

                        var blob = await client.Git.Blob.Get(Login, name, item.Sha);

                        list.Add(new GithubFile()
                        {
                            Name = item.Path,
                            Path = item.Path,
                            Size = blob.Size,
                            Sha = blob.Sha,
                            Url = item.Url.ToString(),
                            Encoding = blob.Encoding
                        });
                    }

                    ret.Add(r, list);
                }

                _cachedFiles.Add(name, ret);
                return ret;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> AddFile(string name, string fileName, string content, string comment, EncodingType type)
        {
            try
            {
                if (_cachedFiles == null)
                    await GetFiles(name);

                Reference reference;
                if (!_cachedFiles.ContainsKey(name))
                {
                    reference = await client.Git.Reference.Create(Login, name,
                        new NewReference(Constants.MasterRef, Crypto.GetSHA1(name)));
                }
                else
                {
                    reference = _cachedFiles[name].First().Key;
                }

                var nb = new NewBlob();
                nb.Encoding = type;
                nb.Content = content;
                var blob = await client.Git.Blob.Create(Login, name, nb);

                var nt = new NewTree();
                nt.Tree.Add(new NewTreeItem()
                {
                    Content = blob.Sha,
                    Path = fileName,
                    Mode = type == EncodingType.Utf8 ? "100644" : "100755",
                    Type = TreeType.Blob
                });
                var tree = await client.Git.Tree.Create(Login, name, nt);

                var nc = new NewCommit(comment, tree.Sha);
                var commit = await client.Git.Commit.Create(Login, name, nc);

                reference = await client.Git.Reference.Update(Login, name, reference.Ref, new ReferenceUpdate(reference.Object.Sha));

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddTextFile(string name, string fileName, string comment)
        {
            var content = File.ReadAllText(fileName);

            return await AddFile(name, fileName, content, comment, EncodingType.Utf8);
        }

        public async Task<bool> AddBinaryFile(string name, string fileName, string comment)
        {
            var content = Convert.ToBase64String(File.ReadAllBytes(fileName));
                        
            return await AddFile(name, fileName, content, comment, EncodingType.Base64);
        }
    }
}
