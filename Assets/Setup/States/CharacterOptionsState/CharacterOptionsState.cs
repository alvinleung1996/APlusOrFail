using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace APlusOrFail.Setup.SceneStates
{
    using Character;
    using Components;

    public class CharacterOptionsState : SceneStateBehavior<ValueTuple<ISetupData, IPlayerSetting>, Void>
    {
        public Canvas uiScene;
        public Button changeNameColorButton;
        public Button chooseCharacterButton;
        public Button remapActionKeyButton;
        public Button closeButton;
        public Button deletePlayerButton;

        public PlayerNameAndColorSetupState nameColorSetupUIScene;
        public CharacterSelectionState charSelectionUIScene;
        public PlayerActionKeySetupState actionKeySetupUIScene;

        private void Awake()
        {
            changeNameColorButton.onClick.AddListener(OnChangeNameColorButtonClicked);
            chooseCharacterButton.onClick.AddListener(OnChooseCharacterButtonClicked);
            remapActionKeyButton.onClick.AddListener(OnRemapActionKeyButtonClicked);
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            deletePlayerButton.onClick.AddListener(OnDeletePlayerButtonClicked);
            uiScene.gameObject.SetActive(false);
        }

        protected override Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            uiScene.gameObject.SetActive(true);
            AutoResizeCamera.instance.Trace(arg.Item2.character);
            return Task.CompletedTask;
        }

        protected override Task OnBlur()
        {
            uiScene.gameObject.SetActive(false);
            AutoResizeCamera.instance.UntraceAll();
            return Task.CompletedTask;
        }

        private void OnChangeNameColorButtonClicked()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                PushSceneState(nameColorSetupUIScene, arg);
            }
        }

        private void OnChooseCharacterButtonClicked()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                PushSceneState(charSelectionUIScene, arg);
            }
        }

        private void OnRemapActionKeyButtonClicked()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                PushSceneState(actionKeySetupUIScene, arg);
            }
        }

        private void OnCloseButtonClicked()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                PopSceneState(null);
            }
        }

        private void OnDeletePlayerButtonClicked()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                arg.Item2.character.GetComponent<CharacterPlayer>().playerSetting = null;
                arg.Item2.Free();
                arg.Item1.characterPlayerSettingMap[arg.Item2.character] = null;
                arg.Item1.UnmapAllActionFromKey(arg.Item2);
                PopSceneState(null);
            }
        }
    }
}
