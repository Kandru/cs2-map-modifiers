using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace MapModifiers
{
    public partial class MapModifiers : BasePlugin, IPluginConfig<PluginConfig>
    {
        [ConsoleCommand("addspawn", "Allows to add new spawn points")]
        [RequiresPermissions("@mapmodifiers/spawnpoints")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY, minArgs: 1, usage: "[ct/t] [name]")]
        public void CommandAddSpawn(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null || player.LifeState != (byte)LifeState_t.LIFE_ALIVE) return;
            var spawnType = command.GetArg(1);
            var spawnName = command.GetArg(2);
            spawnName ??= "unnamed";
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
            if (origin.X == 0 && origin.Y == 0 && origin.Z == 0)
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
            MapConfigSpawnPoint newSpawnPoint = new()
            {
                Name = spawnName,
                Origin = [origin.X, origin.Y, origin.Z + 10], // add 10 units to avoid clipping like original spawn points
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
            // update markers
            ShowSpawnPointMarkers();
            command.ReplyToCommand($"[MapModifiersPlugin] Created Spawn point for {spawnType} at {origin.X}, {origin.Y}, {origin.Z} with angle {angle.X}, {angle.Y}, {angle.Z}");
        }

        [ConsoleCommand("delspawn", "Deletes the nearest custom spawn point (max 200 units)")]
        [RequiresPermissions("@mapmodifiers/spawnpoints")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY, minArgs: 0)]
        public void CommandDelSpawn(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null || player.LifeState != (byte)LifeState_t.LIFE_ALIVE) return;
            var origin = player.PlayerPawn.Value.AbsOrigin;
            if (origin == null)
            {
                command.ReplyToCommand("[MapModifiersPlugin] You do not have a valid position");
                return;
            }
            if (origin.X == 0 && origin.Y == 0 && origin.Z == 0)
            {
                command.ReplyToCommand("[MapModifiersPlugin] You do not have a valid position");
                return;
            }
            SpawnPoint? spawnEntity = GetNearestSpawnPoint(origin, 200);
            if (spawnEntity == null || spawnEntity.AbsOrigin == null || !spawnEntity.IsValid)
            {
                command.ReplyToCommand("[MapModifiersPlugin] No spawn point found within 200 units");
                return;
            }
            // update configuration
            var mapConfig = Config.MapConfigs[_currentMap];
            foreach (var spawnPoint in mapConfig.TSpawns)
            {
                if (spawnPoint.Origin.SequenceEqual([spawnEntity.AbsOrigin.X, spawnEntity.AbsOrigin.Y, spawnEntity.AbsOrigin.Z]))
                {
                    command.ReplyToCommand($"[MapModifiersPlugin] Removed T Spawn Point at {spawnEntity.AbsOrigin.X}, {spawnEntity.AbsOrigin.Y}, {spawnEntity.AbsOrigin.Z}");
                    mapConfig.TSpawns.Remove(spawnPoint);
                    break;
                }
            }
            foreach (var spawnPoint in mapConfig.CTSpawns)
            {
                if (spawnPoint.Origin.SequenceEqual([spawnEntity.AbsOrigin.X, spawnEntity.AbsOrigin.Y, spawnEntity.AbsOrigin.Z]))
                {
                    command.ReplyToCommand($"[MapModifiersPlugin] Removed CT Spawn Point at {spawnEntity.AbsOrigin.X}, {spawnEntity.AbsOrigin.Y}, {spawnEntity.AbsOrigin.Z}");
                    mapConfig.CTSpawns.Remove(spawnPoint);
                    break;
                }
            }
            SaveConfig();
            // delete maker (if any)
            ChangeSpawnPointMarker(new Vector(
                spawnEntity.AbsOrigin.X,
                spawnEntity.AbsOrigin.Y,
                spawnEntity.AbsOrigin.Z
            ),
            color: [125]);
            // TODO: deleting SpawnEntity does crash server on next round
            // delete entity
            //spawnEntity.Remove();
        }

        [ConsoleCommand("showspawns", "Whether to show all spawn points or not")]
        //[RequiresPermissions("@mapmodifiers/spawnpoints")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY, minArgs: 0)]
        public void CommandShowSpawns(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null || !player.PlayerPawn.IsValid) return;
            var amountSpawnPoints = ToggleSpawnPointMarkers();
            command.ReplyToCommand($"[MapModifiersPlugin] Toggled {amountSpawnPoints} spawn points");
        }
    }
}
