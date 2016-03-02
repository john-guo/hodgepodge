using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GithubSync
{
    public class GithubItem
    {
        public string Name { get; set; }

        public string Sha { get; set; }

        public int Size { get; set; }

        public Uri Url { get; set; }

        public string Encoding { get; set; }

        public string Path { get; set; }

        public string Target { get; set; }

        public ContentType Type { get; set; }

        public string Content { get; set; }
        
        public string EncodedContent { get; set; }

        public IList<GithubItem> Items { get; set; }

        public bool IsFile
        {
            get
            {
                return Type == ContentType.File;
            }
        }

        public bool IsTextFile
        {
            get
            {
                if (!IsFile)
                    return false;

                if (string.IsNullOrWhiteSpace(Encoding))
                    return true;

                return Encoding != Constants.base64;
            }
        }

        public string TextContent
        {
            get
            {
                if (Encoding == Constants.base64)
                    return null;

                return Content;
            }
        }

        public byte[] BinaryContent
        {
            get
            {
                if (Encoding != Constants.base64)
                    return null;

                if (string.IsNullOrWhiteSpace(EncodedContent))
                    return null;

                return Convert.FromBase64String(EncodedContent);
            }
        }
    }
}
