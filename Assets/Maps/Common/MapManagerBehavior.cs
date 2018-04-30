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
            public MapArea mapArea { get; }
            public AutoResizeCamera camera { get; }
            public IReadOnlyList<IRoundSetting> roundSettings { get; }
            public int minRoundCount { get; }
            public IReadOnlyList<IReadOnlyPlayerSetting> playerSettings { get; }
            public int passPoints { get; }

            public MapSetting(string name, MapArea mapArea, AutoResizeCamera camera,
                IEnumerable<IRoundSetting> roundSettings, int minRoundCount,
                IEnumerable<IReadOnlyPlayerSetting> playerSettings, int passPoints)
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
            FindObjectOfType<SceneStateManager>().Push(defaultSceneState, stat);
        }

        protected virtual void OnDestroy()
        {
            ((IMapManager)this).Unregister();
        }

        protected virtual IMapSetting mapSetting => new MapSetting(mapName, mapArea, camera, roundSettings, minRoundCount, playerSettings, passPoints);
        protected abstract IEnumerable<IRoundSetting> roundSettings { get; }
        protected abstract int minRoundCount { get; }
        protected virtual IEnumerable<IReadOnlyPlayerSetting> playerSettings
        {
            get
            {
                if (PlayerSetting.players.Count == 0)
                {
                    PlayerSetting player1 = new PlayerSetting
                    {
                        characterSprite = test_characterSprite,
                        name = "Trim",
                        color = Color.blue
                    };
                    player1.MapActionToKey(PlayerAction.Left, KeyCode.Keypad4);
                    player1.MapActionToKey(PlayerAction.Right, KeyCode.Keypad6);
                    player1.MapActionToKey(PlayerAction.Up, KeyCode.Keypad8);
                    player1.MapActionToKey(PlayerAction.Down, KeyCode.Keypad5);
                    player1.MapActionToKey(PlayerAction.Action1, KeyCode.Keypad7);
                    player1.MapActionToKey(PlayerAction.Action2, KeyCode.Keypad9);

                    PlayerSetting player2 = new PlayerSetting
                    {
                        characterSprite = test_characterSprite,
                        name = "Leung",
                        color = Color.red
                    };
                    player2.MapActionToKey(PlayerAction.Left, KeyCode.LeftArrow);
                    player2.MapActionToKey(PlayerAction.Right, KeyCode.RightArrow);
                    player2.MapActionToKey(PlayerAction.Up, KeyCode.UpArrow);
                    player2.MapActionToKey(PlayerAction.Down, KeyCode.DownArrow);
                    player2.MapActionToKey(PlayerAction.Action1, KeyCode.RightAlt);
                    player2.MapActionToKey(PlayerAction.Action2, KeyCode.RightControl);

                    return new IPlayerSetting[] { player1, player2 };
                }
                else
                {
                    return PlayerSetting.players;
                }
            }
        }
        protected abstract int passPoints { get; }
    }
}
