using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace APlusOrFail.Maps.SceneStates.RankSceneState
{
    public class RankSceneState : ObservableSceneStateBehavior<IMapStat, Void, IRankSceneState>, IRankSceneState
    {
        public Canvas canvas;
        public ScoreBoardAnimationController scoreBoardAnimationController;
        public Regions regions;
        public PlayerScores playerScores;
        public TooEasyNoPointBanner tooEasyBanner;
        
        private readonly List<IReadOnlySharedPlayerSetting> waitingPlayers = new List<IReadOnlySharedPlayerSetting>();
        protected override IRankSceneState observable => this;
        private bool poped;

        protected override void Awake()
        {
            base.Awake();
            canvas.gameObject.SetActive(false);
        }
        
        public override async Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            Task task = base.OnFocus(unloadedSceneState, result);
            poped = false;
            if (unloadedSceneState == null)
            {
                waitingPlayers.AddRange(arg.playerStats);
                await ShowUI();
            }
            await task;
        }
        
        public override async Task OnBlur()
        {
            Task task = base.OnBlur();
            await HideUI();
            await task;
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

                if (!poped && waitingPlayers.Count == 0)
                { 
                    PopSceneState(null);
                    poped = true;
                }
            }
        }

        private async Task ShowUI()
        {
            canvas.gameObject.SetActive(true);
            regions.UpdateRegions(arg);
            playerScores.UpdatePlayerScoreList(arg);
            await scoreBoardAnimationController.Open();
            await tooEasyBanner.UpdateBanner(arg.roundStats[arg.currentRound]);
        }

        private async Task HideUI()
        {
            await tooEasyBanner.UpdateBanner(null);
            await scoreBoardAnimationController.Close();
            regions.UpdateRegions(null);
            playerScores.UpdatePlayerScoreList(null);
            canvas.gameObject.SetActive(false);
        }

        private bool HasKeyUp(IReadOnlySharedPlayerSetting player, PlayerAction action)
        {
            KeyCode code = player.GetKeyForAction(action);
            return code != KeyCode.None && Input.GetKeyUp(code);
        }
    }
}
