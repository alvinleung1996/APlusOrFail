using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace APlusOrFail.Setup.SceneStates
{
    using Character;
    using Components;

    public class CharacterSelectionState : SceneStateBehavior<ValueTuple<ISetupData, IPlayerSetting>, Void>
    {
        public Canvas uiScene;
        public Text messageText;
        public Button cancelButton;

        private Transform originalCharacter;
        

        private void Awake()
        {
            
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
            uiScene.gameObject.SetActive(false);
        }

        protected override Task OnLoad()
        {
            foreach (Transform selectable in arg.Item1.characterPlayerSettingMap.Keys)
            {
                selectable.GetComponent<Selectable>().onSelected += OnCharactedSelected;
            }
            return Task.CompletedTask;
        }

        protected override Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            uiScene.gameObject.SetActive(true);
            foreach (Transform character in arg.Item1.characterPlayerSettingMap.Keys) AutoResizeCamera.instance.Trace(character);
            originalCharacter = arg.Item2.character;
            return Task.CompletedTask;
        }

        protected override Task OnBlur()
        {
            uiScene.gameObject.SetActive(false);
            AutoResizeCamera.instance.UntraceAll();
            originalCharacter = null;
            return Task.CompletedTask;
        }

        protected override Task OnUnload()
        {
            foreach (Transform selectable in arg.Item1.characterPlayerSettingMap.Keys)
            {
                selectable.GetComponent<Selectable>().onSelected -= OnCharactedSelected;
            }
            return Task.CompletedTask;
        }

        private void OnCancelButtonClicked()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                PopSceneState(null);
            }
        }

        private void OnCharactedSelected(Selectable selectedChar)
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                ISetupData setupData = arg.Item1;
                IPlayerSetting playerSetting = arg.Item2;

                if (ReferenceEquals(selectedChar.transform, originalCharacter))
                {
                    PopSceneState(null);
                }
                else if (setupData.characterPlayerSettingMap[selectedChar.transform] == null)
                {
                    setupData.characterPlayerSettingMap[playerSetting.character] = null;
                    playerSetting.character.GetComponent<CharacterPlayer>().playerSetting = null;

                    setupData.characterPlayerSettingMap[selectedChar.transform] = playerSetting;
                    selectedChar.GetComponent<CharacterPlayer>().playerSetting = playerSetting;
                    playerSetting.character = selectedChar.transform;
                    playerSetting.characterSpriteId = selectedChar.GetComponent<CharacterSpriteId>().spriteId;

                    PopSceneState(null);
                }
                else
                {
                    messageText.text = "Character has been selected!";
                }
            }
        }
    }
}
