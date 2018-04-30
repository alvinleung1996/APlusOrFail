using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace APlusOrFail.Setup.States.PlayerNameAndColorSetupState
{
    using Character;

    public class PlayerNameAndColorSetupState : SceneStateBehavior<Void, Void>
    {
        public RectTransform uiScene;
        public InputField nameInputField;
        public List<ColorButton> colorButtons;
        public Button enterButton;
        public Button cancelButton;
        
        public GameObject character { get; set; }
        public bool cancelled { get; private set; }

        private IPlayerSetting player;
        private Color color;
        
        private void Start()
        {
            enterButton.onClick.AddListener(OnEnterButtonClicked);
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
            foreach (ColorButton button in colorButtons)
            {
                button.onSelected += OnColorButtonSelected;
            }
            HideUI();
        }

        protected override Task OnLoad()
        {
            cancelled = false;
            player = (IPlayerSetting)character.GetComponent<CharacterPlayer>().player;
            color = player.color;
            nameInputField.text = player.name;
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

        protected override Task OnUnload()
        {
            player = null;
            return Task.CompletedTask;
        }

        private void OnColorButtonSelected(ColorButton button)
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                color = button.color;
            }
        }

        private void OnEnterButtonClicked()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                player.name = nameInputField.text;
                player.color = color;
                SceneStateManager.instance.Pop(this, null);
            }
        }

        private void OnCancelButtonClicked()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                cancelled = true;
                SceneStateManager.instance.Pop(this, null);
            }
        }

        private void HideUI()
        {
            uiScene.gameObject.SetActive(false);
        }

        private void ShowUI()
        {
            uiScene.gameObject.SetActive(true);
            nameInputField.Select();
        }
    }
}
