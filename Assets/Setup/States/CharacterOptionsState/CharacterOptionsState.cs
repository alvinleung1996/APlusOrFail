using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace APlusOrFail.Setup.States.CharacterOptionState
{
    using Character;
    using PlayerNameAndColorSetupState;
    using CharacterSelectionState;
    using PlayerActionKeySetupState;

    public class CharacterOptionsState : SceneStateBehavior<Void, Void>
    {
        public RectTransform uiScene;
        public Button changeNameColorButton;
        public Button chooseCharacterButton;
        public Button remapActionKeyButton;
        public Button closeButton;
        public Button deletePlayerButton;

        public PlayerNameAndColorSetupState nameColorSetupUIScene;
        public CharacterSelectionState charSelectionUIScene;
        public PlayerActionKeySetupState actionKeySetupUIScene;

        public GameObject character { get; set; }

        private CharacterSelectionState activeCharSelectionUIScene;

        private void Start()
        {
            changeNameColorButton.onClick.AddListener(OnChangeNameColorButtonClicked);
            chooseCharacterButton.onClick.AddListener(OnChooseCharacterButtonClicked);
            remapActionKeyButton.onClick.AddListener(OnRemapActionKeyButtonClicked);
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            deletePlayerButton.onClick.AddListener(OnDeletePlayerButtonClicked);

            HideUI();
        }

        protected override Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            ShowUI();
            if (activeCharSelectionUIScene != null)
            {
                if (activeCharSelectionUIScene.selectedCharacter != character)
                {
                    character = activeCharSelectionUIScene.selectedCharacter;
                }
                activeCharSelectionUIScene = null;
            }
            return Task.CompletedTask;
        }

        protected override Task OnBlur()
        {
            HideUI();
            return Task.CompletedTask;
        }

        private void OnChangeNameColorButtonClicked()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                SceneStateManager.instance.Push(nameColorSetupUIScene, null);
                nameColorSetupUIScene.character = character;
            }
        }

        private void OnChooseCharacterButtonClicked()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                SceneStateManager.instance.Push(charSelectionUIScene, null);
                activeCharSelectionUIScene = charSelectionUIScene;
                activeCharSelectionUIScene.originalCharacter = character;
            }
        }

        private void OnRemapActionKeyButtonClicked()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                SceneStateManager.instance.Push(actionKeySetupUIScene, null);
                actionKeySetupUIScene.character = character;
            }
        }

        private void OnCloseButtonClicked()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                SceneStateManager.instance.Pop(this, null);
            }
        }

        private void OnDeletePlayerButtonClicked()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                CharacterPlayer charPlayer = character.GetComponent<CharacterPlayer>();
                //TODO: delete player;
                charPlayer.player = null;
                SceneStateManager.instance.Pop(this, null);
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
