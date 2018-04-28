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
        MapArea mapArea { get; }
        AutoResizeCamera camera { get; }
    }

    public interface IRoundSetting
    {
        string name { get; }
        int roundScore { get; }
        IRoundScoreSetting scoreSetting { get; }
        IReadOnlyList<ObjectPrefabInfo> usableObjects { get; }
        MapGridPlacer spawnArea { get; }
        IRoundRankColorSetting rankColorSetting { get; }
    }

    public interface IRoundScoreSetting
    {
       int this[PlayerScoreChangeReason reason] { get; }
    }

    public interface IRoundRankColorSetting
    {
        Color this[PlayerScoreChangeReason reason] { get; }
    }


    public static class MapSettingExtensions
    {
        public static int GetMapScore(this IMapSetting mapStat)
        {
            return mapStat.roundSettings.Sum(s => s.roundScore);
        }
    }
}
