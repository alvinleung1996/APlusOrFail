using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace APlusOrFail.Setup.States.PlayerActionKeySetupState
{
    using Character;

    public class PlayerActionKeySetupState : SceneStateBehavior<Void, Void>
    {
        private static readonly ReadOnlyCollection<PlayerAction> actionSequence = new ReadOnlyCollection<PlayerAction>(new PlayerAction[]{
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


        public RectTransform uiScene;
        public Text enterKeyMessageText;
        public Button cancelButton;

        public GameObject character { get; set; }
        public bool cancelled { get; private set; }

        private CharacterPlayer charPlayer;
        private Dictionary<PlayerAction, KeyCode> actionKeyMap;
        private int setupingActionIndex = 0;


        private void Start()
        {
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
            HideUI();
        }

        protected override Task OnLoad()
        {
            charPlayer = character.GetComponent<CharacterPlayer>();
            actionKeyMap = new Dictionary<PlayerAction, KeyCode>();
            setupingActionIndex = 0;

            cancelled = false;

            return Task.CompletedTask;
        }

        protected override Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            ShowUI();
            enterKeyMessageText.text = $"Key for {TextForAction(actionSequence[setupingActionIndex])}";
            return Task.CompletedTask;
        }

        protected override Task OnBlur()
        {
            HideUI();
            return Task.CompletedTask;
        }

        protected override Task OnUnload()
        {
            charPlayer = null;
            actionKeyMap = null;
            return Task.CompletedTask;
        }

        private void Update()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                KeyCode? key = KeyDetector.GetKeyDowned();
                if (key != null)
                {
                    OnKeyDown(key.Value);
                }
            }
        }

        private void OnKeyDown(KeyCode key)
        {
            if (PlayerInputRegistry.HasRegisteredByOther(key, charPlayer.player))
            {
                enterKeyMessageText.text = "The key has already used by other player!";
            }
            else if (actionKeyMap.ContainsValue(key))
            {
                enterKeyMessageText.text = "The key is used for other action!";
            }
            else
            {
                actionKeyMap[actionSequence[setupingActionIndex]] = key;
                ++setupingActionIndex;
                if (setupingActionIndex < actionSequence.Count)
                {
                    enterKeyMessageText.text = $"Key for {TextForAction(actionSequence[setupingActionIndex])}";
                }
                else
                {
                    ((IPlayerSetting)charPlayer.player).UnmapAllActionFromKey();
                    foreach (KeyValuePair<PlayerAction, KeyCode> pair in actionKeyMap)
                    {
                        ((IPlayerSetting)charPlayer.player).MapActionToKey(pair.Key, pair.Value);
                    }
                    SceneStateManager.instance.Pop(this, null);
                }
            }
        }

        private void OnCancelButtonClicked()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                SceneStateManager.instance.Pop(this, null);
                cancelled = true;
            }
        }

        private void ShowUI()
        {
            uiScene.gameObject.SetActive(true);
        }

        private void HideUI()
        {
            uiScene.gameObject.SetActive(false);
        }
    }
}
