using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

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

        private float CalculateDistance(Vector point1, Vector point2)
        {
            float dx = point1.X - point2.X;
            float dy = point1.Y - point2.Y;
            float dz = point1.Z - point2.Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}