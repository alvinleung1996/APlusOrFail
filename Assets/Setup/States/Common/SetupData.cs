using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace APlusOrFail.Setup
{
    using Components;
    using Character;

    public interface IReadOnlySetupData
    {
        IReadOnlyDictionary<KeyCode, IReadOnlyPlayerSetting> keyPlayerMap { get; }
        IReadOnlyDictionary<Transform, IReadOnlyPlayerSetting> characterPlayerSettingMap { get; }
    }

    public interface ISetupData : IReadOnlySetupData
    {
        new IDictionary<KeyCode, IPlayerSetting> keyPlayerMap { get; }
        new IDictionary<Transform, IPlayerSetting> characterPlayerSettingMap { get; }
    }


    public interface IReadOnlyPlayerSetting : IReadOnlySharedPlayerSetting
    {
        Transform character { get; }
    }

    public interface IPlayerSetting : IReadOnlyPlayerSetting
    {
        new string name { get; set; }
        new Color color { get; set; }
        new IDictionary<PlayerAction, KeyCode> actionMap { get; }
        new int characterSpriteId { get; set; }
        new Transform character { get; set; }
        void ApplyProperties();
        void Free();
    }


    public static class SetupDataExtensions
    {
        public static void MapActionToKey(this ISetupData setupData, IPlayerSetting playerSetting, PlayerAction action, KeyCode key)
        {
            if (!setupData.keyPlayerMap.ContainsKey(key))
            {
                setupData.keyPlayerMap[key] = playerSetting;
                playerSetting.actionMap[action] = key;
            }
            else
            {
                throw new InvalidOperationException($"key {Enum.GetName(typeof(KeyCode), key)} has already been registered");
            }
        }

        public static void UnmapActionFromKey(this ISetupData setupData, IPlayerSetting playerSetting, PlayerAction action)
        {
            KeyCode key;
            if (playerSetting.actionMap.TryGetValue(action, out key))
            {
                playerSetting.actionMap.Remove(action);
                setupData.keyPlayerMap.Remove(key);
            }
        }

        public static void UnmapAllActionFromKey(this ISetupData setupData, IPlayerSetting playerSetting)
        {
            foreach (KeyCode key in playerSetting.actionMap.Values)
            {
                setupData.keyPlayerMap.Remove(key);
            }
            playerSetting.actionMap.Clear();
        }
    }


    public class SetupData : ISetupData
    {
        public IDictionary<KeyCode, IPlayerSetting> keyPlayerMap { get; } = new Dictionary<KeyCode, IPlayerSetting>();
        IReadOnlyDictionary<KeyCode, IReadOnlyPlayerSetting> IReadOnlySetupData.keyPlayerMap => readonlyKeyPlayerMap;
        private readonly IReadOnlyDictionary<KeyCode, IReadOnlyPlayerSetting> readonlyKeyPlayerMap;

        public IDictionary<Transform, IPlayerSetting> characterPlayerSettingMap { get; }
        IReadOnlyDictionary<Transform, IReadOnlyPlayerSetting> IReadOnlySetupData.characterPlayerSettingMap => readonlyCharacterPlayerSettingMap;
        private readonly IReadOnlyDictionary<Transform, IReadOnlyPlayerSetting> readonlyCharacterPlayerSettingMap;

        public SetupData(IEnumerable<Transform> characters, IEnumerable<IReadOnlySharedPlayerSetting> playerSettings,
            RectTransform canvas, NameTag nameTagPrefab)
        {
            readonlyKeyPlayerMap = new ReadOnlyCovariantDictionary<KeyCode, IReadOnlyPlayerSetting, IPlayerSetting>(
                new ReadOnlyDictionary<KeyCode, IPlayerSetting>(keyPlayerMap));

            characterPlayerSettingMap = characters.ToDictionary(
                s => s, 
                s => playerSettings
                    .Where(ps => ps.characterSpriteId == s.GetComponent<CharacterSpriteId>().spriteId)
                    .Select<IReadOnlySharedPlayerSetting, IPlayerSetting>(ps => new PlayerSetting(ps, s, canvas, nameTagPrefab))
                    .FirstOrDefault()
            );
            readonlyCharacterPlayerSettingMap = new ReadOnlyCovariantDictionary<Transform, IReadOnlyPlayerSetting, IPlayerSetting>(
                new ReadOnlyDictionary<Transform, IPlayerSetting>(characterPlayerSettingMap));
            
            foreach (KeyValuePair<Transform, IPlayerSetting> pair in characterPlayerSettingMap.Where(p => p.Value != null))
            {
                pair.Key.GetComponent<CharacterPlayer>().playerSetting = pair.Value;
                foreach (KeyValuePair<PlayerAction, KeyCode> p in pair.Value.actionMap)
                {
                    keyPlayerMap.Add(p.Value, pair.Value);
                }
            }
        }
    }

    public class PlayerSetting : IPlayerSetting
    {
        private static int autoId = 0;
        private static readonly Dictionary<PlayerAction, KeyCode> emptyActionMap = new Dictionary<PlayerAction, KeyCode>();


        public int id { get; }

        private string _name;
        public string name { get { return _name; } set { SetProperty(ref _name, value); } }

        private Color _color;
        public Color color { get { return _color; } set { SetProperty(ref _color, value); } }

        public int characterSpriteId { get; set; }

        public IDictionary<PlayerAction, KeyCode> actionMap { get; } = new Dictionary<PlayerAction, KeyCode>();
        IReadOnlyDictionary<PlayerAction, KeyCode> IReadOnlySharedPlayerSetting.actionMap => readonlyActionMap;
        private IReadOnlyDictionary<PlayerAction, KeyCode> readonlyActionMap;

        private Transform _character;
        public Transform character { get { return _character; } set { SetProperty(ref _character, value); } }

        private readonly NameTag nameTag;

        public PlayerSetting(int characterSpriteId, Transform character, RectTransform canvas, NameTag nameTagPrefab)
            : this(autoId++, $"Player {autoId - 1}", Color.black, characterSpriteId, emptyActionMap, character, canvas, nameTagPrefab) { }

        public PlayerSetting(IReadOnlySharedPlayerSetting sharedPlayerSetting, Transform character, RectTransform canvas, NameTag nameTagPrefab)
            : this(sharedPlayerSetting.id, sharedPlayerSetting.name, sharedPlayerSetting.color, sharedPlayerSetting.characterSpriteId,
                   sharedPlayerSetting.actionMap, character, canvas, nameTagPrefab) { }

        private PlayerSetting(int id, string name, Color color, int characterSpriteId,
            IReadOnlyDictionary<PlayerAction, KeyCode> actionMap, Transform character,
            RectTransform canvas, NameTag nameTagPrefab)
        {
            this.id = id;
            _name = name;
            _color = color;
            this.characterSpriteId = characterSpriteId;
            this.actionMap = actionMap.ToDictionary(p => p.Key, p => p.Value);
            readonlyActionMap = new ReadOnlyDictionary<PlayerAction, KeyCode>(this.actionMap);

            _character = character;

            nameTag = UnityEngine.Object.Instantiate(nameTagPrefab, canvas);
            nameTag.camera = AutoResizeCamera.instance.GetComponent<Camera>();
            nameTag.canvasRectTransform = canvas;
            nameTag.targetTransform = character;
            nameTag.playerSetting = this;
        }


        private void SetProperty<T>(ref T property, T value)
        {
            if (!Equals(property, value))
            {
                property = value;
                ApplyProperties();
            }
        }

        public void ApplyProperties()
        {
            nameTag.targetTransform = character;
            nameTag.ApplyProperties();
        }

        public void Free()
        {
            UnityEngine.Object.Destroy(nameTag.gameObject);
        }
    }

    public class ReadOnlyCovariantDictionary<TKey, TValue, TBackValue> : IReadOnlyDictionary<TKey, TValue> where TBackValue : TValue
    {
        private class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly IEnumerator<KeyValuePair<TKey, TBackValue>> backEnumerator;

            public Enumerator(IEnumerator<KeyValuePair<TKey, TBackValue>> backEnumerator)
            {
                this.backEnumerator = backEnumerator;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    KeyValuePair<TKey, TBackValue> pair = backEnumerator.Current;
                    return new KeyValuePair<TKey, TValue>(pair.Key, pair.Value);
                }
            }
            object IEnumerator.Current => ((IEnumerator)backEnumerator).Current;
            public void Dispose() => backEnumerator.Dispose();
            public bool MoveNext() => backEnumerator.MoveNext();
            public void Reset() => backEnumerator.Reset();
        }

        private readonly IReadOnlyDictionary<TKey, TBackValue> backDict;

        public ReadOnlyCovariantDictionary(IReadOnlyDictionary<TKey, TBackValue> backDict)
        {
            this.backDict = backDict;
        }

        public TValue this[TKey key] => backDict[key];
        public IEnumerable<TKey> Keys => backDict.Keys;
        public IEnumerable<TValue> Values => (IEnumerable<TValue>)backDict.Values;
        public int Count => backDict.Count;
        public bool ContainsKey(TKey key) => backDict.ContainsKey(key);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new Enumerator(backDict.GetEnumerator());
        public bool TryGetValue(TKey key, out TValue value)
        {
            TBackValue backValue;
            bool result = backDict.TryGetValue(key, out backValue);
            value = backValue;
            return result;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
