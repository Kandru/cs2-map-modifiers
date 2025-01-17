using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;

namespace MapModifiers
{
    public partial class MapModifiers : BasePlugin, IPluginConfig<PluginConfig>
    {
        private void DisableTeamIntroOnRoundStart()
        {
            foreach (MapConfig mapConfig in _currentMapConfigs)
            {
                // get convar
                ConVar? mpTeamIntroTime = ConVar.Find("mp_team_intro_time");
                // check if a change is necessary
                if (mpTeamIntroTime == null || mpTeamIntroTime.GetPrimitiveValue<float>() == 0.0f) return;
                // get intro entities
                var tIntroEntities = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("team_intro_terrorist").ToArray();
                var ctIntroEntities = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("team_intro_counterterrorist").ToArray();
                // server commands
                if (mapConfig.DisableTeamIntro && tIntroEntities.Length == 0 && ctIntroEntities.Length == 0)
                {
                    Console.WriteLine(Localizer["disableteamintro"]);
                    Server.ExecuteCommand("mp_team_intro_time 0");
                }
            }
        }
    }
}
