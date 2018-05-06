using UnityEngine;
using System.Threading.Tasks;

namespace APlusOrFail.Maps.SceneStates.RankSceneState
{
    public class TooEasyNoPointBanner : MonoBehaviour
    {
        //private Animator animator;

        private IReadonlyRoundStat roundStat;

        private void Awake()
        {
            UpdateBanner(roundStat);
        }

        public Task UpdateBanner(IReadonlyRoundStat roundStat)
        {
            this.roundStat = roundStat;
            bool active = roundStat != null && roundStat.tooEasyNoPoint;
            gameObject.SetActive(active);
            return Task.CompletedTask;
        }
    }
}
