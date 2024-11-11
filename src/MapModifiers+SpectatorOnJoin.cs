using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace MapModifiers
{
    public partial class MapModifiers : BasePlugin, IPluginConfig<PluginConfig>
    {
        private void RegisterSpectatorOnJoinListeners()
        {
            RegisterListener<Listeners.OnClientPutInServer>(SpectatorOnJoinOnClientPutInServer);
        }

        private void RemoveSpectatorOnJoinListeners()
        {
            RemoveListener<Listeners.OnClientPutInServer>(SpectatorOnJoinOnClientPutInServer);
        }

        private void SpectatorOnJoinOnClientPutInServer(int playerSlot)
        {
            bool hasSpectatorOnJoinEnabled = false;
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);
            if (player == null || player.IsBot) return;
            foreach (MapConfig mapConfig in _currentMapConfigs)
            {
                if (mapConfig.MovetoSpectatorOnJoin)
                {
                    AddTimer(2f, () =>
                    {
                        CCSPlayerController? tmpPlayer = new(player.Handle);
                        if (tmpPlayer == null) return;
                        tmpPlayer.ChangeTeam(CsTeam.Spectator);
                    });
                    hasSpectatorOnJoinEnabled = true;
                }
            }
            if (!hasSpectatorOnJoinEnabled) RemoveSpectatorOnJoinListeners();
        }
    }
}
