using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
//using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace MapModifiersPlugin;

public partial class MapModifiersPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    [ConsoleCommand("cs2_addspawn", "Allows to add new spawn points")]
    //TODO: [RequiresPermissions("@mapmodifiers/addSpawnPoints")]
    [CommandHelper(whoCanExecute:CommandUsage.CLIENT_ONLY, minArgs:1, usage:"[ct/t]")]
    public void CommandAddSpawn(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.PlayerPawn.IsValid) return;
        var spawnType = command.GetArg(1);
        if (!spawnType.Equals("ct") && !spawnType.Equals("t"))
        {
            command.ReplyToCommand("[MapModifiersPlugin] Invalid spawn type. Use 'ct' or 't'");
            return;
        }
        var origin = player.PlayerPawn.Value.AbsOrigin;
        if (origin == null)
        {
            command.ReplyToCommand("[MapModifiersPlugin] You do not have a valid position");
            return;
        }
        var angle = player.PlayerPawn.Value.AbsRotation;
        if (angle == null)
        {
            command.ReplyToCommand("[MapModifiersPlugin] You do not have a valid angle");
            return;
        }
        // create spawnpoint
        CreateSpawnPoint(spawnType, new SpawnPoint
        {
            Origin = [origin.X, origin.Y, origin.Z],
            Angle = [angle.X, angle.Y, angle.Z]
        });
        command.ReplyToCommand($"[MapModifiersPlugin] Created Spawn point for {spawnType} at {origin.X}, {origin.Y}, {origin.Z} with angle {angle.X}, {angle.Y}, {angle.Z}");
        // TODO: save to config
        //var jsonString = JsonSerializer.Serialize(_config);
        //File.WriteAllText(_configPath, jsonString);
    }
}
