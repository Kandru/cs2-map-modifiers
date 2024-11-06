using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace MapModifiers
{
    public partial class MapModifiers : BasePlugin, IPluginConfig<PluginConfig>
    {
        public override string ModuleName => "Map Modifiers Plugin";
        public override string ModuleAuthor => "Jon-Mailes Graeffe <mail@jonni.it> / Kalle <kalle@kandru.de>";
        public override string ModuleVersion => "0.0.11";

        private string _currentMap = "";

        public override void Load(bool hotReload)
        {
            // initialize configuration
            LoadConfig();
            // register listeners
            RegisterEventHandler<EventRoundStart>(OnRoundStart);
            RegisterListener<Listeners.OnMapStart>(OnMapStart);
            RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
            RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            // print message if hot reload
            if (hotReload)
            {
                // set current map
                _currentMap = Server.MapName;
                // initialize configuration
                InitializeConfig(_currentMap);
                Console.WriteLine(Localizer["core.hotreload"]);
            }
        }

        public override void Unload(bool hotReload)
        {
            Console.WriteLine(Localizer["core.unload"]);
        }

        private void OnMapStart(string mapName)
        {
            // set map name
            _currentMap = mapName.ToLower();
            // update configuration
            LoadConfig();
            SaveConfig();
            // iterate through all configurations
            foreach (MapConfig mapConfig in _currentMapConfigs)
            {
                // spawn points
                OnMapStartSpawnPoints(mapName.ToLower(), mapConfig);
                // delay execution to allow server to load configurations first
                AddTimer(2.0f, () =>
                {
                    // server commands
                    foreach (var command in mapConfig.ServerCommands)
                    {
                        Server.ExecuteCommand(command);
                        Console.WriteLine($"[MapModifiersPlugin] Executed server command: {command}");
                    }
                });

                // client commands
                if (mapConfig.ClientCommands.Count > 0)
                {
                    RegisterListener<Listeners.OnClientPutInServer>(OnClientPutInServer);
                }
            }
        }

        private void OnMapEnd()
        {
            RemoveListener<Listeners.OnClientPutInServer>(OnClientPutInServer);
        }

        private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            // create spawn points if necessary
            CreateSpawnPoints();
            // check if we have enough spawn points
            CountSpawnPoints();
            // continue with original event
            return HookResult.Continue;
        }

        private void OnClientPutInServer(int playerSlot)
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);
            if (player == null) return;
            foreach (MapConfig mapConfig in _currentMapConfigs)
            {
                foreach (string command in mapConfig.ClientCommands)
                {
                    player.ExecuteClientCommand(command);
                    Console.WriteLine($"[MapModifiersPlugin] Executed client command: {command} for {player.PlayerName}");
                }
            }
        }
    }
}
