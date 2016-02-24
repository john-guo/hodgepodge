using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using GameInfoTable = System.Collections.Generic.Dictionary<string, OBB.GameInfo>;
using Newtonsoft.Json;

namespace OBB
{
    public enum OBBMode
    {
        Master,
        Slave
    }

    public enum JsonStatus
    {
        OK,
        FAIL
    }

    public class JsonResult
    {
        public JsonStatus status;
        public string reason;
    }

    public class OBBInfo
    {
        public string ip;
        public int notifyPort;
        public int servicePort;
        public string serviceRoot;
    }

    public class SlaveOBBInfo : OBBInfo
    {
        public SlaveOBBInfo()
        {
            GameInfo = new GameInfoTable();
        }

        public GameInfoTable GameInfo;
    }

    public class GameInfo
    {
        public string name;
        public string description;
        public string thumbnail;
        public bool isRunning;
        public int count;
        public int total;

        [JsonIgnore]
        public GameSetting setting;
    }

    public class RegisterInfo
    {
        public int servicePort;
        public GameInfoTable game;
    }

    public class GameSetting
    {
        public string name;
        public string gameExe;
        public string gameWorkPath;
        public string gameArguments;
        public string description;
        public string thumbnail;
        public string vkScript;
        public string DIK;
        public int DIKDelay;
        public int DIKRepeat;
        public bool needPlay;
        public string playExe;
        public string playWorkPath;
        public string playArguments;
    }

    public class Summarize
    {
        public int count;
        public int total;
    }
}
