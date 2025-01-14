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
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY, minArgs: 1, usage: "[ct/t/both] [name]")]
        public void CommandAddSpawn(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null
                || !player.Pawn.IsValid
                || player.Pawn.Value == null) return;
            var spawnType = command.GetArg(1);
            var spawnName = command.GetArg(2);
            spawnName ??= "unnamed";
            if (!spawnType.Equals("ct") && !spawnType.Equals("t") && !spawnType.Equals("both"))
            {
                command.ReplyToCommand("[MapModifiersPlugin] Invalid spawn type. Use 'ct', 't' or 'both'");
                return;
            }
            var origin = player.Pawn.Value.AbsOrigin;
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
            QAngle angle = new QAngle(
                0,
                (float)Math.Round(player.Pawn.Value.V_angle!.Y, 5),
                0
            );
            MapConfigEntity newSpawnPoint = new()
            {
                Name = spawnName,
                ClassName = spawnType == "t" ? "info_player_terrorist" : spawnType == "ct" ? "info_player_counterterrorist" : "info_player_start",
                Team = spawnType == "t" ? 2 : spawnType == "ct" ? 3 : 0,
                Origin = [origin.X, origin.Y, origin.Z + 10], // add 10 units to avoid clipping like original spawn points
                Angle = [angle.X, angle.Y, angle.Z],
            };
            // create spawnpoint
            CBaseEntity? createdEntity = CreateEntity(newSpawnPoint);
            if (createdEntity == null
                || createdEntity.AbsOrigin == null) return;
            // update entity origin because the engine might place it differently (e.g. hostage_entity is always on the ground)
            newSpawnPoint.Origin = [createdEntity.AbsOrigin.X, createdEntity.AbsOrigin.Y, createdEntity.AbsOrigin.Z];
            // save configuration
            Config.MapConfigs[_currentMap].Entities.Add(newSpawnPoint);
            SaveConfig();
            // update markers
            ShowSpawnPointMarkers();
            command.ReplyToCommand($"[MapModifiersPlugin] Created Spawn point for {spawnType} at {origin.X}, {origin.Y}, {origin.Z} with angle {angle.X}, {angle.Y}, {angle.Z}");
        }

        [ConsoleCommand("addentity", "Allows to add new entities")]
        [RequiresPermissions("@mapmodifiers/spawnpoints")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY, minArgs: 2, usage: "[entity] [ct/t/spec/none] [name]")]
        public void CommandAddEntity(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null
                || !player.Pawn.IsValid
                || player.Pawn.Value == null) return;
            var entityType = command.GetArg(1);
            var entityTeam = command.GetArg(2);
            var entityName = command.GetArg(3);
            entityName ??= "unnamed";
            if (!entityTeam.Equals("ct") && !entityTeam.Equals("t") && !entityTeam.Equals("spec") && !entityTeam.Equals("none"))
            {
                command.ReplyToCommand("[MapModifiersPlugin] Invalid spawn type. Use 'ct', 't', 'spec' or 'none'");
                return;
            }
            var origin = player.Pawn.Value.AbsOrigin;
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
            QAngle angle = new QAngle(
                0,
                (float)Math.Round(player.Pawn.Value.V_angle!.Y, 5),
                0
            );
            MapConfigEntity newEntity = new()
            {
                Name = entityName,
                ClassName = entityType,
                Team = entityTeam == "t" ? 2 : entityTeam == "ct" ? 3 : entityTeam == "spec" ? 1 : 0,
                Origin = [origin.X, origin.Y, origin.Z + 10], // add 10 units to avoid clipping
                Angle = [angle.X, angle.Y, angle.Z],
            };
            // create entity
            CBaseEntity? createdEntity = CreateEntity(newEntity);
            if (createdEntity == null
                || createdEntity.AbsOrigin == null) return;
            // update entity origin because the engine might place it differently (e.g. hostage_entity is always on the ground)
            newEntity.Origin = [createdEntity.AbsOrigin.X, createdEntity.AbsOrigin.Y, createdEntity.AbsOrigin.Z];
            // save configuration
            Config.MapConfigs[_currentMap].Entities.Add(newEntity);
            SaveConfig();
            command.ReplyToCommand($"[MapModifiersPlugin] Created Entity for {entityType} at {origin.X}, {origin.Y}, {origin.Z} with angle {angle.X}, {angle.Y}, {angle.Z}");
        }

        [ConsoleCommand("delentity", "Deletes the nearest custom entity (max 200 units)")]
        [RequiresPermissions("@mapmodifiers/spawnpoints")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY, minArgs: 0, usage: "[delete <true/false>]")]
        public void CommandDelEntity(CCSPlayerController? player, CommandInfo command)
        {
            bool deleteEntity = command.GetArg(1) == "true" ? true : false;
            if (player == null
                || !player.Pawn.IsValid
                || player.Pawn.Value == null) return;
            var origin = player.Pawn.Value.AbsOrigin;
            if (origin == null
                || (origin.X == 0 && origin.Y == 0 && origin.Z == 0))
            {
                command.ReplyToCommand("[MapModifiersPlugin] You do not have a valid position");
                return;
            }
            CBaseEntity? nearestEntity = GetNearestEntity(origin, 200);
            if (nearestEntity == null
                || !nearestEntity.IsValid
                || nearestEntity.AbsOrigin == null)
            {
                command.ReplyToCommand("[MapModifiersPlugin] No entity found within 200 units");
                return;
            }
            // update configuration
            var mapConfig = Config.MapConfigs[_currentMap];
            foreach (var entity in mapConfig.Entities)
            {
                if (entity.Origin.SequenceEqual([nearestEntity.AbsOrigin.X, nearestEntity.AbsOrigin.Y, nearestEntity.AbsOrigin.Z]))
                {
                    // acknowledge removal
                    command.ReplyToCommand($"[MapModifiersPlugin] Removed Entity {entity.Name} ({entity.ClassName}) at {nearestEntity.AbsOrigin.X}, {nearestEntity.AbsOrigin.Y}, {nearestEntity.AbsOrigin.Z}");
                    // remove entity from configuration
                    mapConfig.Entities.Remove(entity);
                    // save configuration
                    SaveConfig();
                    // delete spawn marker
                    if (nearestEntity.DesignerName != null && nearestEntity.DesignerName.Contains("info_player_"))
                        ChangeSpawnPointMarker(new Vector(
                            nearestEntity.AbsOrigin.X,
                            nearestEntity.AbsOrigin.Y,
                            nearestEntity.AbsOrigin.Z
                        ),
                        color: [125]);
                    // delete entity
                    if (deleteEntity) nearestEntity.Remove();
                    break;
                }
            }
        }

        [ConsoleCommand("showspawns", "Whether to show all spawn points or not")]
        [RequiresPermissions("@mapmodifiers/spawnpoints")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY, minArgs: 0)]
        public void CommandShowSpawns(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;
            var amountSpawnPoints = ToggleSpawnPointMarkers();
            command.ReplyToCommand($"[MapModifiersPlugin] Toggled {amountSpawnPoints} spawn points");
        }
    }
}
