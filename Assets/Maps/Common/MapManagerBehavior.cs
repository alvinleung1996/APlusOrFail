using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace APlusOrFail.Maps
{
    using Objects;
    using Components;
    using SceneStates.DefaultSceneState;

    public abstract class MapManagerBehavior : MonoBehaviour, IMapManager
    {
        protected class MapSetting : IMapSetting
        {
            public string name { get; }
            public IReadOnlyList<IRoundSetting> roundSettings { get; }
            public MapArea mapArea { get; }
            public AutoResizeCamera camera { get; }

            public MapSetting(string name, IEnumerable<IRoundSetting> roundSettings, MapArea mapArea, AutoResizeCamera camera)
            {
                this.name = name;
                this.roundSettings = new ReadOnlyCollection<IRoundSetting>(roundSettings.ToList());
                this.mapArea = mapArea;
                this.camera = camera;
            }
        }

        protected class RoundSetting : IRoundSetting
        {
            public string name { get; }
            public int roundScore { get; }
            public IReadOnlyList<ObjectPrefabInfo> usableObjects { get; }
            public MapGridPlacer spawnArea { get; }
            public IRoundScoreSetting scoreSetting { get; }
            public IRoundRankColorSetting rankColorSetting { get; }

            public RoundSetting(string name, int roundScore, IEnumerable<ObjectPrefabInfo> usableObjects, MapGridPlacer spawnArea,
                IRoundScoreSetting scoreSetting, IRoundRankColorSetting rankColorSetting)
            {
                this.name = name;
                this.roundScore = roundScore;
                this.usableObjects = new ReadOnlyCollection<ObjectPrefabInfo>(usableObjects.ToList());
                this.spawnArea = spawnArea;
                this.scoreSetting = scoreSetting;
                this.rankColorSetting = rankColorSetting;
            }
        }

        protected class RoundScoreSetting : IRoundScoreSetting
        {
            private Dictionary<PlayerScoreChangeReason, int> map = new Dictionary<PlayerScoreChangeReason, int>();

            public int this[PlayerScoreChangeReason reason]
            {
                get { return map[reason]; }
                set { map[reason] = value; }
            }
        }

        protected static IRoundScoreSetting defaultRoundScoreSetting { get; } = new RoundScoreSetting
        {
            [PlayerScoreChangeReason.Won] = 30,
            [PlayerScoreChangeReason.KillOtherByTrap] = 10
        };

        protected class RoundRankColorSetting : IRoundRankColorSetting
        {
            private Dictionary<PlayerScoreChangeReason, Color> map = new Dictionary<PlayerScoreChangeReason, Color>();

            public Color this[PlayerScoreChangeReason reason]
            {
                get { return map[reason]; }
                set { map[reason] = value; }
            }
        }

        protected IRoundRankColorSetting defaultRoundRankColorSetting => new RoundRankColorSetting
        {
            [PlayerScoreChangeReason.Won] = Color.green,
            [PlayerScoreChangeReason.KillOtherByTrap] = Color.yellow
        };


        public DefaultSceneState defaultSceneState;
        public string mapName;
        public MapArea mapArea;
        public new AutoResizeCamera camera;
          
        public IMapStat stat { get; protected set; }


        protected virtual void Awake()
        {
            if (((IMapManager)this).Register())
            {
                stat = new MapStat(GetMapSetting());
            }
            else
            {
                Debug.LogErrorFormat("There is another map manager already!");
                Destroy(this);
            }
        }

        protected virtual void Start()
        {
            FindObjectOfType<SceneStateManager>().Push(defaultSceneState, stat);
        }

        protected virtual void OnDestroy()
        {
            ((IMapManager)this).Unregister();
        }

        protected virtual IMapSetting GetMapSetting() => new MapSetting(mapName, GetRoundSettings(), mapArea, camera);

        protected abstract IEnumerable<IRoundSetting> GetRoundSettings();
    }
}
