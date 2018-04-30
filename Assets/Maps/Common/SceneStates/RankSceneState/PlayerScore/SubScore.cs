using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace APlusOrFail.Maps.SceneStates.RankSceneState
{
    using UI;

    [RequireComponent(typeof(Image), typeof(FractionLayoutElement))]
    public class SubScore : MonoBehaviour
    {
        private Image image;
        private FractionLayoutElement layoutElement;

        private IMapStat mapStat;
        private int playerOrder;
        private int scoreOrder;

        private void Awake()
        {
            image = GetComponent<Image>();
            layoutElement = GetComponent<FractionLayoutElement>();
            UpdateSubScore(mapStat, playerOrder, scoreOrder);
        }

        public void UpdateSubScore(IMapStat mapStat, int playerOrder, int scoreOrder)
        {
            this.mapStat = mapStat;
            this.playerOrder = playerOrder;
            this.scoreOrder = scoreOrder;

            gameObject.SetActive(mapStat != null);
            if (mapStat != null)
            {
                IReadOnlyPlayerScoreChange stat = mapStat.GetRoundPlayerStatOfPlayer(playerOrder).SelectMany(rps => rps.scoreChanges).Skip(scoreOrder).First();
                layoutElement.numeratorWidth = Mathf.Max(stat.delta, 0);
                image.color = stat.rankColor;
            }
        }
    }
}
