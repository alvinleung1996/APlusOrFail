using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace APlusOrFail.Maps.SceneStates.RankSceneState
{
    using UI;

    [RequireComponent(typeof(FractionLayoutController))]
    public class PlayerScore : MonoBehaviour
    {
        public Text nameText;
        public RectTransform scores;
        private FractionLayoutController layoutController;
        public SubScore subScorePrefab;

        private IMapStat mapStat;
        private int playerOrder;
        private readonly List<SubScore> subScores = new List<SubScore>();
        

        private void Awake()
        {
            layoutController = scores.GetComponent<FractionLayoutController>();
            UpdatePlayerScore(mapStat, playerOrder);
        }

        public void UpdatePlayerScore(IMapStat mapStat, int playerOrder)
        {
            this.mapStat = mapStat;
            this.playerOrder = playerOrder;
            
            gameObject.SetActive(mapStat != null);
            if (mapStat != null)
            {
                nameText.text = mapStat.playerStats[playerOrder].player.name;
                layoutController.denominatorWidth = mapStat.GetMapScore();

                int i = 0;
                foreach (IPlayerScoreChange playerScoreChange in mapStat.GetRoundPlayerStatOfPlayer(playerOrder).SelectMany(rps => rps.scoreChanges))
                {
                    SubScore subScore;
                    if (i < subScores.Count)
                    {
                        subScore = subScores[i];
                    }
                    else
                    {
                        subScore = Instantiate(subScorePrefab, scores.transform);
                        subScores.Add(subScore);
                    }

                    subScore.UpdateSubScore(mapStat, playerOrder, i);

                    ++i;
                }
                for (int j = i; j < subScores.Count; ++j)
                {
                    subScores[j].UpdateSubScore(null, -1, -1);
                }
            }
        }
    }
}
