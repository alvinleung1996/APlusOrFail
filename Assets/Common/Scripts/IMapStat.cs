using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace APlusOrFail
{
    using Objects;

    public interface IReadOnlyMapStat : IMapSetting
    {
        IReadOnlyList<IReadonlyRoundStat> roundStats { get; }
        int roundCount { get; }

        IReadOnlyList<IReadOnlyPlayerStat> playerStats { get; }
        int playerCount { get; }

        IReadOnlyRoundPlayerStat GetRoundPlayerStat(int roundOrder, int playerOrder);

        int currentRound { get; }
    }

    public interface IMapStat : IReadOnlyMapStat
    {
        new IReadOnlyList<IRoundStat> roundStats { get; }

        new IReadOnlyList<IPlayerStat> playerStats { get; }

        new IRoundPlayerStat GetRoundPlayerStat(int roundOrder, int playerOrder);

        new int currentRound { get; set; }
    }
    

    public enum RoundState
    {
        None,
        SelectingObjects,
        PlacingObjects,
        Playing,
        Ranking
    }

    public interface IReadonlyRoundStat : IRoundSetting
    {
        IReadOnlyMapStat mapStat { get; }

        int order { get; }
        RoundState state { get; }
        bool tooEasyNoPoint { get; }
    }

    public interface IRoundStat : IReadonlyRoundStat
    {
        new IMapStat mapStat { get; }
        new RoundState state { get; set; }
        new bool tooEasyNoPoint { get; set; }
    }


    public interface IReadOnlyPlayerStat
    {
        IReadOnlyMapStat mapStat { get; }

        int order { get; }
        Player player { get; }
        bool wonOverall { get; }
    }

    public interface IPlayerStat : IReadOnlyPlayerStat
    {
        new IMapStat mapStat { get; }

        new bool wonOverall { get; set; }
    }


    public interface IReadOnlyRoundPlayerStat
    {
        IReadOnlyMapStat mapStat { get; }
        IReadonlyRoundStat roundStat { get; }
        IReadOnlyPlayerStat playerStat { get; }

        ObjectPrefabInfo selectedObjectPrefab { get; }
        bool won { get; }
        IReadOnlyList<IPlayerHealthChange> healthChanges { get; }
        IReadOnlyList<IPlayerScoreChange> scoreChanges { get; }
    }

    public interface IRoundPlayerStat : IReadOnlyRoundPlayerStat
    {
        new IMapStat mapStat { get; }
        new IRoundStat roundStat { get; }
        new IPlayerStat playerStat { get; }

        new ObjectPrefabInfo selectedObjectPrefab { get; set; }
        new bool won { get; set; }
        new IList<IPlayerHealthChange> healthChanges { get; }
        new IList<IPlayerScoreChange> scoreChanges { get; }
    }


    public interface IPlayerHealthChange
    {
        PlayerHealthChangeReason reason { get; }
        int healthDelta { get; }
        GameObject cause { get; }
    }

    public interface IPlayerScoreChange
    {
        PlayerScoreChangeReason reason { get; }
        int scoreDelta { get; }
        Color rankColor { get; }
        GameObject cause { get; }
    }

    public enum PlayerHealthChangeReason
    {
        No,
        ByTrap,
        ExitArea
    }

    public enum PlayerScoreChangeReason
    {
        No,
        Won,
        KillOtherByTrap
    }


    public static class MapStatExtensions
    {
        public static IEnumerable<IRoundPlayerStat> GetRoundPlayerStatOfRound(this IMapStat mapStat, int roundOrder)
        {
            return Enumerable.Range(0, mapStat.playerCount).Select(i => mapStat.GetRoundPlayerStat(roundOrder, i));
        }

        public static IEnumerable<IRoundPlayerStat> GetRoundPlayerStatOfPlayer(this IMapStat mapStat, int playerOrder)
        {
            return Enumerable.Range(0, mapStat.roundCount).Select(i => mapStat.GetRoundPlayerStat(i, playerOrder));
        }

        public static IRoundPlayerStat GetRoundPlayerStat(this IMapStat mapStat, int roundOrder, Player player) =>
            mapStat.GetRoundPlayerStat(roundOrder, mapStat.playerStats.FindIndex(ps => ps.player == player));
    }
}
