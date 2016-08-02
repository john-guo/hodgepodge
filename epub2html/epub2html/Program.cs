using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using eBdb.EpubReader;
using System.Diagnostics;

namespace epub2html
{
    public class Program
    {
        static void Main(string[] argv)
        {
            if (argv.Length == 0)
                return;

            try
            {
                var path = argv[0];
                var html = Path.ChangeExtension(path, "html");
                var epub = new Epub(argv[0]);
                if (!epub.Title.Any())
                {
                    epub.Title.Add(Path.GetFileNameWithoutExtension(path));
                }
                File.WriteAllText(html, epub.GetContentAsHtml());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
