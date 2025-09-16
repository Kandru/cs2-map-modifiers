using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

namespace MapModifiers
{
    public partial class MapModifiers : BasePlugin, IPluginConfig<PluginConfig>
    {
        private void RegisterSpectatorOnJoinListeners()
        {
            RegisterEventHandler<EventPlayerActivate>(SpectatorOnJoinOnPlayerActivate);
        }

        private void RemoveSpectatorOnJoinListeners()
        {
            DeregisterEventHandler<EventPlayerActivate>(SpectatorOnJoinOnPlayerActivate);
        }

        private HookResult SpectatorOnJoinOnPlayerActivate(EventPlayerActivate @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;
            if (player == null || player.IsBot) return HookResult.Continue;
            foreach (MapConfig mapConfig in _currentMapConfigs)
            {
                if (mapConfig.MovetoSpectatorOnJoin)
                {
                    // get convar
                    ConVar? mpForcePickTime = ConVar.Find("mp_force_pick_time");
                    if (mpForcePickTime == null) return HookResult.Continue;
                    AddTimer(mpForcePickTime.GetPrimitiveValue<float>() - 0.2f, () =>
                    {
                        if (player == null || !player.IsValid) return;
                        if (player.Team != CsTeam.None) return;
                        player.ChangeTeam(CsTeam.Spectator);
                    });
                }
            }
            return HookResult.Continue;
        }
    }
}
