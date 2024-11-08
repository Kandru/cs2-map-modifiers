using System.IO.Enumeration;
using System.Text.Json;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Config;

namespace MapModifiers
{
    public class MapConfigSpawnPoint
    {
        [JsonPropertyName("name")] public string Name { get; set; } = "unnamed";
        [JsonPropertyName("origin")] public float[] Origin { get; set; } = new float[3];
        [JsonPropertyName("angle")] public float[] Angle { get; set; } = new float[3];
    }

    public class MapConfig
    {
        // commands
        [JsonPropertyName("server_cmds")] public List<string> ServerCommands { get; set; } = new();
        [JsonPropertyName("client_cmds")] public List<string> ClientCommands { get; set; } = new();

        // spawns
        [JsonPropertyName("remove_original_spawns")] public bool DeleteOriginalSpawns { get; set; } = false;
        [JsonPropertyName("statck_original_t_spawns")] public bool StackOriginalTSpawns { get; set; } = false;
        [JsonPropertyName("statck_original_ct_spawns")] public bool StackOriginalCTSpawns { get; set; } = false;
        [JsonPropertyName("t_spawns")] public List<MapConfigSpawnPoint> TSpawns { get; set; } = new();
        [JsonPropertyName("ct_spawns")] public List<MapConfigSpawnPoint> CTSpawns { get; set; } = new();
    }

    public class PluginConfig : BasePluginConfig
    {
        [JsonPropertyName("maps")] public Dictionary<string, MapConfig> MapConfigs { get; set; } = new Dictionary<string, MapConfig>();
    }

    public partial class MapModifiers : BasePlugin, IPluginConfig<PluginConfig>
    {
        public PluginConfig Config { get; set; } = null!;
        private MapConfig[] _currentMapConfigs = Array.Empty<MapConfig>();
        private string _configPath = "";

        private void LoadConfig()
        {
            Config = ConfigManager.Load<PluginConfig>("MapModifiers");
            _configPath = Path.Combine(ModuleDirectory, $"../../configs/plugins/MapModifiers/MapModifiers.json");
        }

        private void InitializeConfig(string mapName)
        {
            // select map configs whose regexes (keys) match against the map name
            _currentMapConfigs = (from mapConfig in Config.MapConfigs
                                  where FileSystemName.MatchesSimpleExpression(mapConfig.Key, mapName)
                                  select mapConfig.Value).ToArray();

            if (_currentMapConfigs.Length > 0)
            {
                if (Config.MapConfigs.TryGetValue("default", out var config))
                {
                    // add default configuration
                    _currentMapConfigs = new[] { config };
                    Console.WriteLine(Localizer["core.defaultconfig"].Value.Replace("{mapName}", mapName));
                }
                else
                {
                    // there is no config to apply
                    Console.WriteLine(Localizer["core.noconfig"].Value.Replace("{mapName}", mapName));
                }
            }
            else
            {
                Console.WriteLine(Localizer["core.defaultconfig"].Value.Replace("{mapName}", mapName));
                // create default configuration
                Config.MapConfigs.Add(mapName, new MapConfig());
            }
            Console.WriteLine(Localizer["core.foundconfig"].Value.Replace("{count}", _currentMapConfigs.Length.ToString()).Replace("{mapName}", mapName));
        }

        public void OnConfigParsed(PluginConfig config)
        {
            Config = config;
            Console.WriteLine(Localizer["core.configinitialized"]);
        }

        private void SaveConfig()
        {
            var jsonString = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, jsonString);
        }
    }
}
