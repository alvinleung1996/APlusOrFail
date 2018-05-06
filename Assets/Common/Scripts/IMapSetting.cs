using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace APlusOrFail
{
    using Objects;
    using Components;

    public interface IMapSetting
    {
        string name { get; }

        IReadOnlyList<IRoundSetting> roundSettings { get; }
        int minRoundCount { get; }

        IReadOnlyList<IReadOnlySharedPlayerSetting> playerSettings { get; }

        int passPoints { get; }
    }

    public interface IRoundSetting
    {
        string name { get; }
        int points { get; }
        float timeLimit { get; }
        MapGridPlacer spawnArea { get; }
        IReadOnlyList<ObjectPrefabInfo> usableObjects { get; }
        IReadOnlyDictionary<PlayerPointsChangeReason, int> pointsMap { get; }
        IReadOnlyDictionary<PlayerPointsChangeReason, Color> rankColorMap { get; }
    }


    public static class MapSettingExtensions
    {
        //public static int GetMapScore(this IMapSetting mapStat)
        //{
        //    return mapStat.roundSettings.Sum(s => s.points);
        //}
    }
}
