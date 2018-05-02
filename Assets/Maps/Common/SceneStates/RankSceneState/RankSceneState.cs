using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace APlusOrFail.Maps.SceneStates.RankSceneState
{
    public class RankSceneState : SceneStateBehavior<IMapStat, Void>
    {
        public Canvas canvas;
        public Regions regions;
        public PlayerScores playerScores;
        public TooEasyNoPointBanner tooEasyBanner;

        private readonly List<IReadOnlySharedPlayerSetting> waitingPlayers = new List<IReadOnlySharedPlayerSetting>();
        
        private void Awake()
        {
            canvas.gameObject.SetActive(false);
        }

        protected override async Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            if (unloadedSceneState == null)
            {
                waitingPlayers.AddRange(arg.playerStats);
                await ShowUI();
            }
        }

        protected override async Task OnBlur()
        {
            await HideUI();
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

        private async Task ShowUI()
        {
            canvas.gameObject.SetActive(true);
            regions.UpdateRegions(arg);
            playerScores.UpdatePlayerScoreList(arg);
            await tooEasyBanner.UpdateBanner(arg.roundStats[arg.currentRound]);
        }

        private async Task HideUI()
        {
            canvas.gameObject.SetActive(false);
            regions.UpdateRegions(null);
            playerScores.UpdatePlayerScoreList(null);
            await tooEasyBanner.UpdateBanner(null);
        }

        private bool HasKeyUp(IReadOnlySharedPlayerSetting player, PlayerAction action)
        {
            KeyCode code = player.GetKeyForAction(action);
            return code != KeyCode.None && Input.GetKeyUp(code);
        }
    }
}
