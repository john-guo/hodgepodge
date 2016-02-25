using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;

namespace GithubTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            Task.Run(() => Test()).Wait();
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            ;
        }

        void CreateRepository(string name)
        {
            var nrep = new NewRepository("test");
            //nrep.AutoInit = true;
            try
            {
                //client.Repository.Create(nrep).Wait();
                //var rep = await client.Repository.Create(nrep);

            }
            catch (Exception e)
            {
                ;
            }
        }

        static async void Test()
        {
            var client = new GitHubClient(new ProductHeaderValue("Test"));
            var username = "john-guo";
            var password = "q1w2e3r4`";
            client.Credentials = new Credentials(username, password);
           
            var content = await client.Repository.Content.GetAllContents(username, "test");
        }
    }
}
