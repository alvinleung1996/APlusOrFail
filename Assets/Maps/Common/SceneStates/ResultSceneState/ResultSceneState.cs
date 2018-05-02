using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace APlusOrFail.Maps.SceneStates
{
    using Components;
    using Character;

    public class ResultSceneState : SceneStateBehavior<IMapStat, Void>
    {
        public Canvas canvas;
        public CharacterControl characterPrefab;

        private List<CharacterControl> attacedCharacters = new List<CharacterControl>();
        private readonly List<IReadOnlySharedPlayerSetting> waitingPlayers = new List<IReadOnlySharedPlayerSetting>();

        private void Awake()
        {
            canvas.gameObject.SetActive(false);
        }

        protected override Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            if (unloadedSceneState == null)
            {
                canvas.gameObject.SetActive(true);
                
                foreach (IPlayerStat ps in arg.playerStats.Where(ps => ps.wonOverall))
                {
                    CharacterControl charControl = Instantiate(
                        characterPrefab,
                        arg.roundSettings[arg.currentRound - 1].spawnArea.transform.position,
                        characterPrefab.transform.rotation
                    );
                    attacedCharacters.Add(charControl);

                    charControl.GetComponent<CharacterSpriteId>().spriteId = ps.characterSpriteId;
                    charControl.GetComponent<CharacterPlayer>().playerSetting = ps;
                    AutoResizeCamera.instance.Trace(charControl.transform);
                    waitingPlayers.Add(ps);
                }
            }
            return Task.CompletedTask;
        }

        private void Update()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                for (int i = waitingPlayers.Count - 1; i >= 0; --i)
                {
                    bool ok = HasKeyUp(waitingPlayers[i], PlayerAction.Action1);
                    if (ok)
                    {
                        waitingPlayers.RemoveAt(i);
                    }
                }

                if (waitingPlayers.Count == 0)
                {
                    PopSceneState(null);
                }
            }
        }

        protected override Task OnBlur()
        {
            canvas.gameObject.SetActive(false);

            foreach (CharacterControl charControl in attacedCharacters)
            {
                Destroy(charControl.gameObject);
                AutoResizeCamera.instance.Untrace(charControl.transform);
            }
            attacedCharacters.Clear();

            return Task.CompletedTask;
        }

        private bool HasKeyUp(IReadOnlySharedPlayerSetting player, PlayerAction action)
        {
            KeyCode code = player.GetKeyForAction(action);
            return code != KeyCode.None && Input.GetKeyUp(code);
        }
    }
}
