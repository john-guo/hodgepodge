using HttpServer.HttpModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HttpServer;
using HttpServer.Sessions;
using System.IO;
using System.Net;
using System.Diagnostics;
using Proc = System.Diagnostics.Process;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using GameSettings = System.Collections.Generic.Dictionary<string, OBB.GameSetting>;
using GameInfoTable = System.Collections.Generic.Dictionary<string, OBB.GameInfo>;
using System.Threading.Tasks;
using System.Threading;

namespace OBB
{
    public class MyModule : HttpModule
    {
        private delegate bool Func(IHttpRequest request, IHttpResponse response);
        private const string prefix = "/op/";
        private Dictionary<string, Func> masterHandlers;
        private Dictionary<string, Func> slaveHandlers;

        public MyModule()
        {
            masterHandlers = new Dictionary<string, Func>();
            slaveHandlers = new Dictionary<string, Func>();

            masterHandlers[@""] = Root;
            slaveHandlers[@""] = Root;

            slaveHandlers[@"query"] = Query;
            slaveHandlers[@"start"] = StartGame;
            slaveHandlers[@"stop"] = StopGame;
            slaveHandlers[@"reload"] = Reload;

            masterHandlers[@"query"] = Query;
            masterHandlers[@"register"] = Register;
        }

        private void Write(IHttpResponse response, string text)
        {
            var writer = new StreamWriter(response.Body);
            writer.Write(text);
            writer.Flush();
            response.ContentLength = response.Body.Length;
            response.Send();
        }

        private bool Root(IHttpRequest request, IHttpResponse response)
        {
            Write(response, "FUCK");
            return true;
        }

