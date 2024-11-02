using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace MapModifiers
{
    public partial class MapModifiers : BasePlugin, IPluginConfig<PluginConfig>
    {
        [ConsoleCommand("addspawn", "Allows to add new spawn points")]
        [RequiresPermissions("@mapmodifiers/addspawn")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY, minArgs: 1, usage: "[ct/t]")]
        public void CommandAddSpawn(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null) return;
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
            if (origin.X == 0 && origin.Y == 0 && origin.Z == 0)
            {
                command.ReplyToCommand("[MapModifiersPlugin] You do not have a valid position");
                return;
            }
            MapConfigSpawnPoint newSpawnPoint = new()
            {
                Origin = [origin.X, origin.Y, origin.Z],
                Angle = [angle.X, angle.Y, angle.Z],
            };
            // create spawnpoint
            CreateSpawnPoint(spawnType, newSpawnPoint);
            // save configuration
            if (spawnType == "t")
            {
                Config.MapConfigs[_currentMap].TSpawns.Add(newSpawnPoint);
            }
            else
            {
                Config.MapConfigs[_currentMap].CTSpawns.Add(newSpawnPoint);
            }
            SaveConfig();
            command.ReplyToCommand($"[MapModifiersPlugin] Created Spawn point for {spawnType} at {origin.X}, {origin.Y}, {origin.Z} with angle {angle.X}, {angle.Y}, {angle.Z}");
        }

        [ConsoleCommand("showspawns", "Whether to show all spawn points or not")]
        [RequiresPermissions("@mapmodifiers/showspawns")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY, minArgs: 1, usage: "[0/1]")]
        public void CommandShowSpawns(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null || !player.PlayerPawn.IsValid) return;
            var show = command.GetArg(1);
            if (!show.Equals("0") && !show.Equals("1"))
            {
                command.ReplyToCommand("[MapModifiersPlugin] Invalid option. Use '0' or '1'");
                return;
            }
            if (show.Equals("0"))
            {
                var amountHidden = RemoveSpawnPointMarkers();
                command.ReplyToCommand($"[MapModifiersPlugin] Hid {amountHidden} spawn points");
                return;
            }
            else
            {
                var amountSpawnPoints = ShowSpawnPointMarkers();
                command.ReplyToCommand($"[MapModifiersPlugin] Showing {amountSpawnPoints} spawn points");
            }
        }
    }
}
