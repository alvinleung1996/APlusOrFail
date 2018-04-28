using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Linq;

namespace APlusOrFail.Maps.SceneStates.DefaultSceneState
{
    using ObjectSelectionSceneState;
    using PlaceObjectSceneState;
    using RoundSceneState;
    using RankSceneState;

    public class DefaultSceneState : SceneStateBehavior<IMapStat, Void>
    {
        public ObjectSelectionSceneState objectSelectionUIScene;
        public PlaceObjectSceneState placeObjectUIScene;
        public RoundSceneState roundUIScene;
        public RankSceneState rankSceneState;
        public ResultSceneState resultSceneState;
        

        protected override Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            Type unloadedType = unloadedSceneState?.GetType();
            if (unloadedSceneState == null)
            {
                OnMapStart();
            }
            else if (unloadedType == typeof(ObjectSelectionSceneState))
            {
                OnObjectSelectionFinished();
            }
            else if (unloadedType == typeof(PlaceObjectSceneState))
            {
                OnPlaceObjectFinished();
            }
            else if (unloadedType == typeof(RoundSceneState))
            {
                OnRoundUISceneFinished();
            }
            else if (unloadedType == typeof(RankSceneState))
            {
                OnRankFinished();
            }
            else if (unloadedType == typeof(ResultSceneState))
            {
                OnResultFinished();
            }
            return Task.CompletedTask;
        }

        private void OnMapStart()
        {
            OnRankFinished();
        }

        private void OnObjectSelectionFinished()
        {
            arg.roundStats[arg.currentRound].state = RoundState.PlacingObjects;
            SceneStateManager.instance.Push(placeObjectUIScene, arg);
        }

        private void OnPlaceObjectFinished()
        {
            arg.roundStats[arg.currentRound].state = RoundState.Playing;
            SceneStateManager.instance.Push(roundUIScene, arg);
        }

        private void OnRoundUISceneFinished()
        {
            arg.roundStats[arg.currentRound].state = RoundState.Ranking;
            SceneStateManager.instance.Push(rankSceneState, arg);
        }

        private List<int> maxScores = new List<int>();
        private void OnRankFinished()
        {
            if (arg.currentRound >= 0 && arg.currentRound < arg.roundCount) arg.roundStats[arg.currentRound].state = RoundState.None;
            ++arg.currentRound;
            if (arg.currentRound < arg.roundCount)
            {
                SceneStateManager.instance.Push(objectSelectionUIScene, arg);
                arg.roundStats[arg.currentRound].state = RoundState.SelectingObjects;
            }
            else
            {
                print("Round Finished!");

                maxScores.Clear();
                maxScores.AddRange(arg.playerStats.Select(ps => arg
                    .GetRoundPlayerStatOfPlayer(ps.order)
                    .SelectMany(rps => rps.scoreChanges)
                    .Sum(sc => sc.scoreDelta)));

                int maxScore = maxScores.Max();

                foreach (PlayerStat ps in arg.playerStats.Where((ps, i) => maxScores[i] == maxScore))
                {
                    ps.wonOverall = true;
                }

                PushSceneState(resultSceneState, arg);
            }
        }

        private void OnResultFinished()
        {
            SceneManager.LoadSceneAsync(SceneBuildIndex.setup);
        }
    }
}
