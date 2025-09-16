using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace MapModifiers
{
    public partial class MapModifiers : BasePlugin, IPluginConfig<PluginConfig>
    {
        public override string ModuleName => "Map Modifiers Plugin";
        public override string ModuleAuthor => "Jon-Mailes Graeffe <mail@jonni.it> / Kalle <kalle@kandru.de>";

        private string _currentMap = "";
        private bool _firstRoundOfMap = true;

        public override void Load(bool hotReload)
        {
            // initialize configuration
            LoadConfig();
            // register listeners
            RegisterClientCommandsListeners();
            RegisterSpectatorOnJoinListeners();
            RegisterListener<Listeners.OnMapStart>(OnMapStart);
            RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
            RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            RegisterEventHandler<EventRoundStart>(OnRoundStart);
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
            // remove listeners
            RemoveClientCommandsListeners();
            RemoveSpectatorOnJoinListeners();
            RemoveListener<Listeners.OnMapStart>(OnMapStart);
            RemoveListener<Listeners.OnMapEnd>(OnMapEnd);
            RemoveListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            DeregisterEventHandler<EventRoundStart>(OnRoundStart);
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

        private void OnMapEnd()
        {
            _firstRoundOfMap = true;
        }

        private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            // inform plugins
            EntitiesOnRoundStart(@event, info);
            ServerCommandsOnRoundStart();
            if (_firstRoundOfMap)
            {
                SpawnPointsOnRoundStart(@event, info);
            }
            // reset first round flag
            _firstRoundOfMap = false;
            // continue event
            return HookResult.Continue;
        }
    }
}
