using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            public MapArea mapArea { get; }
            public AutoResizeCamera camera { get; }
            public IReadOnlyList<IRoundSetting> roundSettings { get; }
            public int minRoundCount { get; }
            public IReadOnlyList<IReadOnlySharedPlayerSetting> playerSettings { get; }
            public int passPoints { get; }

            public MapSetting(string name, MapArea mapArea, AutoResizeCamera camera,
                IEnumerable<IRoundSetting> roundSettings, int minRoundCount,
                IEnumerable<IReadOnlySharedPlayerSetting> playerSettings, int passPoints)
            {
                this.name = name;
                this.mapArea = mapArea;
                this.camera = camera;
                this.roundSettings = roundSettings.ToList();
                this.minRoundCount = minRoundCount;
                this.playerSettings = playerSettings.ToList();
                this.passPoints = passPoints;
            }
        }

        protected class RoundSetting : IRoundSetting
        {
            public string name { get; }
            public int points { get; }
            public MapGridPlacer spawnArea { get; }
            public IReadOnlyList<ObjectPrefabInfo> usableObjects { get; }
            public IReadOnlyDictionary<PlayerPointsChangeReason, int> pointsMap { get; }
            public IReadOnlyDictionary<PlayerPointsChangeReason, Color> rankColorMap { get; }

            public RoundSetting(string name, int points, MapGridPlacer spawnArea, IEnumerable<ObjectPrefabInfo> usableObjects,
                IReadOnlyDictionary<PlayerPointsChangeReason, int> pointsMap, IReadOnlyDictionary<PlayerPointsChangeReason, Color> rankColorMap)
            {
                this.name = name;
                this.points = points;
                this.spawnArea = spawnArea;
                this.usableObjects = new ReadOnlyCollection<ObjectPrefabInfo>(usableObjects.ToList());
                this.pointsMap = pointsMap.ToDictionary(p => p.Key, p => p.Value);
                this.rankColorMap = rankColorMap.ToDictionary(p => p.Key, p => p.Value);
            }
        }

        protected static IReadOnlyDictionary<PlayerPointsChangeReason, int> defaultRoundPointSetting => new Dictionary<PlayerPointsChangeReason, int>
        {
            [PlayerPointsChangeReason.Won] = 40,
            [PlayerPointsChangeReason.KillOtherByTrap] = 10
        };

        protected static IReadOnlyDictionary<PlayerPointsChangeReason, Color> defaultRoundRankColorSetting => new Dictionary<PlayerPointsChangeReason, Color>
        {
            [PlayerPointsChangeReason.Won] = Color.green,
            [PlayerPointsChangeReason.KillOtherByTrap] = Color.yellow
        };


        public SceneStateManager sceneStateManager;
        public DefaultSceneState defaultSceneState;
        public string mapName;
        public MapArea mapArea;
        public new AutoResizeCamera camera;
        public GameObject test_characterSprite;

        public IMapStat stat { get; protected set; }


        protected virtual void Awake()
        {
            if (((IMapManager)this).Register())
            {
                stat = new MapStat(mapSetting);
            }
            else
            {
                Debug.LogErrorFormat("There is another map manager already!");
                Destroy(this);
            }
        }

        protected virtual void Start()
        {
            sceneStateManager.onLastSceneStatePoped += OnLastSceneStatePoped;
            sceneStateManager.Push(defaultSceneState, stat);
        }

        protected virtual void OnDestroy()
        {
            ((IMapManager)this).Unregister();
        }

        protected virtual IMapSetting mapSetting => new MapSetting(mapName, mapArea, camera, roundSettings, minRoundCount, playerSettings, passPoints);
        protected abstract IEnumerable<IRoundSetting> roundSettings { get; }
        protected abstract int minRoundCount { get; }
        protected virtual IEnumerable<IReadOnlySharedPlayerSetting> playerSettings
        {
            get
            {
                if (SharedData.CharacterSpriteIdMap == null || SharedData.playerSettings == null)
                {
                    SharedData.CharacterSpriteIdMap = new Dictionary<int, GameObject>
                    {
                        [0] = test_characterSprite
                    };

                    ReadOnlySharedPlayerSetting player1 = new ReadOnlySharedPlayerSetting(
                        0, "Trim", Color.blue, 0, new Dictionary<PlayerAction, KeyCode>
                        {
                            [PlayerAction.Left] = KeyCode.Keypad4,
                            [PlayerAction.Right] = KeyCode.Keypad6,
                            [PlayerAction.Up] = KeyCode.Keypad8,
                            [PlayerAction.Down] = KeyCode.Keypad5,
                            [PlayerAction.Action1] = KeyCode.Keypad7,
                            [PlayerAction.Action2] = KeyCode.Keypad9
                        }
                    );

                    ReadOnlySharedPlayerSetting player2 = new ReadOnlySharedPlayerSetting(
                        1, "Leung", Color.red, 0, new Dictionary<PlayerAction, KeyCode>
                        {
                            [PlayerAction.Left] = KeyCode.LeftArrow,
                            [PlayerAction.Right] = KeyCode.RightArrow,
                            [PlayerAction.Up] = KeyCode.UpArrow,
                            [PlayerAction.Down] = KeyCode.DownArrow,
                            [PlayerAction.Action1] = KeyCode.RightAlt,
                            [PlayerAction.Action2] = KeyCode.RightControl
                        }
                    );

                    return new IReadOnlySharedPlayerSetting[] { player1, player2 };
                }
                else
                {
                    return SharedData.playerSettings;
                }
            }
        }
        protected abstract int passPoints { get; }

        private void OnLastSceneStatePoped(SceneStateManager manager, ValueTuple<ISceneState, object> result)
        {
            SceneManager.LoadSceneAsync(SceneBuildIndex.setup);
        }
    }
}
