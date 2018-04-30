using UnityEngine;
using UnityEngine.UI;

namespace APlusOrFail.Maps.SceneStates.RankSceneState
{
    using UI;

    public class Region : MonoBehaviour
    {
        public Text nameText;
        private FractionLayoutElement layoutElement;
        
        private IRoundStat roundStat;

        private void Awake()
        {
            layoutElement = GetComponent<FractionLayoutElement>();
            UpdateRegion(roundStat);
        }

        public void UpdateRegion(IRoundStat roundStat)
        {
            this.roundStat = roundStat;
            gameObject.SetActive(roundStat != null);
            nameText.text = roundStat?.name ?? "";
            layoutElement.numeratorWidth = roundStat?.points ?? 0;
        }
    }
}
