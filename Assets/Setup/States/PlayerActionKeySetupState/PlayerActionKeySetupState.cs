using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;

namespace APlusOrFail.Setup.SceneStates
{
    using Components;

    public class PlayerActionKeySetupState : SceneStateBehavior<ValueTuple<ISetupData, IPlayerSetting>, ValueTuple<IPlayerSetting, bool>>
    {
        private static readonly ReadOnlyCollection<PlayerAction> actionSequence = new ReadOnlyCollection<PlayerAction>(new PlayerAction[] {
            PlayerAction.Left,
            PlayerAction.Right,
            PlayerAction.Up,
            PlayerAction.Down,
            PlayerAction.Action1,
            PlayerAction.Action2
        });

        private static string TextForAction(PlayerAction action)
        {
            switch (action)
            {
                case PlayerAction.Left: return "left";
                case PlayerAction.Right: return "right";
                case PlayerAction.Up: return "jump";
                case PlayerAction.Down: return "squat";
                case PlayerAction.Action1: return "action1";
                case PlayerAction.Action2: return "action2";
                default: return "";
            }
        }


        public Canvas uiScene;
        public Text enterKeyMessageText;
        public Button cancelButton;

        private readonly Dictionary<PlayerAction, KeyCode> originalActionMap = new Dictionary<PlayerAction, KeyCode>();
        private Coroutine runningCoroutine;

        private void Awake()
        {
            uiScene.gameObject.SetActive(false);
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }

        protected override Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            uiScene.gameObject.SetActive(true);
            AutoResizeCamera.instance.Trace(arg.Item2.character);
            foreach (var pair in arg.Item2.actionMap) originalActionMap.Add(pair.Key, pair.Value);
            runningCoroutine = StartCoroutine(RegisterKeyCoroutine());
            return Task.CompletedTask;
        }

        protected override Task OnBlur()
        {
            uiScene.gameObject.SetActive(false);
            AutoResizeCamera.instance.UntraceAll();
            originalActionMap.Clear();
            return Task.CompletedTask;
        }

        private IEnumerator RegisterKeyCoroutine()
        {
            ISetupData setupData = arg.Item1;
            IPlayerSetting playerSetting = arg.Item2;
            
            arg.Item1.UnmapAllActionFromKey(arg.Item2);

            for (int i = 0; i < actionSequence.Count; ++i)
            {
                enterKeyMessageText.text = $"Key for {TextForAction(actionSequence[i])}";

                KeyCode key;
                while (true)
                {
                    yield return null;
                    key = KeyDetector.GetKeyDowned();
                    if (key != KeyCode.None)
                    {
                        IPlayerSetting registeredPlayerSetting;
                        if (setupData.keyPlayerMap.TryGetValue(key, out registeredPlayerSetting))
                        {
                            if (registeredPlayerSetting == playerSetting)
                            {
                                enterKeyMessageText.text = "The key is used for other action!";
                            }
                            else
                            {
                                enterKeyMessageText.text = "The key has already used by other player!";
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                setupData.MapActionToKey(playerSetting, actionSequence[i], key);
            }

            PopSceneState(new ValueTuple<IPlayerSetting, bool>(playerSetting, true));
        }

        private void OnCancelButtonClicked()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                PopSceneState(new ValueTuple<IPlayerSetting, bool>(arg.Item2, false));
                arg.Item1.UnmapAllActionFromKey(arg.Item2);
                foreach (var pair in originalActionMap) arg.Item1.MapActionToKey(arg.Item2, pair.Key, pair.Value);
                StopCoroutine(runningCoroutine);
                runningCoroutine = null;
            }
        }
    }
}
