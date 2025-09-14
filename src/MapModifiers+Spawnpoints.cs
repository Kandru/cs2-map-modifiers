using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using MapModifiers.Utils;
using System.Drawing;

namespace MapModifiers
{
    public partial class MapModifiers : BasePlugin, IPluginConfig<PluginConfig>
    {
        private void SpawnPointsOnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            // count spawns
            var playerSpawnEntities = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_start").ToArray();
            var ctSpawnEntities = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_counterterrorist").ToArray();
            var tSpawnEntities = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_terrorist").ToArray();
            // count max players per team
            int amountTerroristsAllowedToSpawn = Server.MaxPlayers / 2;
            int amountCounterTerroristsAllowedToSpawn = Server.MaxPlayers / 2;
            // check the real values via GameRules (if available) and overwrite the defaults
            var NumSpawnableTerrorists = GameRules.Get("MaxNumTerrorists");
            if (NumSpawnableTerrorists is int numSpawnableTerrorists)
            {
                amountTerroristsAllowedToSpawn = numSpawnableTerrorists;
            }
            var NumSpawnableCTs = GameRules.Get("MaxNumCTs");
            if (NumSpawnableCTs is int numSpawnableCTs)
            {
                amountCounterTerroristsAllowedToSpawn = numSpawnableCTs;
            }
            // print info to console
            Console.WriteLine(Localizer["spawnpoints.count"].Value
                .Replace("{spawns}", playerSpawnEntities.Length.ToString())
                .Replace("{ct}", ctSpawnEntities.Length.ToString())
                .Replace("{t}", tSpawnEntities.Length.ToString())
                .Replace("{ct_allowed}", amountCounterTerroristsAllowedToSpawn.ToString())
                .Replace("{t_allowed}", amountTerroristsAllowedToSpawn.ToString())
                .Replace("{maxplayers}", Server.MaxPlayers.ToString()));
            if (playerSpawnEntities.Length < (amountTerroristsAllowedToSpawn + amountCounterTerroristsAllowedToSpawn)
                && (ctSpawnEntities.Length < amountCounterTerroristsAllowedToSpawn
                || tSpawnEntities.Length < amountTerroristsAllowedToSpawn))
            {
                var message = Localizer["spawnpoints.countspawns.warning"].Value
                    .Replace("{spawns}", playerSpawnEntities.Length.ToString())
                    .Replace("{ct}", ctSpawnEntities.Length.ToString())
                    .Replace("{t}", tSpawnEntities.Length.ToString())
                    .Replace("{ct_allowed}", amountCounterTerroristsAllowedToSpawn.ToString())
                    .Replace("{t_allowed}", amountTerroristsAllowedToSpawn.ToString())
                    .Replace("{maxplayers}", Server.MaxPlayers.ToString());
                Console.WriteLine(message);
                SendGlobalChatMessage(message);
            }
        }

        private int ToggleSpawnPointMarkers()
        {
            var spawnMarkerEntities = Utilities.FindAllEntitiesByDesignerName<CDynamicProp>("prop_dynamic").ToArray();
            var count = spawnMarkerEntities.Count(x => x.Globalname != null && x.Globalname.Contains("mapmodifiers_spawnpointmarker"));
            if (count == 0)
            {
                return ShowSpawnPointMarkers();
            }
            else
            {
                return RemoveSpawnPointMarkers();
            }
        }

