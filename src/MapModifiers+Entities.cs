using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace MapModifiers
{
    public partial class MapModifiers : BasePlugin, IPluginConfig<PluginConfig>
    {
        private void EntitiesOnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            Console.WriteLine(Localizer["entities.onmapstart"].Value
                           .Replace("{mapName}", _currentMap));
            foreach (MapConfig mapConfig in _currentMapConfigs)
            {
                foreach (MapConfigEntity entity in mapConfig.Entities)
                {
                    CreateEntity(entity);
                }
            }
        }

        private CBaseEntity? CreateEntity(MapConfigEntity entity)
        {
            var lookupEntities = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>(entity.ClassName).ToArray();
            // check if entity already exists at that origin
            if (lookupEntities.Any(x => x.AbsOrigin != null && entity.Origin != null &&
                Math.Round(x.AbsOrigin.X, 3) == Math.Round(entity.Origin[0], 3) &&
                Math.Round(x.AbsOrigin.Y, 3) == Math.Round(entity.Origin[1], 3) &&
                Math.Round(x.AbsOrigin.Z, 3) == Math.Round(entity.Origin[2], 3)))
            {
                return null;
            }
            CBaseEntity newEntity = Utilities.CreateEntityByName<CBaseEntity>(entity.ClassName)!;
            if (newEntity == null
                || !newEntity.IsValid
                || newEntity.AbsOrigin == null
                || newEntity.AbsRotation == null)
            {
                Console.WriteLine(Localizer["entities.error.noentity"]);
                return null;
            }
            // set attributes
            newEntity.Globalname = $"mapmodifiers_{entity.ClassName}_{(int)entity.Origin[0]}{(int)entity.Origin[1]}{(int)entity.Origin[2]}";
            newEntity.AbsOrigin.X = entity.Origin[0];
            newEntity.AbsOrigin.Y = entity.Origin[1];
            newEntity.AbsOrigin.Z = entity.Origin[2];
            newEntity.AbsRotation.X = entity.Angle[0];
            newEntity.AbsRotation.Y = entity.Angle[1];
            newEntity.AbsRotation.Z = entity.Angle[2];
            newEntity.TeamNum = (byte)entity.Team;
            // spawn it
            newEntity.DispatchSpawn();
            Console.WriteLine(Localizer["entities.created"].Value
                .Replace("{type}", entity.ClassName)
                .Replace("{origin}", $"{entity.Origin[0]} {entity.Origin[1]} {entity.Origin[2]}")
                .Replace("{angle}", $"{entity.Angle[0]} {entity.Angle[1]} {entity.Angle[2]}"));
            return newEntity;
        }

        private CBaseEntity? GetNearestEntity(Vector origin, float maxDistance = 200)
        {
            var allEntities = Utilities.GetAllEntities().ToArray();
            CBaseEntity? nearestEntity = null;
            float nearestDistance = float.MaxValue;
            foreach (var entity in allEntities)
            {
                if (entity == null) continue;
                if (entity.DesignerName == null
                    || entity.DesignerName == "cs_player_controller"
                    || entity.DesignerName == "cs_player_manager"
                    || entity.DesignerName == "observer") continue;
                if (entity.As<CBaseEntity>() == null) continue;
                CBaseEntity baseEntity = entity.As<CBaseEntity>();
                if (baseEntity == null
                    || baseEntity.AbsOrigin == null
                    || (baseEntity.Globalname != null && baseEntity.Globalname.Contains("mapmodifiers_spawnpointmarker"))) continue;
                float distance = CalculateDistance(baseEntity.AbsOrigin, origin);
                if (distance <= maxDistance && distance < nearestDistance && distance >= 1)
                {
                    nearestDistance = distance;
                    nearestEntity = baseEntity;
                }
            }
            return nearestEntity;
        }
    }
}
