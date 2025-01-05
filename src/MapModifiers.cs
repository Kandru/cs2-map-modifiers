using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace MapModifiers
{
    public partial class MapModifiers : BasePlugin, IPluginConfig<PluginConfig>
    {
        public override string ModuleName => "Map Modifiers Plugin";
        public override string ModuleAuthor => "Jon-Mailes Graeffe <mail@jonni.it> / Kalle <kalle@kandru.de>";

        private string _currentMap = "";

        public override void Load(bool hotReload)
        {
            // initialize configuration
            LoadConfig();
            // register listeners
            RegisterClientCommandsListeners();
            RegisterSpectatorOnJoinListeners();
            RegisterListener<Listeners.OnMapStart>(OnMapStart);
            RegisterEventHandler<EventRoundStart>(OnRoundStart);
            RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            // print message if hot reload
            if (hotReload)
            {
                // initialize configuration
                InitializeConfig(_currentMap);
                // set current map
                _currentMap = Server.MapName;
                Console.WriteLine(Localizer["core.hotreload"]);
            }
        }

        public override void Unload(bool hotReload)
        {
            RemoveClientCommandsListeners();
            Console.WriteLine(Localizer["core.unload"]);
        }

        private void OnMapStart(string mapName)
        {
            // set map name
            _currentMap = mapName.ToLower();
            // update configuration
            LoadConfig();
            InitializeConfig(_currentMap);
            SaveConfig();
            // inform plugins
            ClientCommandsOnMapStart(mapName);
        }

        private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            // inform plugins
            SpawnPointsOnRoundStart(@event, info);
            ServerCommandsOnRoundStart();
            // continue event
            return HookResult.Continue;
        }
    }
}
