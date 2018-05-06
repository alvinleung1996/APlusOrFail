using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace APlusOrFail.Setup.SceneStates
{
    using Components;
    using Character;

    public class DefaultState : SceneStateBehavior<ISetupData, int>, IMapSelectionRegistry
    {
        private class MapSelectionHandler : IMapSelectionHandler
        {
            public int sceneIndex { get; set; }
            public int priority { get; set; }

            public MapSelectionHandler(int sceneIndex, int priority)
            {
                this.sceneIndex = sceneIndex;
                this.priority = priority;
            }
        }


        public Canvas canvas;
        public NameTag nameTagPrefab;
        public RectTransform counterPanel;
        public Text counterText;

        public PlayerNameAndColorSetupState inputPlayerNameUIScene;
        public PlayerActionKeySetupState actionKeySetupUIScene;
        public CharacterOptionsState charOptionUIScene;

        public float countDownTime = 5;

        private readonly List<MapSelectionHandler> selectionHandlers = new List<MapSelectionHandler>();
        private MapSelectionHandler selectedHandler;
        private float remainingTime;


        private void Awake()
        {
            if (!this.Register())
            {
                Destroy(this);
                return;
            }
            canvas.gameObject.SetActive(false);
            counterPanel.gameObject.SetActive(false);
            RecomputeSelectedHandlers();
        }

        private void OnDestroy()
        {
            this.Unregister();
        }
        
        public override Task OnLoad(SceneStateManager sceneStateManager, ISetupData arg)
        {
            Task task = base.OnLoad(sceneStateManager, arg);
            foreach (Transform selectable in arg.characterPlayerSettingMap.Keys)
            {
                selectable.GetComponent<Selectable>().onSelected += OnCharacterSelected;
            }
            canvas.gameObject.SetActive(true);
            return task;
        }

        private void OnCharacterSelected(Selectable selectedChar)
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                IPlayerSetting playerSetting;
                if ((playerSetting = arg.characterPlayerSettingMap[selectedChar.transform]) != null)
                {
                    PushSceneState(charOptionUIScene, new ValueTuple<ISetupData, IPlayerSetting>(arg, playerSetting));
                }
                else
                {
                    playerSetting = new PlayerSetting(
                        selectedChar.GetComponent<CharacterSpriteId>().spriteId,
                        selectedChar.transform,
                        canvas.GetComponent<RectTransform>(), nameTagPrefab
                    );
                    arg.characterPlayerSettingMap[selectedChar.transform] = playerSetting;
                    selectedChar.GetComponent<CharacterPlayer>().playerSetting = playerSetting;
                    PushSceneState(inputPlayerNameUIScene, new ValueTuple<ISetupData, IPlayerSetting>(arg, playerSetting));
                }
            }
        }

        public override Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            Task task = base.OnFocus(unloadedSceneState, result);
            foreach (Transform character in arg.characterPlayerSettingMap.Keys)
            {
                AutoResizeCamera.instance.Trace(character);
            }

            RecomputeSelectedHandlers();

            Type unloadedType = unloadedSceneState?.GetType();
            if (unloadedType == typeof(PlayerNameAndColorSetupState))
            {
                ValueTuple<IPlayerSetting, bool> r = (ValueTuple<IPlayerSetting, bool>)result;
                IPlayerSetting playerSetting = r.Item1;
                bool success = r.Item2;
                
                if (success)
                {
                    PushSceneState(actionKeySetupUIScene, new ValueTuple<ISetupData, IPlayerSetting>(arg, playerSetting));
                }
                else
                {
                    playerSetting.character.GetComponent<CharacterPlayer>().playerSetting = null;
                    playerSetting.Free();
                    arg.characterPlayerSettingMap[playerSetting.character.transform] = null;
                    arg.UnmapAllActionFromKey(playerSetting);
                }
            }
            else if (unloadedType == typeof(PlayerActionKeySetupState))
            {
                ValueTuple<IPlayerSetting, bool> r = (ValueTuple<IPlayerSetting, bool>)result;
                IPlayerSetting playerSetting = r.Item1;
                bool success = r.Item2;
                
                if (!success)
                {
                    playerSetting.character.GetComponent<CharacterPlayer>().playerSetting = null;
                    playerSetting.Free();
                    arg.characterPlayerSettingMap[playerSetting.character.transform] = null;
                    arg.UnmapAllActionFromKey(playerSetting);
                }
            }
            return task;
        }

        private void Update()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                if (selectedHandler != null)
                {
                    remainingTime -= Time.deltaTime;
                    if (remainingTime > 0)
                    {
                        counterText.text = $"{Mathf.CeilToInt(remainingTime)}";
                    }
                    else
                    {
                        PopSceneState(selectedHandler.sceneIndex);
                        selectedHandler = null;
                        selectionHandlers.Clear();
                    }
                }
            }
        }

        public override Task OnBlur()
        {
            Task task = base.OnBlur();
            AutoResizeCamera.instance.UntraceAll();
            RecomputeSelectedHandlers(false);
            return task;
        }

        public override Task OnUnload()
        {
            Task task = base.OnUnload();
            canvas.gameObject.SetActive(false);
            foreach (Transform selectable in arg.characterPlayerSettingMap.Keys)
            {
                selectable.GetComponent<Selectable>().onSelected -= OnCharacterSelected;
            }
            return task;
        }


        public IMapSelectionHandler Schedule(int sceneIndex, int priority)
        {
            MapSelectionHandler handler = new MapSelectionHandler(sceneIndex, priority);
            selectionHandlers.Add(handler);
            RecomputeSelectedHandlers();
            return handler;
        }

        public void UpdateSchedule(IMapSelectionHandler handler, int sceneIndex, int priority)
        {
            MapSelectionHandler h = handler as MapSelectionHandler;
            if (h != null && selectionHandlers.Contains(h) && (h.sceneIndex != sceneIndex || h.priority != priority))
            {
                h.sceneIndex = sceneIndex;
                h.priority = priority;
                RecomputeSelectedHandlers();
            }
        }

        public void Unschedule(IMapSelectionHandler handler)
        {
            MapSelectionHandler h = handler as MapSelectionHandler;
            if (h != null && selectionHandlers.Remove(h))
            {
                RecomputeSelectedHandlers();
            }
        }

        private void RecomputeSelectedHandlers(bool enable = true)
        {
            MapSelectionHandler nextHandler = null;
            if (enable && phase.IsAtLeast(SceneStatePhase.Focused))
            {
                foreach (MapSelectionHandler handler in selectionHandlers)
                {
                    if (nextHandler == null || nextHandler.priority < handler.priority)
                    {
                        nextHandler = handler;
                    }
                    else if (nextHandler != null && nextHandler.priority == handler.priority)
                    {
                        nextHandler = null;
                        break;
                    }
                }
            }
            if (nextHandler != selectedHandler)
            {
                counterPanel.gameObject.SetActive(nextHandler != null);
                selectedHandler = nextHandler;
                remainingTime = countDownTime;
            }
        }
    }
}
