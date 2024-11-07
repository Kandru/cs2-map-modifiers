using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace MapModifiers
{
    public partial class MapModifiers : BasePlugin, IPluginConfig<PluginConfig>
    {
        private void ServerCommandsOnMapStart(string mapName)
        {
            foreach (MapConfig mapConfig in _currentMapConfigs)
            {
                // delay execution to allow server to load configurations first
                AddTimer(2.0f, () =>
                {
                    // server commands
                    foreach (var command in mapConfig.ServerCommands)
                    {
                        Server.ExecuteCommand(command);
                        Console.WriteLine(Localizer["servercommands.execute"].Value
                            .Replace("{command}", command)
                            .Replace("{mapName}", mapName));
                    }
                });
            }
        }
    }
}
