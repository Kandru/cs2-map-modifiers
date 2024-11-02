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
            RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            // print message if hot reload
            if (hotReload)
            {
                // set current map
                _currentMap = Server.MapName;
                // initialize configuration
                InitializeConfig(_currentMap);
                Console.WriteLine("[MapModifiers] Hot reload detected, restart map for all changes to take effect!");
            }
        }

        public override void Unload(bool hotReload)
        {
            Console.WriteLine("[MapModifiersPlugin] Unloaded Plugin!");
        }

        private void OnMapStart(string mapName)
        {
            // set map name
            _currentMap = mapName.ToLower();
            // initialize configuration
            InitializeConfig(mapName.ToLower());
            // iterate through all configurations
            foreach (MapConfig mapConfig in _currentMapConfigs)
            {
                // spawn points
                OnMapStartSpawnPoints(mapName.ToLower(), mapConfig);
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

        private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            CreateSpawnPoints();
            // continue with original event
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
}