        private bool StartGame(IHttpRequest request, IHttpResponse response)
        {
            string json;
            var callback = request.Param["callback"].Value;
            var gameName = request.Param["name"].Value;

            GameSetting setting;
            GameInfo info;
            if (!OBBContext.Current.GameSettings.TryGetValue(gameName, out setting))
            {
                json = Utility.GetJsonResult(JsonStatus.FAIL, "Invalid name");
                Write(response, String.Format("{0}({1});", callback, json));
                return true;
            }
            if (!OBBContext.Current.GameInfo.TryGetValue(gameName, out info))
            {
                json = Utility.GetJsonResult(JsonStatus.FAIL, "Invalid name");
                Write(response, String.Format("{0}({1});", callback, json));
                return true;
            }

            var fileName = setting.gameExe;
            var directory = setting.gameWorkPath;
            var argument = setting.gameArguments;

            if (String.IsNullOrEmpty(fileName))
            {
                json = Utility.GetJsonResult(JsonStatus.FAIL, "Empty exe");
                Write(response, String.Format("{0}({1});", callback, json));
                return true;
            }

            try
            {
                var procName = Utility.GetProcName(fileName);

                if (Proc.GetProcessesByName(procName).Any())
                    throw new Exception("Already running");

                var pInfo = new ProcessStartInfo(fileName, argument);
                pInfo.UseShellExecute = true;
                pInfo.CreateNoWindow = true;
                pInfo.WorkingDirectory = directory;
                pInfo.WindowStyle = ProcessWindowStyle.Maximized;

                var process = Proc.Start(pInfo);

                info.isRunning = true;

                info.count++;
                info.total++;

                NativeWin32.SetForegroundWindow(process.MainWindowHandle.ToInt32());
                if (!String.IsNullOrWhiteSpace(setting.vkScript))
                {
                    InputScript script;
                    if (OBBContext.Current.Scripts.TryGetValue(setting.vkScript, out script))
                    {
                        script.Go(process);
                    }
                }
                if (!String.IsNullOrWhiteSpace(setting.DIK))
                {

                    DIK key;
                    Enum.TryParse<DIK>(setting.DIK, out key);
                    Debug.WriteLine(key);
                    new Task(() =>
                    {
                        Debug.WriteLine("SendKey Begin");
                        for (int i = 0; i <= setting.DIKRepeat; ++i)
                        {
                            if (setting.DIKDelay > 0)
                                Thread.Sleep(setting.DIKDelay);

                            Debug.WriteLine("SendKey");
                            MySendKeys.SendKeyPress(key);
                        }
                        Debug.WriteLine("SendKey End");
                    }).Start();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                json = Utility.GetJsonResult(JsonStatus.FAIL, ex.ToString());
                Write(response, String.Format("{0}({1});", callback, json));
                return true;
            }

            json = Utility.GetJsonResult(JsonStatus.OK);
            Write(response, String.Format("{0}({1});", callback, json));
            return true;
        }

        private bool StopGame(IHttpRequest request, IHttpResponse response)
        {
            string json;
            var callback = request.Param["callback"].Value;
            var gameName = request.Param["name"].Value;
            

            GameInfo info;
            if (!OBBContext.Current.GameInfo.TryGetValue(gameName, out info))
            {
                json = Utility.GetJsonResult(JsonStatus.FAIL, "Invalid name");
                Write(response, String.Format("{0}({1});", callback, json));
                return true;
            }
            var setting = info.setting;

            var procName = Utility.GetProcName(setting.gameExe);
            if (String.IsNullOrEmpty(procName))
            {
                json = Utility.GetJsonResult(JsonStatus.FAIL, "Empty Exe");
                Write(response, String.Format("{0}({1});", callback, json));
                return true;
            }

            foreach (var process in Proc.GetProcessesByName(procName))
            {
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

            info.isRunning = false;

            json = Utility.GetJsonResult(JsonStatus.OK);
            Write(response, String.Format("{0}({1});", callback, json));
            return true;
        }

        private bool Reload(IHttpRequest request, IHttpResponse response)
        {
            string json;
            var callback = request.Param["callback"].Value;

            try
            {
                OBBContext.Current.LoadGameConfig();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                json = Utility.GetJsonResult(JsonStatus.FAIL, "Empty Exe");
                Write(response, String.Format("{0}({1});", callback, json));
                return true;
            }

            json = Utility.GetJsonResult(JsonStatus.OK);
            Write(response, String.Format("{0}({1});", callback, json));
            return true;
        }

        private bool Query(IHttpRequest request, IHttpResponse response)
        {
            if (OBBContext.Current.IsMaster)
            {
                var json = JsonConvert.SerializeObject(OBBContext.Current.SlaveInfo);
                Write(response, json);
            }
            else
            {
                var callback = request.Param["callback"].Value;

                var runningGame = OBBContext.Current.GameInfo.Where(pair => pair.Value.isRunning == true);
                foreach (var game in runningGame)
                {
                    var gameName = game.Key;
                    var procName = Utility.GetProcName(OBBContext.Current.GameSettings[gameName].gameExe);

                    if (!Proc.GetProcessesByName(procName).Any())
                    {
                        game.Value.isRunning = false;
                    }
                }

                var json = JsonConvert.SerializeObject(OBBContext.Current.GameInfo);
                if (String.IsNullOrWhiteSpace(callback))
                {
                    Write(response, json);
                }
                else
                {
                    Write(response, String.Format("{0}({1});", callback, json));
                }
            }

            return true;
        }

        private bool Register(IHttpRequest request, IHttpResponse response)
        {
            try
            {
                var postBody = Encoding.UTF8.GetString(request.GetBody());
                var r = JsonConvert.DeserializeObject<RegisterInfo>(postBody);

                if (r == null)
                    throw new Exception("Null data");

                var slave = OBBContext.Current.SlaveInfo.Where(s => s.ip == request.RemoteEndPoint.Address.ToString()).FirstOrDefault();
                if (slave == null)
                {
                    slave = new SlaveOBBInfo();
                    slave.ip = request.RemoteEndPoint.Address.ToString();
                    slave.servicePort = r.servicePort;

                    OBBContext.Current.SlaveInfo.Add(slave);
                }
                slave.GameInfo = r.game;

                Write(response, Utility.GetJsonResult(JsonStatus.OK));
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Write(response, Utility.GetJsonResult(JsonStatus.FAIL, ex.Message));
                return false;
            }
        }

        private bool CanHandle(Uri uri)
        {
            //var rawPath = uri.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped);
            var rawPath = uri.AbsolutePath;
            return rawPath.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Process(IHttpRequest request, IHttpResponse response, IHttpSession session)
        {
            try
            {
                if (!CanHandle(request.Uri))
                {
                    return false;
                }

                response.Encoding = Encoding.UTF8;

                var command = Path.GetFileName(request.Uri.AbsolutePath);

                Func handler;
                if (OBBContext.Current.IsMaster)
                {
                    if (!masterHandlers.TryGetValue(command, out handler))
                    {
                        response.Status = HttpStatusCode.NotFound;
                        return false;
                    }
                }
                else
                {
                    if (!slaveHandlers.TryGetValue(command, out handler))
                    {
                        response.Status = HttpStatusCode.NotFound;
                        return false;
                    }
                }

                response.AddHeader("Cache-Control", "no-cache");
                response.AddHeader("Pragma", "no-cache");
                response.AddHeader("Expires", "0");

                if (!handler(request, response))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }
    }
}
