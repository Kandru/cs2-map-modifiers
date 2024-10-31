using System.Numerics;
using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace MapModifiersPlugin;

public partial class MapModifiersPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    private static void CreateSpawnPoints(MapConfig mapConfig)
    {
        // TODO: move spawn point logic to here
    }

    private static void CreateSpawnPoint(string type, SpawnPoint spawnPoint) {
        CBaseEntity spawn;
        if (type == "t") {
            spawn = Utilities.CreateEntityByName<CInfoPlayerTerrorist>("info_player_terrorist");
        }else{
            spawn = Utilities.CreateEntityByName<CInfoPlayerTerrorist>("info_player_counterterrorist");
        }
        if (spawn == null || spawn.AbsOrigin == null || spawn.AbsRotation == null || !spawn.IsValid)
        {
            Console.WriteLine("[MapModifiersPlugin] ERROR: could not spawn entity");
            return;
        }
        // set attributes
        spawn.AbsOrigin.X = spawnPoint.Origin[0];
        spawn.AbsOrigin.Y = spawnPoint.Origin[1];
        spawn.AbsOrigin.Z = spawnPoint.Origin[2];
        spawn.AbsRotation.X = spawnPoint.Angle[0];
        spawn.AbsRotation.Y = spawnPoint.Angle[1];
        spawn.AbsRotation.Z = spawnPoint.Angle[2];
        // spawn it
        spawn.DispatchSpawn();
    }
}
