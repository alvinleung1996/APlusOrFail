using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace APlusOrFail.Maps.SceneStates.RankSceneState
{
    using UI;

    public class Regions : MonoBehaviour
    {
        private FractionLayoutController layoutController;
        public Region regionPrefab;

        private readonly List<Region> childRegions = new List<Region>();
        
        private IMapStat mapStat;


        private void Awake()
        {
            layoutController = GetComponent<FractionLayoutController>();
            UpdateRegions(mapStat);
        }

        public void UpdateRegions(IMapStat mapStat)
        {
            this.mapStat = mapStat;

            int i = 0;
            if (mapStat != null)
            {
                foreach (IRoundStat roundStat in mapStat.roundStats.Take(Mathf.Max(mapStat.currentRound + 1, mapStat.minRoundCount)))
                {
                    Region region;
                    if (i < childRegions.Count)
                    {
                        region = childRegions[i];
                    }
                    else
                    {
                        region = Instantiate(regionPrefab, transform);
                        childRegions.Add(region);
                    }

                    region.UpdateRegion(roundStat);

                    ++i;
                }
            }
            layoutController.denominatorWidth = mapStat?.roundSettings
                .Take(Mathf.Max(mapStat.currentRound + 1, mapStat.minRoundCount))
                .Sum(rs => rs.points)
                ?? 0;
            for (int j = i; j < childRegions.Count; ++j)
            {
                childRegions[j].UpdateRegion(null);
            }
        }
        
    }
}
