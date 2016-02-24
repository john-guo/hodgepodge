using System;
using HttpServer;
using System.Net;

namespace Test
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			HttpServer.HttpServer server = new HttpServer();
			server.Start (IPAddress.Any, 80);
			server.Add (new HttpServer.HttpModules.FileModule ("/", Environment.CurrentDirectory));
			Console.WriteLine ("Hello World!");
			server.Stop ();
		}
	}
}
