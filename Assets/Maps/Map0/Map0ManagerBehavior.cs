﻿using System.Collections.Generic;
using UnityEngine;

namespace APlusOrFail.Maps
{
    using Components;
    using Objects;

    public class Map0ManagerBehavior : MapManagerBehavior
    {
        public float timeLimit = 300;
        public List<ObjectPrefabInfo> usableObjects;
        public MapGridPlacer spawnArea;
        

        protected override IEnumerable<IRoundSetting> roundSettings => new IRoundSetting[]
        {
            new RoundSetting("Ask the professor", 50, timeLimit, spawnArea, usableObjects, defaultRoundPointSetting, defaultRoundRankColorSetting),
            new RoundSetting("Capture the professor", 50, timeLimit, spawnArea, usableObjects, defaultRoundPointSetting, defaultRoundRankColorSetting),
            new RoundSetting("Ask the professor", 50, timeLimit, spawnArea, usableObjects, defaultRoundPointSetting, defaultRoundRankColorSetting),
            new RoundSetting("Capture the professor", 50, timeLimit, spawnArea, usableObjects, defaultRoundPointSetting, defaultRoundRankColorSetting),
            new RoundSetting("Ask the professor", 50, timeLimit, spawnArea, usableObjects, defaultRoundPointSetting, defaultRoundRankColorSetting)
        };

        protected override int minRoundCount => 3;
        protected override int passPoints => 150;

    }
}
