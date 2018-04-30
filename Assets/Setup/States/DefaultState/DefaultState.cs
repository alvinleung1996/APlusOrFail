using System.Threading.Tasks;

namespace APlusOrFail.Setup.States.DefaultSceneState
{
    using Character;
    using CharacterOptionState;
    using PlayerNameAndColorSetupState;
    using PlayerActionKeySetupState;

    public class DefaultState : SceneStateBehavior<Void, Void>
    {
        public PlayerNameAndColorSetupState inputPlayerNameUIScene;
        public PlayerActionKeySetupState actionKeySetupUIScene;
        public CharacterOptionsState charOptionUIScene;

        private PlayerNameAndColorSetupState activeNameColorSetupScene;
        private PlayerActionKeySetupState activeKeySetupScene;

        private void Start()
        {
            foreach (Selectable selectable in FindObjectsOfType(typeof(Selectable)))
            {
                selectable.OnSelected += OnCharacterSelected;
            }
        }

        private void OnCharacterSelected(Selectable selectedChar)
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                CharacterPlayer charPlayer = selectedChar.GetComponent<CharacterPlayer>();
                if (charPlayer.player == null)
                {
                    charPlayer.player = new PlayerSetting()
                    {
                        characterSprite = selectedChar.GetComponent<CharacterSprite>().overrideCharacterSprite
                    };
                    SceneStateManager.instance.Push(inputPlayerNameUIScene, null);
                    activeNameColorSetupScene = inputPlayerNameUIScene;
                    activeNameColorSetupScene.character = selectedChar.gameObject;
                }
                else
                {
                    SceneStateManager.instance.Push(charOptionUIScene, null);
                    charOptionUIScene.character = selectedChar.gameObject;
                }
            }
        }

        protected override Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            if (activeNameColorSetupScene != null)
            {
                if (activeNameColorSetupScene.cancelled)
                {
                    CharacterPlayer charPlayer = activeNameColorSetupScene.character.GetComponent<CharacterPlayer>();
                    //TODO: delete player;
                    charPlayer.player = null;
                }
                else
                {
                    SceneStateManager.instance.Push(actionKeySetupUIScene, null);
                    activeKeySetupScene = actionKeySetupUIScene;
                    activeKeySetupScene.character = activeNameColorSetupScene.character;
                }
                activeNameColorSetupScene = null;
            }
            else if (activeKeySetupScene != null)
            {
                if (activeKeySetupScene.cancelled)
                {
                    CharacterPlayer charPlayer = activeKeySetupScene.character.GetComponent<CharacterPlayer>();
                    // TODO: delete player
                    charPlayer.player = null;
                }
                activeKeySetupScene = null;
            }
            return Task.CompletedTask;
        }

    }
}
