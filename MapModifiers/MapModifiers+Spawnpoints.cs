using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

namespace MapModifiers
{
    public partial class MapModifiers : BasePlugin, IPluginConfig<PluginConfig>
    {
        private void OnMapStartSpawnPoints(string mapName, MapConfig mapConfig)
        {
            // TODO: does not work on map start. Should run on round start instead
            Console.WriteLine("[MapModifiersPlugin] Modifying spawn points for map " + mapName);
            // try to delete spawn points
            if (mapConfig.TSpawns.Count > 0 || mapConfig.CTSpawns.Count > 0)
            {
                if (mapConfig.DeleteOriginalSpawns)
                {
                    // sanity checks
                    if (mapConfig.TSpawns.Count == 0)
                    {
                        Console.WriteLine("[MapModifiersPlugin] WARNING: Map " + mapName + " has no configured spawns for CT, but according to configuration the original spawns shall be removed.");
                        Console.WriteLine("[MapModifiersPlugin] WARNING: This would result in the game having no spawns at all, which will let the server crash. That's why the original spawns are kept and the request to remove original spawns is just ignored!");
                    }
                    else if (mapConfig.CTSpawns.Count == 0)
                    {
                        Console.WriteLine("[MapModifiersPlugin] WARNING: Map " + mapName + " has no configured spawns for CT, but according to configuration the original spawns shall be removed.");
                        Console.WriteLine("[MapModifiersPlugin] WARNING: This would result in the game having no spawns at all, which will let the server crash. That's why the original spawns are kept and the request to remove original spawns is just ignored!");
                    }
                    else
                    {
                        RemoveSpawnPoints();
                    }
                }
            }
        }

        private SpawnPoint? GetNearestSpawnPoint(Vector origin, float maxDistance = 200)
        {
            var spawnEntities = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_").ToArray();
            SpawnPoint? nearestSpawn = null;
            float nearestDistance = float.MaxValue;
            foreach (var spawn in spawnEntities)
            {
                if (spawn.AbsOrigin == null) continue;
                float distance = CalculateDistance(spawn.AbsOrigin, origin);
                if (distance <= maxDistance && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestSpawn = spawn;
                }
            }
            return nearestSpawn;
        }

