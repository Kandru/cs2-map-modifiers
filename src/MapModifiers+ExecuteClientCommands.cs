using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace MapModifiers
{
    public partial class MapModifiers : BasePlugin, IPluginConfig<PluginConfig>
    {
        private void RegisterClientCommandsListeners()
        {
            RegisterListener<Listeners.OnMapEnd>(ClientCommandsOnMapEnd);
        }

        private void RemoveClientCommandsListeners()
        {
            RemoveListener<Listeners.OnMapEnd>(ClientCommandsOnMapEnd);
            RemoveListener<Listeners.OnClientPutInServer>(ClientCommandsOnClientPutInServer);
        }

        private void ClientCommandsOnMapStart(string mapName)
        {
            foreach (MapConfig mapConfig in _currentMapConfigs)
            {
                if (mapConfig.ClientCommands.Count > 0)
                {
                    RegisterListener<Listeners.OnClientPutInServer>(ClientCommandsOnClientPutInServer);
                }
            }
        }

        private void ClientCommandsOnMapEnd()
        {
            RemoveListener<Listeners.OnClientPutInServer>(ClientCommandsOnClientPutInServer);
        }

        private void ClientCommandsOnClientPutInServer(int playerSlot)
        {
            // execute client commands
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);
            if (player == null || player.IsBot) return;
            foreach (MapConfig mapConfig in _currentMapConfigs)
            {
                foreach (string command in mapConfig.ClientCommands)
                {
                    player.ExecuteClientCommand(command);
                    Console.WriteLine(Localizer["clientcommands.executed"].Value
                    .Replace("{command}", command)
                    .Replace("{player}", player.PlayerName));
                }
            }
        }
    }
}
