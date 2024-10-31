using System.IO.Enumeration;
using System.Text.Json;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace MapModifiersPlugin;

public class SpawnPoint
{
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
    public bool OverrideSpawns { get; set; } = false;
    // ReSharper disable once InconsistentNaming
    [JsonPropertyName("t_spawns")] public List<SpawnPoint> TSpawns { get; set; } = new();
    // ReSharper disable once InconsistentNaming
    [JsonPropertyName("ct_spawns")] public List<SpawnPoint> CTSpawns { get; set; } = new();
}

public class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("maps")] public Dictionary<string, MapConfig> MapConfigs { get; set; } = new Dictionary<string, MapConfig>();
}

public partial class MapModifiersPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "Map Modifiers Plugin";
    public override string ModuleAuthor => "Jon-Mailes Graeffe <mail@jonni.it> / Kalle <kalle@kandru.de>";
    public override string ModuleVersion => "0.0.11";

    public PluginConfig Config { get; set; } = null!;
    private MapConfig[] _currentMapConfigs = Array.Empty<MapConfig>();
    private bool _overrideSpawns;
    private bool _doneUpdatingSpawns;
    
    public void OnConfigParsed(PluginConfig config)
    {
        Config = config;
        Console.WriteLine(JsonSerializer.Serialize(config));
        Console.WriteLine("[MapModifiersPlugin] Reloaded configuration! For changes to take effect, restart the map.");
    }

    public override void Load(bool hotReload)
    {
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
    }

    private void OnMapStart(string mapName)
    {
        // reset some values, revert some registrations
        _overrideSpawns = false;
        _doneUpdatingSpawns = false;
        RemoveListener("OnClientPutInServer", OnClientPutInServer);
        DeregisterEventHandler("game_start", EventGameStart, true); // should theoretically be gone already
        
        // select map configs whose regexes (keys) match against the map name
        _currentMapConfigs = (from mapConfig in Config.MapConfigs
            where FileSystemName.MatchesSimpleExpression(mapConfig.Key, mapName)
            select mapConfig.Value).ToArray();

        if (_currentMapConfigs.Any())
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
                return;
            }
        }
        
        Console.WriteLine("[MapModifiersPlugin] Found " + _currentMapConfigs.Count() + " matching map-specific configurations for " + mapName + "!");
        
        foreach (var mapConfig in _currentMapConfigs)
        {
            // spawns
            if (mapConfig.TSpawns.Count > 0 || mapConfig.CTSpawns.Count > 0)
            {
                if (mapConfig.OverrideSpawns)
                {
                    // sanity checks
                    if (mapConfig.TSpawns.Count == 0)
                    {
                        Console.WriteLine("[MapModifiersPlugin] WARNING: Map " + mapName + " has no configured spawns for CT, but according to configuration the original spawns shall be removed.");
                        Console.WriteLine("[MapModifiersPlugin] WARNING: This would result in the game having no spawns at all, which will let the server crash. That's why the original spawns are kept and the request to remove original spawns is just ignored!");
                    }
                    else if (mapConfig.CTSpawns.Count == 0)
                    {
                        Console.WriteLine("[MapModifiersPlugin] WARNING: Map " + mapName + " has no configured spawns for CT, but according to configuration the original spawns shall be removed.");
                        Console.WriteLine("[MapModifiersPlugin] WARNING: This would result in the game having no spawns at all, which will let the server crash. That's why the original spawns are kept and the request to remove original spawns is just ignored!");
                    }
                    else
                    {
                        _overrideSpawns = true;
                    }
                }
                
                RegisterEventHandler<EventGameStart>(EventGameStart);
            }
            
            // server commands
            foreach (var command in mapConfig.ServerCommands)
            {
                Server.ExecuteCommand(command);
                Console.WriteLine("[MapModifiersPlugin] Executed server commands for this map!");
            }

            // client commands
            if (mapConfig.ClientCommands.Count > 0)
            {
                RegisterListener<Listeners.OnClientPutInServer>(OnClientPutInServer);
            }
        }
            
    }

    private HookResult EventGameStart(EventGameStart @event, GameEventInfo info)
    {
        if (_doneUpdatingSpawns) return HookResult.Continue;
        
        // delete original spawns set by map author
        if (_overrideSpawns)
        {
            foreach (var spawn in Utilities.FindAllEntitiesByDesignerName<CInfoPlayerTerrorist>("info_player_terrorist"))
            {
                //tSpawn.Remove();
                spawn.AbsOrigin.X = 50;
                spawn.AbsOrigin.Y = -350f;
                spawn.AbsOrigin.Z = 80f;
            }
            foreach (var spawn in Utilities.FindAllEntitiesByDesignerName<CInfoPlayerCounterterrorist>("info_player_counterterrorist"))
            {
                //ctSpawn.Remove();
                spawn.AbsOrigin.X = 50;
                spawn.AbsOrigin.Y = -350f;
                spawn.AbsOrigin.Z = 80f;
            }
            
            Console.WriteLine("[MapModifiersPlugin] Removed original map spawns!");
        }

        // add spawns corresponding to the configuration
        foreach (var mapConfig in _currentMapConfigs)
        {
            foreach (var spawnConfig in mapConfig.TSpawns)
            {
                CreateSpawnPoint("t", new SpawnPoint
                {
                    Origin = [spawnConfig.Origin[0], spawnConfig.Origin[1], spawnConfig.Origin[2]],
                    Angle = [spawnConfig.Angle[0], spawnConfig.Angle[1], spawnConfig.Angle[2]]
                });
            }

            foreach (var spawnConfig in mapConfig.CTSpawns)
            {
                CreateSpawnPoint("ct", new SpawnPoint
                {
                    Origin = [spawnConfig.Origin[0], spawnConfig.Origin[1], spawnConfig.Origin[2]],
                    Angle = [spawnConfig.Angle[0], spawnConfig.Angle[1], spawnConfig.Angle[2]]
                });
            }
        }

        // remove event handler for now, to reduce overhead
        DeregisterEventHandler("game_start", EventGameStart, true);

        Console.WriteLine("[MapModifiersPlugin] Added custom spawns for this map!");
        _doneUpdatingSpawns = true;
        return HookResult.Continue;
    }

    private void OnClientPutInServer(int playerSlot)
    {
        //TODO too early?
        var playerController = Utilities.GetPlayerFromSlot(playerSlot);
        if (playerController == null) return;
        foreach (var mapConfig in _currentMapConfigs)
        {
            foreach (string command in mapConfig.ClientCommands)
            {
                playerController.ExecuteClientCommand(command);
            }
        }
    }
}
