using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace APlusOrFail.Setup.SceneStates
{
    using Components;

    public class PlayerNameAndColorSetupState : SceneStateBehavior<ValueTuple<ISetupData, IPlayerSetting>, ValueTuple<IPlayerSetting, bool>>
    {
        public Canvas uiScene;
        public InputField nameInputField;
        public List<ColorButton> colorButtons;
        public Button enterButton;
        public Button cancelButton;

        private string originalName;
        private Color originalColor;
        
        private void Start()
        {
            enterButton.onClick.AddListener(OnEnterButtonClicked);
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
            foreach (ColorButton button in colorButtons)
            {
                button.onSelected += OnColorButtonSelected;
            }
            uiScene.gameObject.SetActive(false);
        }

        protected override Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            uiScene.gameObject.SetActive(true);
            AutoResizeCamera.instance.Trace(arg.Item2.character);
            originalName = arg.Item2.name;
            originalColor = arg.Item2.color;
            nameInputField.text = originalName;
            nameInputField.Select();
            return Task.CompletedTask;
        }

        protected override Task OnBlur()
        {
            uiScene.gameObject.SetActive(false);
            AutoResizeCamera.instance.UntraceAll();
            return Task.CompletedTask;
        }

        private void OnColorButtonSelected(ColorButton button)
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                arg.Item2.color = button.color;
            }
        }

        private void OnEnterButtonClicked()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                arg.Item2.name = nameInputField.text;
                arg.Item2.ApplyProperties();
                PopSceneState(new ValueTuple<IPlayerSetting, bool>(arg.Item2, true));
            }
        }

        private void OnCancelButtonClicked()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                arg.Item2.name = originalName;
                arg.Item2.color = originalColor;
                arg.Item2.ApplyProperties();
                PopSceneState(new ValueTuple<IPlayerSetting, bool>(arg.Item2, false));
            }
        }
    }
}
