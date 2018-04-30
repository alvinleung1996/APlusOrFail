using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace APlusOrFail.Maps.SceneStates.RoundSceneState
{
    using Character;
    using Components;
    using Components.NameTag;

    public class RoundSceneState : SceneStateBehavior<IMapStat, Void>
    {
        public CharacterControl characterPrefab;
        public RectTransform canvasRectTransform;
        public NameTag nameTagPrefab;
        
        private readonly HashSet<CharacterControl> notEndedCharControls = new HashSet<CharacterControl>();
        private readonly HashSet<CharacterControl> endedCharControls = new HashSet<CharacterControl>();
        private readonly List<NameTag> nameTags = new List<NameTag>();


        protected override Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            if (unloadedSceneState == null)
            {
                canvasRectTransform.gameObject.SetActive(true);

                MapGridPlacer spawnArea = arg.roundSettings[arg.currentRound].spawnArea;
                RectInt bound = spawnArea.GetComponentsInChildren<MapGridRect>()
                    .GetLocalRects()
                    .Rotate(spawnArea.rotation)
                    .Move(spawnArea.gridPosition)
                    .GetInnerBound();
                Vector2 spawnPoint = MapManager.mapStat.mapArea.LocalToWorldPosition(bound.center);

                int i = 0;
                foreach (IReadOnlyPlayerSetting player in arg.playerStats)
                {
                    CharacterControl charControl = Instantiate(characterPrefab, spawnPoint, characterPrefab.transform.rotation);
                    CharacterPlayer charPlayer = charControl.GetComponent<CharacterPlayer>();

                    charControl.onEndedChanged += OnCharEnded;
                    charPlayer.player = player;

                    if (charControl.ended)
                    {
                        endedCharControls.Add(charControl);
                    }
                    else
                    {
                        notEndedCharControls.Add(charControl);
                    }


                    NameTag nameTag;
                    if (i >= nameTags.Count)
                    {
                        nameTag = Instantiate(nameTagPrefab, canvasRectTransform);
                        nameTag.camera = arg.camera.GetComponent<Camera>();
                        nameTag.canvasRectTransform = canvasRectTransform;
                        nameTags.Add(nameTag);
                    }
                    else
                    {
                        nameTag = nameTags[i];
                    }
                    nameTag.charPlayer = charPlayer;


                    arg.camera.Trace(charControl.gameObject);


                    ++i;
                }
                for (int j = i; j < nameTags.Count; ++j)
                {
                    nameTags[j].charPlayer = null;
                }

                if (notEndedCharControls.Count == 0)
                {
                    OnAllCharacterEnded();
                }
            }
            return Task.CompletedTask;
        }

        protected override Task OnBlur()
        {
            canvasRectTransform.gameObject.SetActive(false);

            IEnumerable<CharacterControl> charControls = notEndedCharControls.Concat(endedCharControls);
            CalculateScore(charControls);
            foreach (CharacterControl charControl in charControls)
            {
                charControl.onEndedChanged -= OnCharEnded;
                Destroy(charControl.gameObject);
            }
            notEndedCharControls.Clear();
            endedCharControls.Clear();

            foreach (NameTag nameTag in nameTags)
            {
                nameTag.charPlayer = null;
            }

            arg.camera.UntraceAll();

            return Task.CompletedTask;
        }

        private void OnCharEnded(CharacterControl charControl, bool ended)
        {
            if (ended)
            {
                endedCharControls.Add(charControl);
                notEndedCharControls.Remove(charControl);
                arg.camera.Untrace(charControl.gameObject);
            }
            else
            {
                endedCharControls.Remove(charControl);
                notEndedCharControls.Add(charControl);
                arg.camera.Trace(charControl.gameObject);
            }

            if (notEndedCharControls.Count == 0)
            {
                OnAllCharacterEnded();
            }
        }

        private void OnAllCharacterEnded()
        {
            SceneStateManager.instance.Pop(this, null);
        }

        private void CalculateScore(IEnumerable<CharacterControl> charControls)
        {
            IRoundStat roundStat = arg.roundStats[arg.currentRound];

            roundStat.tooEasyNoPoint = charControls.All(cc => cc.won);

            foreach (CharacterControl charControl in charControls)
            {
                IReadOnlyPlayerSetting player = charControl.GetComponent<CharacterPlayer>().player;
                IRoundPlayerStat roundPlayerStat = arg.roundPlayerStats[arg.currentRound, arg.playerStats.FindIndex(ps => ps == player)];

                if (!roundStat.tooEasyNoPoint && charControl.won)
                {
                    charControl.ChangeScore(roundStat.CreatePointsChange(PlayerPointsChangeReason.Won, charControl.wonCause));
                }

                roundPlayerStat.healthChanges.AddRange(charControl.healthChanges);
                roundPlayerStat.scoreChanges.AddRange(charControl.scoreChanges);
            }

        }
    }
}
