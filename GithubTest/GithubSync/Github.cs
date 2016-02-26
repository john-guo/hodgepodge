using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using System.IO;

namespace GithubSync
{
    public class Github
    {
        private GitHubClient client;
        private IReadOnlyList<Repository> _cachedRepositoris;
        private Dictionary<string, Dictionary<Reference, IList<GitFile>>> _cachedFiles;

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
        }

        public void SetCredentials(string username, string password)
        {
            client.Credentials = new Credentials(username, password);
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

        public async Task<IDictionary<Reference, IList<GitFile>>> GetFiles(string name)
        {
            if (_cachedFiles == null)
                _cachedFiles = new Dictionary<string, Dictionary<Reference, IList<GitFile>>>();

            if (_cachedFiles.ContainsKey(name))
                return _cachedFiles[name];

            var ret = new Dictionary<Reference, IList<GitFile>>();

            try
            {
                var refs = await client.Git.Reference.GetAll(Login, name);
                foreach (var r in refs)
                {
                    var commit = await client.Git.Commit.Get(Login, name, r.Object.Sha);

                    var tree = await client.Git.Tree.GetRecursive(Login, name, commit.Tree.Sha);

                    var list = new List<GitFile>();

                    foreach (var item in tree.Tree)
                    {
                        if (item.Type != TreeType.Blob)
                            continue;

                        var blob = await client.Git.Blob.Get(Login, name, item.Sha);

                        list.Add(new GitFile()
                        {
                            Name = item.Path,
                            Path = item.Path,
                            Size = blob.Size,
                            Sha = blob.Sha,
                            Url = item.Url,
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
