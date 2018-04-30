using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace APlusOrFail.Setup.States.CharacterSelectionState
{
    using Character;

    public class CharacterSelectionState : SceneStateBehavior<Void, Void>
    {
        public RectTransform uiScene;
        public Text messageText;
        public Button cancelButton;

        public GameObject originalCharacter { get; set; }

        public bool cancelled { get; private set; }
        public GameObject selectedCharacter { get; private set; }
        

        private void Start()
        {
            foreach (Selectable selectable in FindObjectsOfType<Selectable>())
            {
                selectable.OnSelected += OnCharactedSelected;
            }
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
            HideUI();
        }

        protected override Task OnLoad()
        {
            cancelled = false;
            selectedCharacter = null;
            return Task.CompletedTask;
        }

        protected override Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            ShowUI();
            return Task.CompletedTask;
        }

        protected override Task OnBlur()
        {
            HideUI();
            return Task.CompletedTask;
        }

        private void OnCancelButtonClicked()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                cancelled = true;
                SceneStateManager.instance.Pop(this, null);
            }
        }

        private void OnCharactedSelected(Selectable selectedChar)
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                CharacterPlayer selectedCharPlayer = selectedChar.GetComponent<CharacterPlayer>();
                if (selectedChar.gameObject != originalCharacter && selectedCharPlayer.player != null)
                {
                    messageText.text = "Character has been selected!";
                }
                else
                {
                    selectedCharacter = selectedChar.gameObject;
                    if (selectedChar.gameObject != originalCharacter)
                    {
                        CharacterPlayer originalCharPlayer = originalCharacter.GetComponent<CharacterPlayer>();
                        selectedCharPlayer.player = originalCharPlayer.player;
                        originalCharPlayer.player = null;
                        ((IPlayerSetting)selectedCharPlayer.player).characterSprite = selectedChar.GetComponent<CharacterSprite>().overrideCharacterSprite;
                    }
                    SceneStateManager.instance.Pop(this, null);
                }
                
            }
        }

        private void HideUI()
        {
            uiScene.gameObject.SetActive(false);
        }

        private void ShowUI()
        {
            uiScene.gameObject.SetActive(true);
        }
    }
}