        private int ShowSpawnPointMarkers()
        {
            var spawnPointEntities = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_").ToArray();
            var spawnMarkerEntities = Utilities.FindAllEntitiesByDesignerName<CDynamicProp>("prop_dynamic").Where(x => x.Globalname != null && x.Globalname.Contains("mapmodifiers_spawnpointmarker")).ToArray();
            var count = 0;
            foreach (var entity in spawnPointEntities)
            {
                count++;
                var spawnPointEntity = entity.As<SpawnPoint>();
                // check if spawn point marker already exists at that origin
                if (spawnMarkerEntities.Any(x => x.AbsOrigin != null && spawnPointEntity.AbsOrigin != null &&
                    Math.Round(x.AbsOrigin.X, 3) == Math.Round(spawnPointEntity.AbsOrigin.X, 3) &&
                    Math.Round(x.AbsOrigin.Y, 3) == Math.Round(spawnPointEntity.AbsOrigin.Y, 3) &&
                    Math.Round(x.AbsOrigin.Z, 3) == Math.Round(spawnPointEntity.AbsOrigin.Z, 3)))
                {
                    continue;
                }
                CDynamicProp spawnMarkerEntity = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic")!;
                if (spawnPointEntity.AbsOrigin == null || spawnMarkerEntity.AbsOrigin == null
                    || spawnPointEntity.AbsRotation == null || spawnMarkerEntity.AbsRotation == null
                    || !spawnMarkerEntity.IsValid || !spawnPointEntity.IsValid)
                {
                    Console.WriteLine(Localizer["spawnpoints.marker.noentity"]);
                    return 0;
                }
                // random string due to a problem when deleting entities and create them again with the same name
                // they will not show up again
                var randomString = new string(Enumerable.Range(0, 5).Select(_ => (char)new Random().Next('a', 'z' + 1)).ToArray());
                spawnMarkerEntity.Globalname = $"mapmodifiers_spawnpointmarker_{randomString}";
                spawnMarkerEntity.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_NONE;
                spawnMarkerEntity.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_NONE;
                spawnMarkerEntity.AbsOrigin.X = spawnPointEntity.AbsOrigin.X;
                spawnMarkerEntity.AbsOrigin.Y = spawnPointEntity.AbsOrigin.Y;
                spawnMarkerEntity.AbsOrigin.Z = spawnPointEntity.AbsOrigin.Z;
                spawnMarkerEntity.AbsRotation.X = spawnPointEntity.AbsRotation.X;
                spawnMarkerEntity.AbsRotation.Y = spawnPointEntity.AbsRotation.Y;
                spawnMarkerEntity.AbsRotation.Z = spawnPointEntity.AbsRotation.Z;
                spawnMarkerEntity.DispatchSpawn();
                spawnMarkerEntity.SetModel("models/props/cs_office/file_cabinet1.vmdl");
                int alpha = 255;
                // check whether spawnpoint does exist in current map configuration
                var mapConfig = Config.MapConfigs[_currentMap];
                var spawnPointConfig = mapConfig.Entities.FirstOrDefault(x => x.Origin.SequenceEqual([spawnPointEntity.AbsOrigin.X, spawnPointEntity.AbsOrigin.Y, spawnPointEntity.AbsOrigin.Z]));
                if (spawnPointConfig == null)
                {
                    alpha = 125;
                }
                // set color depending on spawn side
                if (spawnPointEntity.DesignerName.Contains("counterterrorist"))
                {
                    // CT spawn
                    spawnMarkerEntity.Render = Color.FromArgb(0, 0, 255);
                    // if custom CT spawn
                    if (spawnPointEntity.Globalname != null && spawnPointEntity.Globalname.Contains("mapmodifiers_info_player_counterterrorist"))
                        spawnMarkerEntity.Render = Color.FromArgb(alpha, 4, 138, 255);
                }
                else if (spawnPointEntity.DesignerName.Contains("terrorist"))
                {
                    spawnMarkerEntity.Render = Color.FromArgb(255, 0, 0);
                    // if custom T spawn
                    if (spawnPointEntity.Globalname != null && spawnPointEntity.Globalname.Contains("mapmodifiers_info_player_terrorist"))
                        spawnMarkerEntity.Render = Color.FromArgb(alpha, 255, 90, 0);
                }
                else
                {
                    spawnMarkerEntity.Render = Color.FromArgb(255, 255, 255);
                    // if custom DM spawn
                    if (spawnPointEntity.Globalname != null && spawnPointEntity.Globalname.Contains("mapmodifiers_info_player_start"))
                        spawnMarkerEntity.Render = Color.FromArgb(alpha, 220, 220, 220);
                }
            }
            return count;
        }

        private int RemoveSpawnPointMarkers()
        {
            var spawnMarkerEntities = Utilities.FindAllEntitiesByDesignerName<CDynamicProp>("prop_dynamic").Where(x => x.Globalname != null && x.Globalname.Contains("mapmodifiers_spawnpointmarker")).ToArray();
            var count = 0;
            foreach (var entity in spawnMarkerEntities)
            {
                count++;
                var spawnMarkerEntity = entity.As<CDynamicProp>();
                spawnMarkerEntity.Remove();
            }
            return count;
        }

        private bool ChangeSpawnPointMarker(Vector origin, int[]? color = null)
        {
            var spawnMarkerEntities = Utilities.FindAllEntitiesByDesignerName<CDynamicProp>("prop_dynamic").Where(x => x.Globalname != null && x.Globalname.Contains("mapmodifiers_spawnpointmarker")).ToArray();
            var spawnMarkerEntity = spawnMarkerEntities.FirstOrDefault(x => x.AbsOrigin != null && origin != null &&
                Math.Round(x.AbsOrigin.X, 3) == Math.Round(origin.X, 3) &&
                Math.Round(x.AbsOrigin.Y, 3) == Math.Round(origin.Y, 3) &&
                Math.Round(x.AbsOrigin.Z, 3) == Math.Round(origin.Z, 3));
            if (spawnMarkerEntity != null)
            {
                // change color of entity if given
                if (color != null)
                {
                    // resize array if necessary
                    int oldLength = color.Length;
                    Array.Resize(ref color, 4);
                    // Set new elements to -1
                    for (int i = oldLength; i < color.Length; i++)
                    {
                        color[i] = -1;
                    }
                    if (color[0] == -1) color[0] = spawnMarkerEntity.Render.A;
                    if (color[1] == -1) color[1] = spawnMarkerEntity.Render.R;
                    if (color[2] == -1) color[2] = spawnMarkerEntity.Render.G;
                    if (color[3] == -1) color[3] = spawnMarkerEntity.Render.B;
                    spawnMarkerEntity.Render = Color.FromArgb(color[0], color[1], color[2], color[3]);
                }
                spawnMarkerEntity.DispatchSpawn();
                Console.WriteLine(Localizer["spawnpoints.marker.changed"]);
                return true;
            }
            return false;
        }
    }
}
