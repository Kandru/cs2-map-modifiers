using System.IO.Enumeration;
using System.Text.Json;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

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
        [JsonPropertyName("remove_original_spawns")]
        public bool DeleteOriginalSpawns { get; set; } = false;
        // ReSharper disable once InconsistentNaming
        [JsonPropertyName("t_spawns")] public List<MapConfigSpawnPoint> TSpawns { get; set; } = new();
        // ReSharper disable once InconsistentNaming
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
                    Console.WriteLine("[MapModifiersPlugin] Found no map-specific configuration for " + mapName + ", using default one!");
                }
                else
                {
                    // there is no config to apply
                    Console.WriteLine("[MapModifiersPlugin] No map-specific configuration for " + mapName + " or default one found. Skipping!");
                }
            }
            else
            {
                Console.WriteLine("[MapModifiersPlugin] No map-specific configuration for " + mapName + " found. Creating default one!");
                // create default configuration
                Config.MapConfigs.Add(mapName, new MapConfig());
            }
            Console.WriteLine("[MapModifiersPlugin] Found " + _currentMapConfigs.Count() + " matching map-specific configurations for " + mapName + "!");
        }

        public void OnConfigParsed(PluginConfig config)
        {
            Config = config;
            Console.WriteLine("[MapModifiersPlugin] Initialized map configuration!");
        }

        private void SaveConfig()
        {
            var jsonString = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, jsonString);
        }
    }
}
