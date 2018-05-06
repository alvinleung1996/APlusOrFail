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

    public class DefaultSceneState : ObservableSceneStateBehavior<IMapStat, Void, IDefaultSceneState>, IDefaultSceneState
    {
        public ObjectSelectionSceneState objectSelectionUIScene;
        public PlaceObjectSceneState placeObjectUIScene;
        public RoundSceneState roundUIScene;
        public RankSceneState rankSceneState;
        public ResultSceneState resultSceneState;

        protected override IDefaultSceneState observable => this;

        public override Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            Task task = base.OnFocus(unloadedSceneState, result);
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
            return task;
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

        private readonly List<int> playerPoints = new List<int>();
        private void OnRankFinished()
        {
            if (arg.currentRound >= 0 && arg.currentRound < arg.roundSettings.Count)
                arg.roundStats[arg.currentRound].state = RoundState.None;

            ++arg.currentRound;

            playerPoints.Clear();
            playerPoints.AddRange(arg.playerStats.Select((ps, i) => arg
                    .GetRoundPlayerStatOfPlayer(i)
                    .SelectMany(rps => rps.scoreChanges)
                    .Sum(sc => sc.delta)));
            bool someonePassed = playerPoints.Any(p => p >= arg.passPoints);

            if (arg.currentRound < arg.roundSettings.Count && !someonePassed)
            {
                PushSceneState(objectSelectionUIScene, arg);
                arg.roundStats[arg.currentRound].state = RoundState.SelectingObjects;
            }
            else
            {
                print("Round Finished!");

                int maxPoints = playerPoints.Max();

                foreach (PlayerStat ps in arg.playerStats.Where((ps, i) => playerPoints[i] == maxPoints))
                {
                    ps.wonOverall = true;
                }

                PushSceneState(resultSceneState, arg);
            }
        }

        private void OnResultFinished()
        {
            PopSceneState(null);
        }
    }
}
