using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using System.Collections.Concurrent;
using System.Reflection;

namespace MapModifiers.Utils
{
    public static class GameRules
    {
        public static CCSGameRulesProxy? _gameRulesProxy;
        public static CCSGameRules? _gameRules;
        private static IEnumerable<CCSTeam>? _teamManager;
        private static readonly ConcurrentDictionary<string, PropertyInfo?> _rulePropertyCache = new(StringComparer.Ordinal);

        private static CCSGameRules? GetGameRule()
        {
            if (_gameRules == null
                || _gameRulesProxy == null
                || !_gameRulesProxy.IsValid)
            {
                _gameRulesProxy = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
                    .FirstOrDefault(static e => e != null && e.IsValid);
                _gameRules = _gameRulesProxy?.GameRules;
            }
            return _gameRules;
        }

        private static bool TryGetGameRules(out CCSGameRules rules, out CCSGameRulesProxy proxy)
        {
            _ = GetGameRule();
            if (_gameRules != null && _gameRulesProxy != null)
            {
                rules = _gameRules;
                proxy = _gameRulesProxy;
                return true;
            }
            rules = null!;
            proxy = null!;
            return false;
        }

        public static object? Get(string rule)
        {
            if (!TryGetGameRules(out CCSGameRules? rules, out _))
            {
                return null;
            }

            Type type = rules.GetType();
            string key = string.Concat(type.FullName, ":", rule);
            PropertyInfo? property = _rulePropertyCache.GetOrAdd(key, _ => type.GetProperty(rule));
            return property?.CanRead == true ? property.GetValue(rules) : null;
        }

        public static void SetRoundTime(int seconds)
        {
            if (TryGetGameRules(out CCSGameRules? rules, out CCSGameRulesProxy? proxy))
            {
                rules.RoundTime = seconds;
                Utilities.SetStateChanged(proxy, "CCSGameRulesProxy", "m_pGameRules");
            }
        }

        public static void TerminateRound(RoundEndReason reason, float delay = 0f)
        {
            if (TryGetGameRules(out CCSGameRules? rules, out CCSGameRulesProxy? proxy))
            {
                rules.RoundsPlayedThisPhase++;
                rules.ITotalRoundsPlayed++;
                rules.TotalRoundsPlayed++;
                rules.TerminateRound(delay, reason);
                Utilities.SetStateChanged(proxy, "CCSGameRulesProxy", "m_pGameRules");
            }
        }

        private static IEnumerable<CCSTeam>? GetTeamManager()
        {
            if (_teamManager == null || _teamManager.Any(static t => t == null || !t.IsValid))
            {
                _teamManager = Utilities.FindAllEntitiesByDesignerName<CCSTeam>("cs_team_manager");
            }
            return _teamManager;
        }

        public static void SetTeamScore(int score, CsTeam team)
        {
            IEnumerable<CCSTeam>? teamManager = GetTeamManager();
            if (teamManager == null)
            {
                return;
            }

            byte teamId = (byte)team;
            foreach (CCSTeam entry in teamManager)
            {
                if (entry.TeamNum == teamId)
                {
                    entry.Score = score;
                    Utilities.SetStateChanged(entry, "CTeam", "m_iScore");
                    break;
                }
            }
        }

        public static int GetTeamScore(CsTeam team)
        {
            IEnumerable<CCSTeam>? teamManager = GetTeamManager();
            if (teamManager == null)
            {
                return 0;
            }

            byte teamId = (byte)team;
            foreach (CCSTeam entry in teamManager)
            {
                if (entry.TeamNum == teamId)
                {
                    return entry.Score;
                }
            }

            return 0;
        }

        public static void SetMaxTplayers(int maxPlayers)
        {
            if (TryGetGameRules(out CCSGameRules? rules, out CCSGameRulesProxy? proxy))
            {
                rules.NumSpawnableTerrorist = maxPlayers;
                rules.MaxNumTerrorists = maxPlayers;
            }
        }

        public static void SetMaxCTplayers(int maxPlayers)
        {
            if (TryGetGameRules(out CCSGameRules? rules, out CCSGameRulesProxy? proxy))
            {
                rules.NumSpawnableCT = maxPlayers;
                rules.MaxNumCTs = maxPlayers;
            }
        }

        public static void ResetCaches()
        {
            _gameRules = null;
            _gameRulesProxy = null;
            _teamManager = null;
            _rulePropertyCache.Clear();
        }
    }
}