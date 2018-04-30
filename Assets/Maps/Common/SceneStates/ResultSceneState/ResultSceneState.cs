using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace APlusOrFail.Maps.SceneStates
{
    using Character;

    public class ResultSceneState : SceneStateBehavior<IMapStat, Void>
    {
        public Canvas canvas;
        public CharacterControl characterPrefab;

        private List<CharacterControl> attacedCharacters = new List<CharacterControl>();

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
                    CharacterPlayer charPlayer = charControl.GetComponent<CharacterPlayer>();

                    charPlayer.player = ps;

                    attacedCharacters.Add(charControl);
                    arg.camera.Trace(charControl.gameObject);
                }
            }
            return Task.CompletedTask;
        }


        protected override Task OnBlur()
        {
            canvas.gameObject.SetActive(false);

            foreach (CharacterControl charControl in attacedCharacters)
            {
                Destroy(charControl.gameObject);
                arg.camera.Untrace(charControl.gameObject);
            }
            attacedCharacters.Clear();

            return Task.CompletedTask;
        }
    }
}
