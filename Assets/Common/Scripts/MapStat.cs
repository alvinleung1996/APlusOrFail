using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace APlusOrFail
{
    using Objects;
    using Components;

    public class MapStat : IMapStat
    {
        public string name { get; }
        
        public IReadOnlyList<IRoundStat> roundStats { get; }
        IReadOnlyList<IRoundSetting> IMapSetting.roundSettings => roundStats;
        IReadOnlyList<IReadonlyRoundStat> IReadOnlyMapStat.roundStats => roundStats;
        public int minRoundCount { get; }

        public IReadOnlyList<IPlayerStat> playerStats { get; }
        IReadOnlyList<IReadOnlySharedPlayerSetting> IMapSetting.playerSettings => playerStats;
        IReadOnlyList<IReadOnlyPlayerStat> IReadOnlyMapStat.playerStats => playerStats;

        public IRoundPlayerStats roundPlayerStats { get; }
        IReadOnlyRoundPlayerStats IReadOnlyMapStat.roundPlayerStats => roundPlayerStats;

        public int passPoints { get; }

        public int currentRound { get; set; } = -1;

        public MapStat(IMapSetting mapSetting)
        {
            name = mapSetting.name;

            List<IRoundStat> roundStats = new List<IRoundStat>(mapSetting.roundSettings.Count);
            for (int i = 0; i < mapSetting.roundSettings.Count; ++i)
            {
                roundStats.Add(new RoundStat(mapSetting.roundSettings[i]));
            }
            this.roundStats = new ReadOnlyCollection<IRoundStat>(roundStats);
            minRoundCount = mapSetting.minRoundCount;

            List<IPlayerStat> playerStats = new List<IPlayerStat>(mapSetting.playerSettings.Count);
            for (int j = 0; j < mapSetting.playerSettings.Count; ++j)
            {
                playerStats.Add(new PlayerStat(mapSetting.playerSettings[j]));
            }
            this.playerStats = new ReadOnlyCollection<IPlayerStat>(playerStats);

            IRoundPlayerStat[,] roundPlayerStats = new IRoundPlayerStat[mapSetting.roundSettings.Count, mapSetting.playerSettings.Count];
            for (int i = 0; i < mapSetting.roundSettings.Count; ++i)
            {
                for (int j = 0; j < mapSetting.playerSettings.Count; ++j)
                {
                    roundPlayerStats[i, j] = new RoundPlayerStat();
                }
            }
            this.roundPlayerStats = new RoundPlayerStats(roundPlayerStats);

            this.passPoints = mapSetting.passPoints;
        }
    }

    public class RoundPlayerStats : IRoundPlayerStats
    {
        private readonly IRoundPlayerStat[,] stats;
        public IRoundPlayerStat this[int roundOrder, int playerOrder] { get { return stats[roundOrder, playerOrder]; } set { stats[roundOrder, playerOrder] = value; } }
        IReadOnlyRoundPlayerStat IReadOnlyRoundPlayerStats.this[int roundOrder, int playerOrder] => this[roundOrder, playerOrder];

        public RoundPlayerStats(IRoundPlayerStat[,] stats)
        {
            this.stats = stats;
        }
    }

    public class RoundStat : IRoundStat
    {
        public string name { get; }
        public int points { get; }
        public float timeLimit { get; }
        public MapGridPlacer spawnArea { get; }
        public IReadOnlyList<ObjectPrefabInfo> usableObjects { get; }
        public IReadOnlyDictionary<PlayerPointsChangeReason, int> pointsMap { get; }
        public IReadOnlyDictionary<PlayerPointsChangeReason, Color> rankColorMap { get; }
        public RoundState state { get; set; } = RoundState.None;
        public bool tooEasyNoPoint { get; set; }

        public RoundStat(IRoundSetting roundSetting)
        {
            name = roundSetting.name;
            points = roundSetting.points;
            timeLimit = roundSetting.timeLimit;
            spawnArea = roundSetting.spawnArea;
            usableObjects = new List<ObjectPrefabInfo>(roundSetting.usableObjects);
            pointsMap = roundSetting.pointsMap.ToDictionary(p => p.Key, p => p.Value);
            rankColorMap = roundSetting.rankColorMap.ToDictionary(p => p.Key, p => p.Value);
        }
    }

    public class PlayerStat : IPlayerStat
    {
        public bool wonOverall { get; set; }
        public int id { get; }
        public string name { get; }
        public Color color { get; }
        public int characterSpriteId { get; }
        public IReadOnlyDictionary<PlayerAction, KeyCode> actionMap { get; }

        public PlayerStat(IReadOnlySharedPlayerSetting playerSetting)
        {
            id = playerSetting.id;
            name = playerSetting.name;
            color = playerSetting.color;
            characterSpriteId = playerSetting.characterSpriteId;
            actionMap = playerSetting.actionMap.ToDictionary(p => p.Key, p => p.Value);
        }
    }

    public class RoundPlayerStat : IRoundPlayerStat
    {
        public ObjectPrefabInfo selectedObjectPrefab { get; set; }
        public bool won { get; set; }

        public IList<IReadOnlyPlayerHealthChange> healthChanges { get; } = new List<IReadOnlyPlayerHealthChange>();
        private IReadOnlyList<IReadOnlyPlayerHealthChange> _readonlyHealthChanges;
        IReadOnlyList<IReadOnlyPlayerHealthChange> IReadOnlyRoundPlayerStat.healthChanges => _readonlyHealthChanges;

        public IList<IReadOnlyPlayerScoreChange> scoreChanges { get; } = new List<IReadOnlyPlayerScoreChange>();
        private IReadOnlyList<IReadOnlyPlayerScoreChange> _readonlyScoreChanges;
        IReadOnlyList<IReadOnlyPlayerScoreChange> IReadOnlyRoundPlayerStat.scoreChanges => _readonlyScoreChanges;

        public RoundPlayerStat()
        {
            _readonlyHealthChanges = new ReadOnlyCollection<IReadOnlyPlayerHealthChange>(healthChanges);
            _readonlyScoreChanges = new ReadOnlyCollection<IReadOnlyPlayerScoreChange>(scoreChanges);
        }
    }
    
    public class ReadOnlyPlayerHealthChange : IReadOnlyPlayerHealthChange
    {
        public PlayerHealthChangeReason reason { get; }
        public int delta { get; }
        public GameObject cause { get; }

        public ReadOnlyPlayerHealthChange(PlayerHealthChangeReason reason, int healthDelta, GameObject cause)
        {
            this.delta = healthDelta;
            this.reason = reason;
            this.cause = cause;
        }
    }

    public class ReadOnlyPlayerPointsChange : IReadOnlyPlayerScoreChange
    {
        public PlayerPointsChangeReason reason { get; }
        public int delta { get; }
        public Color rankColor { get; }
        public GameObject cause { get; }

        public ReadOnlyPlayerPointsChange(PlayerPointsChangeReason reason, int scoreDelta, Color rankColor, GameObject cause)
        {
            this.delta = scoreDelta;
            this.reason = reason;
            this.rankColor = rankColor;
            this.cause = cause;
        }
    }
}
