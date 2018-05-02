using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

    public interface IReadOnlySharedPlayerSetting
    {
        int id { get; }
        string name { get; }
        Color color { get; }
        int characterSpriteId { get; }
        IReadOnlyDictionary<PlayerAction, KeyCode> actionMap { get; }
    }

    public static class PlayerSettingExtensions
    {
        public static bool HasKeyForAction(this IReadOnlySharedPlayerSetting playerSetting, PlayerAction action)
        {
            return playerSetting.actionMap.ContainsKey(action);
        }

        public static KeyCode GetKeyForAction(this IReadOnlySharedPlayerSetting playerSetting, PlayerAction action)
        {
            KeyCode key;
            return playerSetting.actionMap.TryGetValue(action, out key) ? key : KeyCode.None;
        }
    }
    

    public class ReadOnlySharedPlayerSetting : IReadOnlySharedPlayerSetting
    {
        public int id { get; }
        public string name { get; }
        public Color color { get; }
        public int characterSpriteId { get; }
        public IReadOnlyDictionary<PlayerAction, KeyCode> actionMap { get; }

        public ReadOnlySharedPlayerSetting(IReadOnlySharedPlayerSetting playerSetting)
            : this(playerSetting.id, playerSetting.name, playerSetting.color, playerSetting.characterSpriteId, playerSetting.actionMap) { }

        public ReadOnlySharedPlayerSetting(int id, string name, Color color, int characterId,
            IReadOnlyDictionary<PlayerAction, KeyCode> actionMap)
        {
            this.id = id;
            this.name = name;
            this.color = color;
            this.characterSpriteId = characterId;
            this.actionMap = actionMap.ToDictionary(p => p.Key, p => p.Value);
        }
    }
}
