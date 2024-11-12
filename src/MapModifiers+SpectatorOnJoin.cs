using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace MapModifiers
{
    public partial class MapModifiers : BasePlugin, IPluginConfig<PluginConfig>
    {
        private void RegisterSpectatorOnJoinListeners()
        {
            RegisterEventHandler<EventPlayerActivate>(SpectatorOnJoinOnPlayerActivate);
        }

        private HookResult SpectatorOnJoinOnPlayerActivate(EventPlayerActivate @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;
            if (player == null || player.IsBot) return HookResult.Continue;
            foreach (MapConfig mapConfig in _currentMapConfigs)
            {
                if (mapConfig.MovetoSpectatorOnJoin)
                {
                    AddTimer(1.0f, () =>
                    {
                        player.ChangeTeam(CsTeam.Spectator);
                    });
                }
            }
            return HookResult.Continue;
        }
    }
}
