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
        MapArea mapArea { get; }
        AutoResizeCamera camera { get; }

        IReadOnlyList<IRoundSetting> roundSettings { get; }
        int minRoundCount { get; }

        IReadOnlyList<IReadOnlyPlayerSetting> playerSettings { get; }

        int passPoints { get; }
    }

    public interface IRoundSetting
    {
        string name { get; }
        int points { get; }
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
