using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace APlusOrFail
{
    public enum PlayerAction
    {
        Left,
        Right,
        Up,
        Down,
        Action1,
        Action2
    }

    public interface IReadOnlyPlayerSetting
    {
        int id { get; }
        string name { get; }
        Color color { get; }
        GameObject characterSprite { get; }
        IReadOnlyDictionary<PlayerAction, KeyCode> actionMap { get; }
    }

    public interface IPlayerSetting : IReadOnlyPlayerSetting
    {
        new string name { get; set; }
        new Color color { get; set; }
        new GameObject characterSprite { get; set; }
        new IDictionary<PlayerAction, KeyCode> actionMap { get; }
    }

    public static class PlayerSettingExtensions
    {
        public static bool HasKeyForAction(this IReadOnlyPlayerSetting playerSetting, PlayerAction action)
        {
            return playerSetting.actionMap.ContainsKey(action);
        }

        public static KeyCode GetKeyForAction(this IReadOnlyPlayerSetting playerSetting, PlayerAction action)
        {
            KeyCode key;
            return playerSetting.actionMap.TryGetValue(action, out key) ? key : KeyCode.None;
        }

        public static void MapActionToKey(this IPlayerSetting playerSetting, PlayerAction action, KeyCode key)
        {
            PlayerInputRegistry.RegisterKey(key, playerSetting);
            playerSetting.actionMap[action] = key;
        }

        public static void UnmapActionFromKey(this IPlayerSetting playerSetting, PlayerAction action)
        {
            KeyCode key;
            if (playerSetting.actionMap.TryGetValue(action, out key))
            {
                PlayerInputRegistry.UnregisterKey(key, playerSetting);
                playerSetting.actionMap.Remove(action);
            }

        }

        public static void UnmapAllActionFromKey(this IPlayerSetting playerSetting)
        {
            foreach (KeyCode key in playerSetting.actionMap.Values)
            {
                PlayerInputRegistry.UnregisterKey(key, playerSetting);
            }
            playerSetting.actionMap.Clear();
        }
    }


    public class PlayerSetting : IPlayerSetting
    {
        private static int playerAutoId = 1;
        private static readonly List<PlayerSetting> playerList = new List<PlayerSetting>();
        public static readonly ReadOnlyCollection<PlayerSetting> players = new ReadOnlyCollection<PlayerSetting>(playerList);


        public int id { get; set; }

        private string _name;
        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                //onNameChanged?.Invoke(this, value);
            }
        }
        //public event EventHandler<PlayerSetting, string> onNameChanged;

        private Color _color = Color.white;
        public Color color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                //onColorChanged?.Invoke(this, value);
            }
        }
        //public event EventHandler<PlayerSetting, Color> onColorChanged;

        private GameObject _characterSprite;
        public GameObject characterSprite
        {
            get
            {
                return _characterSprite;
            }
            set
            {
                _characterSprite = value;
                //onCharacterSpriteChanged?.Invoke(this, value);
            }
        }
        //public event EventHandler<PlayerSetting, GameObject> onCharacterSpriteChanged;
        
        public IDictionary<PlayerAction, KeyCode> actionMap { get; } = new Dictionary<PlayerAction, KeyCode>();
        IReadOnlyDictionary<PlayerAction, KeyCode> IReadOnlyPlayerSetting.actionMap => readonlyActionMap;
        private readonly IReadOnlyDictionary<PlayerAction, KeyCode> readonlyActionMap;

        public PlayerSetting()
        {
            readonlyActionMap = new ReadOnlyDictionary<PlayerAction, KeyCode>(actionMap);

            id = playerAutoId++;
            name = $"Player {id}";
            playerList.Add(this);
        }
        
        //public void Delete()
        //{
        //    playerList.Remove(this);
        //    this.UnmapAllActionFromKey();
        //    onDelete?.Invoke(this);
        //}
        //public event EventHandler<IReadOnlyPlayerSetting> onDelete;
    }
}
