using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace APlusOrFail
{
    using Objects;

    public interface IReadOnlyMapStat : IMapSetting
    {
        IReadOnlyList<IReadonlyRoundStat> roundStats { get; }
        IReadOnlyList<IReadOnlyPlayerStat> playerStats { get; }
        IReadOnlyRoundPlayerStats roundPlayerStats { get; }
        int currentRound { get; }
    }

    public interface IMapStat : IReadOnlyMapStat
    {
        new IReadOnlyList<IRoundStat> roundStats { get; }
        new IReadOnlyList<IPlayerStat> playerStats { get; }
        new IRoundPlayerStats roundPlayerStats { get; }
        new int currentRound { get; set; }
    }

    public interface IReadOnlyRoundPlayerStats
    {
        IReadOnlyRoundPlayerStat this[int roundOrder, int playerOrder] { get; }
    }

    public interface IRoundPlayerStats : IReadOnlyRoundPlayerStats
    {
        new IRoundPlayerStat this[int roundOrder, int playerOrder] { get; }
    }
    


    public enum RoundState
    {
        None,
        SelectingObjects,
        PlacingObjects,
        Playing,
        Ranking,
        Result
    }

    public interface IReadonlyRoundStat : IRoundSetting
    {
        RoundState state { get; }
        bool tooEasyNoPoint { get; }
    }

    public interface IRoundStat : IReadonlyRoundStat
    {
        new RoundState state { get; set; }
        new bool tooEasyNoPoint { get; set; }
    }



    public interface IReadOnlyPlayerStat : IReadOnlyPlayerSetting
    {
        bool wonOverall { get; }
    }

    public interface IPlayerStat : IReadOnlyPlayerStat
    {
        new bool wonOverall { get; set; }
    }



    public interface IReadOnlyRoundPlayerStat
    {
        ObjectPrefabInfo selectedObjectPrefab { get; }
        bool won { get; }
        IReadOnlyList<IReadOnlyPlayerHealthChange> healthChanges { get; }
        IReadOnlyList<IReadOnlyPlayerScoreChange> scoreChanges { get; }
    }

    public interface IRoundPlayerStat : IReadOnlyRoundPlayerStat
    {
        new ObjectPrefabInfo selectedObjectPrefab { get; set; }
        new bool won { get; set; }
        new IList<IReadOnlyPlayerHealthChange> healthChanges { get; }
        new IList<IReadOnlyPlayerScoreChange> scoreChanges { get; }
    }



    public interface IReadOnlyPlayerHealthChange
    {
        PlayerHealthChangeReason reason { get; }
        int delta { get; }
        GameObject cause { get; }
    }

    public interface IReadOnlyPlayerScoreChange
    {
        PlayerPointsChangeReason reason { get; }
        int delta { get; }
        Color rankColor { get; }
        GameObject cause { get; }
    }

    public enum PlayerHealthChangeReason
    {
        No,
        ByTrap,
        ExitArea
    }

    public enum PlayerPointsChangeReason
    {
        No,
        Won,
        KillOtherByTrap
    }



    public static class MapStatExtensions
    {
        public static IEnumerable<IRoundPlayerStat> GetRoundPlayerStatOfRound(this IMapStat mapStat, int roundOrder)
        {
            return Enumerable.Range(0, mapStat.playerStats.Count).Select(i => mapStat.roundPlayerStats[roundOrder, i]);
        }

        public static IEnumerable<IRoundPlayerStat> GetRoundPlayerStatOfPlayer(this IMapStat mapStat, int playerOrder)
        {
            return Enumerable.Range(0, mapStat.roundStats.Count).Select(i => mapStat.roundPlayerStats[i, playerOrder]);
        }

        public static IRoundPlayerStat GetRoundPlayerStat(this IMapStat mapStat, int roundOrder, IReadOnlyPlayerSetting player) =>
            mapStat.roundPlayerStats[roundOrder, mapStat.playerStats.FindIndex(ps => ps == player)];

        public static IReadOnlyPlayerScoreChange CreatePointsChange(this IRoundSetting setting, PlayerPointsChangeReason reason, GameObject cause) =>
            new ReadOnlyPlayerPointsChange(reason, setting.pointsMap[reason], setting.rankColorMap[reason], cause);
    }
}
