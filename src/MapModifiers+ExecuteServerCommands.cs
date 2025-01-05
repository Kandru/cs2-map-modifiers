using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace MapModifiers
{
    public partial class MapModifiers : BasePlugin, IPluginConfig<PluginConfig>
    {
        private void ServerCommandsOnRoundStart()
        {
            foreach (MapConfig mapConfig in _currentMapConfigs)
            {
                // server commands
                foreach (var command in mapConfig.ServerCommands)
                {
                    Console.WriteLine(Localizer["servercommands.execute"].Value
                        .Replace("{command}", command)
                        .Replace("{mapName}", _currentMap));
                    // delay execution to allow server to load configurations first
                    Server.ExecuteCommand(command);
                }
            }
        }
    }
}
