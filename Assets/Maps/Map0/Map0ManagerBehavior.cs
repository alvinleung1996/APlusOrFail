using System.Collections.Generic;
using UnityEngine;

namespace APlusOrFail.Maps
{
    using Components;
    using Objects;

    public class Map0ManagerBehavior : MapManagerBehavior
    {
        public List<ObjectPrefabInfo> usableObjects;
        public MapGridPlacer spawnArea;
        public GameObject test_characterSprite;

        protected override void Awake()
        {
            if (Player.players.Count == 0)
            {
                Player player = new Player
                {
                    characterSprite = test_characterSprite,
                    name = "Trim",
                    color = Color.blue
                };
                player.MapActionToKey(Player.Action.Left, KeyCode.Keypad4);
                player.MapActionToKey(Player.Action.Right, KeyCode.Keypad6);
                player.MapActionToKey(Player.Action.Up, KeyCode.Keypad8);
                player.MapActionToKey(Player.Action.Down, KeyCode.Keypad5);
                player.MapActionToKey(Player.Action.Action1, KeyCode.Keypad7);
                player.MapActionToKey(Player.Action.Action2, KeyCode.Keypad9);

                Player player2 = new Player
                {
                    characterSprite = test_characterSprite,
                    name = "Leung",
                    color = Color.red
                };
                player2.MapActionToKey(Player.Action.Left, KeyCode.LeftArrow);
                player2.MapActionToKey(Player.Action.Right, KeyCode.RightArrow);
                player2.MapActionToKey(Player.Action.Up, KeyCode.UpArrow);
                player2.MapActionToKey(Player.Action.Down, KeyCode.DownArrow);
                player2.MapActionToKey(Player.Action.Action1, KeyCode.RightAlt);
                player2.MapActionToKey(Player.Action.Action2, KeyCode.RightControl);
            }

            base.Awake();
        }

        protected override IEnumerable<IRoundSetting> GetRoundSettings()
        {
            return new IRoundSetting[]
            {
                new RoundSetting("Ask the professor", 50, usableObjects, spawnArea, defaultRoundScoreSetting, defaultRoundRankColorSetting),
                new RoundSetting("Capture the professor", 50, usableObjects, spawnArea, defaultRoundScoreSetting, defaultRoundRankColorSetting),
                new RoundSetting("Ask the professor", 50, usableObjects, spawnArea, defaultRoundScoreSetting, defaultRoundRankColorSetting),
                new RoundSetting("Capture the professor", 50, usableObjects, spawnArea, defaultRoundScoreSetting, defaultRoundRankColorSetting),
                new RoundSetting("Ask the professor", 50, usableObjects, spawnArea, defaultRoundScoreSetting, defaultRoundRankColorSetting)
            };
        }
    }
}
