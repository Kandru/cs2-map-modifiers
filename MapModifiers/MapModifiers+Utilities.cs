using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace MapModifiers
{
    public partial class MapModifiers : BasePlugin, IPluginConfig<PluginConfig>
    {
        public void SendGlobalChatMessage(string message, float delay = 0)
        {
            foreach (CCSPlayerController player in Utilities.GetPlayers())
            {
                if (player.IsBot) continue;
                AddTimer(delay, () => player.PrintToChat(message));
            }
        }

        public void SendGlobalCenterMessage(string message, float delay = 0, bool alert = false)
        {
            foreach (CCSPlayerController player in Utilities.GetPlayers())
            {
                if (player.IsBot) continue;
                if (alert) AddTimer(delay, () => player.PrintToCenterAlert(message));
                else AddTimer(delay, () => player.PrintToCenterHtml(message));
            }
        }
    }
}