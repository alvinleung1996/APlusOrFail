using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace APlusOrFail.Maps.SceneStates.RoundSceneState
{
    using Character;
    using Components;

    public class RoundSceneState : ObservableSceneStateBehavior<IMapStat, Void, IPlaySceneState>, IPlaySceneState
    {
        public CharacterControl characterPrefab;
        public RectTransform canvasRectTransform;
        public NameTag nameTagPrefab;

        protected override IPlaySceneState observable => this;
        private readonly HashSet<CharacterControl> notEndedCharControls = new HashSet<CharacterControl>();
        private readonly HashSet<CharacterControl> endedCharControls = new HashSet<CharacterControl>();
        private readonly List<NameTag> nameTags = new List<NameTag>();
        private Coroutine timerCoroutine;


        public override Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            Task task = base.OnFocus(unloadedSceneState, result);

            if (unloadedSceneState == null)
            {
                canvasRectTransform.gameObject.SetActive(true);

                MapGridPlacer spawnArea = arg.roundSettings[arg.currentRound].spawnArea;
                RectInt bound = spawnArea.GetComponentsInChildren<MapGridRect>()
                    .GetLocalRects()
                    .Rotate(spawnArea.rotation)
                    .Move(spawnArea.gridPosition)
                    .GetInnerBound();
                Vector2 spawnPoint = MapArea.instance.LocalToWorldPosition(bound.center);

                int i = 0;
                foreach (IReadOnlySharedPlayerSetting player in arg.playerStats)
                {
                    CharacterControl charControl = Instantiate(characterPrefab, spawnPoint, characterPrefab.transform.rotation);
                    CharacterSpriteId charId = charControl.GetComponent<CharacterSpriteId>();
                    CharacterPlayer charPlayer = charControl.GetComponent<CharacterPlayer>();

                    charId.spriteId = player.characterSpriteId;
                    charPlayer.playerSetting = player;
                    charControl.onEndedChanged += OnCharEnded;

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
                        nameTag = Instantiate(nameTagPrefab, canvasRectTransform.transform);
                        nameTag.camera = AutoResizeCamera.instance.GetComponent<Camera>();
                        nameTag.canvasRectTransform = canvasRectTransform;
                        nameTags.Add(nameTag);
                    }
                    else
                    {
                        nameTag = nameTags[i];
                    }
                    nameTag.targetTransform = charPlayer.transform;
                    nameTag.playerSetting = player;


                    AutoResizeCamera.instance.Trace(charControl.transform);


                    ++i;
                }
                for (int j = i; j < nameTags.Count; ++j)
                {
                    nameTags[j].targetTransform = null;
                }

                if (notEndedCharControls.Count == 0)
                {
                    OnAllCharacterEnded();
                }
            }

            timerCoroutine = StartCoroutine(TimerCoroutine());
            return task;
        }

        public override Task OnBlur()
        {
            Task task = base.OnBlur();

            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }

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
                nameTag.targetTransform = null;
            }

            AutoResizeCamera.instance.UntraceAll();

            return task;
        }

        private readonly List<CharacterControl> tempControls = new List<CharacterControl>();
        private IEnumerator TimerCoroutine()
        {
            yield return new WaitForSeconds(arg.roundSettings[arg.currentRound].timeLimit);
            timerCoroutine = null;

            tempControls.AddRange(notEndedCharControls);
            foreach (CharacterControl charPlayer in tempControls)
            {
                charPlayer.ChangeHealth(new ReadOnlyPlayerHealthChange(PlayerHealthChangeReason.Timeout, -charPlayer.health, null));
            }

            tempControls.Clear();
        }

        private void OnCharEnded(CharacterControl charControl, bool ended)
        {
            if (ended)
            {
                endedCharControls.Add(charControl);
                notEndedCharControls.Remove(charControl);
                AutoResizeCamera.instance.Untrace(charControl.transform);
            }
            else
            {
                endedCharControls.Remove(charControl);
                notEndedCharControls.Add(charControl);
                AutoResizeCamera.instance.Trace(charControl.transform);
            }

            if (notEndedCharControls.Count == 0)
            {
                OnAllCharacterEnded();
            }
        }

        private void OnAllCharacterEnded()
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
            SceneStateManager.instance.Pop(this, null);
        }

        private void CalculateScore(IEnumerable<CharacterControl> charControls)
        {
            IRoundStat roundStat = arg.roundStats[arg.currentRound];

            roundStat.tooEasyNoPoint = charControls.Count() > 1 && charControls.All(cc => cc.won);

            foreach (CharacterControl charControl in charControls)
            {
                IReadOnlySharedPlayerSetting player = charControl.GetComponent<CharacterPlayer>().playerSetting;
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
