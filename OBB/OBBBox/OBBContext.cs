using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameSettings = System.Collections.Generic.Dictionary<string, OBB.GameSetting>;
using GameInfoTable = System.Collections.Generic.Dictionary<string, OBB.GameInfo>;
using GameDataTable = System.Collections.Generic.Dictionary<string, OBB.Summarize>;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using System.Threading;
using OBB.Properties;

namespace OBB
{
    public sealed class OBBContext
    {
        private const string configFile = "config.json";
        private const string dataFile = "data.json";
        private static OBBContext _context;

        static OBBContext()
        {
            _context = new OBBContext();
        }
        
        private OBBContext()
        {
            SlaveInfo = new List<SlaveOBBInfo>();
            GameSettings = new Dictionary<string, OBB.GameSetting>();
            GameInfo = new Dictionary<string, OBB.GameInfo>();
            GameData = new Dictionary<string, Summarize>();
            Scripts = new Dictionary<string, InputScript>();
        }

        public Dictionary<string, InputScript> Scripts { get; private set; }

        public OBBMode Mode { get; set; }

        public OBBInfo Info { get; set; }

        public OBBInfo MasterInfo { get; set; }
        public List<SlaveOBBInfo> SlaveInfo { get; set; }

        public GameInfoTable GameInfo;
        public GameSettings GameSettings;
        public GameDataTable GameData;

        public bool IsMaster { get { return Mode == OBBMode.Master; } }

        public void LoadGameConfig()
        {
            if (IsMaster)
                return;

            try
            {
                var json = File.ReadAllText(configFile, Encoding.UTF8);
                if (String.IsNullOrWhiteSpace(json))
                    return;

                GameSettings = JsonConvert.DeserializeObject<GameSettings>(json);

                GameInfo = GameSettings.ToDictionary(pair => pair.Key,
                    pair => new GameInfo()
                    {
                        name = pair.Value.name,
                        description = pair.Value.description,
                        thumbnail = pair.Value.thumbnail,
                        isRunning = false,
                        setting = pair.Value
                    });

                var assembly = AssemblyHelper.GetAssembly(new[]{ "OBBBox.exe" }, Settings.Default.scriptsPath);
                var objects = AssemblyHelper.GetObjects<InputScript>(assembly);

                Scripts = objects.ToDictionary(s => s.GetType().Name, s => s);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            LoadGameData();
        }

        public void LoadGameData()
        {
            if (IsMaster)
                return;

            try
            {
                var json = File.ReadAllText(dataFile, Encoding.UTF8);
                if (String.IsNullOrWhiteSpace(json))
                    return;

                GameData = JsonConvert.DeserializeObject<GameDataTable>(json);

                foreach (var data in GameData)
                {
                    if (!GameInfo.ContainsKey(data.Key))
                        continue;

                    GameInfo[data.Key].total = data.Value.total;
                    GameInfo[data.Key].count = data.Value.count;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void SaveGameData()
        {
            if (IsMaster)
                return;

            try
            {
                foreach (var game in GameInfo)
                {
                    if (!GameData.ContainsKey(game.Key))
                    {
                        GameData[game.Key] = new Summarize();
                    }

                    GameData[game.Key].total = game.Value.total;
                    GameData[game.Key].count = game.Value.count;
                }

                var json = JsonConvert.SerializeObject(GameData);
                File.WriteAllText(dataFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public static OBBContext Current
        {
            get { return _context; }
        }
    }
}