        private float CalculateDistance(Vector point1, Vector point2)
        {
            float dx = point1.X - point2.X;
            float dy = point1.Y - point2.Y;
            float dz = point1.Z - point2.Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
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
            var spawnMarkerEntities = Utilities.FindAllEntitiesByDesignerName<CDynamicProp>("prop_dynamic").Where(x => x.Globalname != null && x.Globalname.Contains("mapmodifiers_spawnpoint")).ToArray();
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
                    Console.WriteLine("[MapModifiersPlugin] WARNING: spawn point marker already exists at that position. Skipping!");
                    continue;
                }
                CDynamicProp spawnMarkerEntity = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic")!;
                if (spawnPointEntity.AbsOrigin == null || spawnMarkerEntity.AbsOrigin == null
                    || spawnPointEntity.AbsRotation == null || spawnMarkerEntity.AbsRotation == null
                    || !spawnMarkerEntity.IsValid || !spawnPointEntity.IsValid)
                {
                    Console.WriteLine("[MapModifiersPlugin] ERROR: could not spawn entity");
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
                var spawnPointConfig = mapConfig.TSpawns.Concat(mapConfig.CTSpawns).FirstOrDefault(x => x.Origin.SequenceEqual([spawnPointEntity.AbsOrigin.X, spawnPointEntity.AbsOrigin.Y, spawnPointEntity.AbsOrigin.Z]));
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
                    if (spawnPointEntity.Globalname != null && spawnPointEntity.Globalname.Contains("mapmodifiers_spawnpoint_"))
                        spawnMarkerEntity.Render = Color.FromArgb(alpha, 4, 138, 255);
                }
                else
                {
                    spawnMarkerEntity.Render = Color.FromArgb(255, 0, 0);
                    // if custom T spawn
                    if (spawnPointEntity.Globalname != null && spawnPointEntity.Globalname.Contains("mapmodifiers_spawnpoint_"))
                        spawnMarkerEntity.Render = Color.FromArgb(alpha, 255, 90, 0);
                }
            }
            return count;
        }

        private int RemoveSpawnPointMarkers()
        {
            var spawnMarkerEntities = Utilities.FindAllEntitiesByDesignerName<CDynamicProp>("prop_dynamic").Where(x => x.Globalname != null && x.Globalname.Contains("mapmodifiers_spawnpoint")).ToArray();
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
            var spawnMarkerEntities = Utilities.FindAllEntitiesByDesignerName<CDynamicProp>("prop_dynamic").Where(x => x.Globalname != null && x.Globalname.Contains("mapmodifiers_spawnpoint")).ToArray();
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
                Console.WriteLine("[MapModifiersPlugin] changed single spawn point marker");
                return true;
            }
            return false;
        }

        private void CreateSpawnPoints()
        {
            Console.WriteLine("[MapModifiersPlugin] creating custom spawn points");
            // add spawns corresponding to the configuration
            foreach (var mapConfig in _currentMapConfigs)
            {
                foreach (var spawnConfig in mapConfig.TSpawns)
                {
                    CreateSpawnPoint("t", new MapConfigSpawnPoint
                    {
                        Origin = [spawnConfig.Origin[0], spawnConfig.Origin[1], spawnConfig.Origin[2]],
                        Angle = [spawnConfig.Angle[0], spawnConfig.Angle[1], spawnConfig.Angle[2]]
                    });
                }

                foreach (var spawnConfig in mapConfig.CTSpawns)
                {
                    CreateSpawnPoint("ct", new MapConfigSpawnPoint
                    {
                        Origin = [spawnConfig.Origin[0], spawnConfig.Origin[1], spawnConfig.Origin[2]],
                        Angle = [spawnConfig.Angle[0], spawnConfig.Angle[1], spawnConfig.Angle[2]]
                    });
                }
            }
        }

        private int RemoveSpawnPoints(bool removeCustom = false)
        {
            var spawnEntities = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_").ToArray();
            var count = 0;
            foreach (var entity in spawnEntities)
            {
                count++;
                var spawnEntity = entity.As<SpawnPoint>();
                if (removeCustom || !spawnEntity.Globalname.Contains("mapmodifiers_spawnpoint")) spawnEntity.Remove();
            }
            return count;
        }

        private void RemoveSpawnPoint(string globalname = "")
        {
            var spawnEntities = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_").ToArray();
            foreach (var entity in spawnEntities.Where(x => x.Globalname != null && x.Globalname == globalname))
            {
                var spawnEntity = entity.As<SpawnPoint>();
                spawnEntity.Remove();
            }
        }

        private void CreateSpawnPoint(string type, MapConfigSpawnPoint spawnPoint)
        {
            var spawnPointEntities = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_").ToArray();
            // check if spawn point already exists at that origin
            if (spawnPointEntities.Any(x => x.AbsOrigin != null && spawnPoint.Origin != null &&
                Math.Round(x.AbsOrigin.X, 3) == Math.Round(spawnPoint.Origin[0], 3) &&
                Math.Round(x.AbsOrigin.Y, 3) == Math.Round(spawnPoint.Origin[1], 3) &&
                Math.Round(x.AbsOrigin.Z, 3) == Math.Round(spawnPoint.Origin[2], 3)))
            {
                Console.WriteLine("[MapModifiersPlugin] WARNING: spawn point already exists at that position. Skipping!");
                return;
            }
            SpawnPoint spawn;
            if (type == "t")
            {
                spawn = Utilities.CreateEntityByName<SpawnPoint>("info_player_terrorist")!;
            }
            else
            {
                spawn = Utilities.CreateEntityByName<SpawnPoint>("info_player_counterterrorist")!;
            }
            if (spawn == null || spawn.AbsOrigin == null || spawn.AbsRotation == null || !spawn.IsValid)
            {
                Console.WriteLine("[MapModifiersPlugin] ERROR: could not spawn entity");
                return;
            }
            // set attributes
            spawn.Globalname = $"mapmodifiers_spawnpoint_{(int)spawnPoint.Origin[0]}{(int)spawnPoint.Origin[1]}{(int)spawnPoint.Origin[2]}";
            spawn.AbsOrigin.X = spawnPoint.Origin[0];
            spawn.AbsOrigin.Y = spawnPoint.Origin[1];
            spawn.AbsOrigin.Z = spawnPoint.Origin[2];
            spawn.AbsRotation.X = spawnPoint.Angle[0];
            spawn.AbsRotation.Y = spawnPoint.Angle[1];
            spawn.AbsRotation.Z = spawnPoint.Angle[2];
            // spawn it
            spawn.DispatchSpawn();
            Console.WriteLine("[MapModifiersPlugin] created spawn point for " + type + " at " + spawnPoint.Origin[0] + ", " + spawnPoint.Origin[1] + ", " + spawnPoint.Origin[2] + " with angle " + spawnPoint.Angle[0] + ", " + spawnPoint.Angle[1] + ", " + spawnPoint.Angle[2]);
        }

        private void CountSpawnPoints()
        {
            var spawnEntities = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_").ToArray();
            Console.WriteLine("[MapModifiersPlugin] Counted " + spawnEntities.Length + " spawn points");
            if (spawnEntities.Length < Server.MaxPlayers)
            {
                Console.WriteLine($"[MapModifiersPlugin] WARNING: Only {spawnEntities.Length} spawn points for {Server.MaxPlayers} players!");
                SendGlobalChatMessage($"[MapModifiersPlugin] WARNING: Only {spawnEntities.Length} spawn points for {Server.MaxPlayers} players!");
            }
        }
    }
}
