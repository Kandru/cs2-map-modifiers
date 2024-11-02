using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
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

        private int ShowSpawnPointMarkers()
        {
            var spawnEntities = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_").ToArray();
            var count = 0;
            foreach (var entity in spawnEntities)
            {
                count++;
                var spawnPoint = entity.As<SpawnPoint>();
                CDynamicProp spawnEntity = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic")!;
                if (spawnPoint.AbsOrigin == null || spawnEntity.AbsOrigin == null
                    || spawnPoint.AbsRotation == null || spawnEntity.AbsRotation == null
                    || !spawnEntity.IsValid || !spawnPoint.IsValid)
                {
                    Console.WriteLine("[MapModifiersPlugin] ERROR: could not spawn entity");
                    return 0;
                }
                // random string due to a problem when deleting entities and create them again with the same name
                // they will not show up again
                var randomString = new string(Enumerable.Range(0, 5).Select(_ => (char)new Random().Next('a', 'z' + 1)).ToArray());
                spawnEntity.Globalname = $"mapmodifiers_spawnpointmarker_{randomString}";
                spawnEntity.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_NONE;
                spawnEntity.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_NONE;
                spawnEntity.AbsOrigin.X = spawnPoint.AbsOrigin.X;
                spawnEntity.AbsOrigin.Y = spawnPoint.AbsOrigin.Y;
                spawnEntity.AbsOrigin.Z = spawnPoint.AbsOrigin.Z;
                spawnEntity.AbsRotation.X = spawnPoint.AbsRotation.X;
                spawnEntity.AbsRotation.Y = spawnPoint.AbsRotation.Y;
                spawnEntity.AbsRotation.Z = spawnPoint.AbsRotation.Z;
                spawnEntity.DispatchSpawn();
                spawnEntity.SetModel("models/props/cs_office/file_cabinet1.vmdl");
                if (spawnPoint.DesignerName.Contains("counterterrorist")) {
                    // CT spawn
                    spawnEntity.Render = Color.FromArgb(0, 0, 255);
                    // if custom CT spawn
                    if (spawnPoint.Globalname.Contains("mapmodifiers_spawnpoint_"))
                        spawnEntity.Render = Color.FromArgb(4, 138, 255);
                }else{
                    spawnEntity.Render = Color.FromArgb(255, 0, 0);
                    // if custom T spawn
                    if (spawnPoint.Globalname.Contains("mapmodifiers_spawnpoint_"))
                        spawnEntity.Render = Color.FromArgb(255, 90, 0);
                }
            }
            return count;
        }

        private int RemoveSpawnPointMarkers()
        {
            var spawnEntities = Utilities.FindAllEntitiesByDesignerName<CDynamicProp>("prop_dynamic").ToArray();
            var count = 0;
            foreach (var entity in spawnEntities.Where(x => x.Globalname != null && x.Globalname.Contains("mapmodifiers_spawnpointmarker")))
            {
                count++;
                var spawnEntity = entity.As<CDynamicProp>();
                spawnEntity.Remove();
            }
            return count;
        }

        private void CreateSpawnPoints()
        {
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

        private void RemoveSpawnPoint(string globalname = "") {
            var spawnEntities = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_").ToArray();
            foreach (var entity in spawnEntities.Where(x => x.Globalname != null && x.Globalname == globalname))
            {
                var spawnEntity = entity.As<SpawnPoint>();
                spawnEntity.Remove();
            }
        }

        private void CreateSpawnPoint(string type, MapConfigSpawnPoint spawnPoint) {
            SpawnPoint spawn;
            if (type == "t") {
                spawn = Utilities.CreateEntityByName<SpawnPoint>("info_player_terrorist")!;
            }else{
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
            spawn.AbsOrigin.Z = spawnPoint.Origin[2] + 10; // add a little extra height like the originals
            spawn.AbsRotation.X = spawnPoint.Angle[0];
            spawn.AbsRotation.Y = spawnPoint.Angle[1];
            spawn.AbsRotation.Z = spawnPoint.Angle[2];
            // spawn it
            spawn.DispatchSpawn();
        }
    }
}
