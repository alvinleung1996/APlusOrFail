using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.Linq;

namespace APlusOrFail.Setup
{
    using SceneStates;

    public class SetupManagerBehavior : MonoBehaviour, ISetupManager
    {
        public SceneStateManager sceneStateManager;
        public DefaultState defaultSceneState;
        public List<Transform> characters;
        public List<GameObject> characterSprites; // index is id

        private ISetupData setupData;

        private void Awake()
        {
            if (!this.Register())
            {
                Destroy(this);
            }
            if (SharedData.CharacterSpriteIdMap == null)
            {
                SharedData.CharacterSpriteIdMap = characterSprites
                    .Select((s, i) => new ValueTuple<int, GameObject>(i, s))
                    .ToDictionary(t => t.Item1, t => t.Item2);
            }
        }

        private void Start()
        {
            sceneStateManager.onLastSceneStatePoped += OnLastSceneStatePoped;
            setupData = new SetupData(characters, SharedData.playerSettings ?? Enumerable.Empty<IReadOnlySharedPlayerSetting>(),
                defaultSceneState.canvas.GetComponent<RectTransform>(), defaultSceneState.nameTagPrefab);
            
            sceneStateManager.Push(defaultSceneState, setupData);
        }

        private void OnDestroy()
        {
            this.Unregister();
        }

        public void OnLastSceneStatePoped(SceneStateManager sceneStateManager, ValueTuple<ISceneState, object> result)
        {
            SharedData.playerSettings = setupData.characterPlayerSettingMap.Values
                .Where(v => v != null)
                .Select(v => new ReadOnlySharedPlayerSetting(v))
                .ToList();
            SceneManager.LoadSceneAsync((int)result.Item2);
        }
    }

    public interface ISetupManager
    {

    }

    public static class SetupManager
    {
        public static ISetupManager instance { get; private set; }

        public static bool Register(this ISetupManager setupManager)
        {
            if (instance == null)
            {
                instance = setupManager;
                return true;
            }
            return false;
        }

        public static void Unregister(this ISetupManager setupManager)
        {
            if (instance == setupManager)
            {
                instance = null;
            }
        }
    }
}
