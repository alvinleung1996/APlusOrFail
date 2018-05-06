using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace APlusOrFail.Maps.SceneStates
{
    using Components;
    using Character;

    public class ResultSceneState : ObservableSceneStateBehavior<IMapStat, Void, IResultSceneState>, IResultSceneState
    {
        public Canvas canvas;
        public CharacterControl characterPrefab;
        public NameTag nameTagPrefab;

        private readonly List<ValueTuple<IReadOnlySharedPlayerSetting, CharacterControl, NameTag>> waitingPlayers
            = new List<ValueTuple<IReadOnlySharedPlayerSetting, CharacterControl, NameTag>>();
        protected override IResultSceneState observable => this;
        private bool poped;

        protected override void Awake()
        {
            base.Awake();
            canvas.gameObject.SetActive(false);
        }

        public override Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            Task task = base.OnFocus(unloadedSceneState, result);

            poped = false;

            if (unloadedSceneState == null)
            {
                canvas.gameObject.SetActive(true);

                RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
                Camera camera = AutoResizeCamera.instance.GetComponent<Camera>();

                foreach (IPlayerStat ps in arg.playerStats.Where(ps => ps.wonOverall))
                {
                    CharacterControl charControl = Instantiate(
                        characterPrefab,
                        arg.roundSettings[arg.currentRound - 1].spawnArea.transform.position,
                        characterPrefab.transform.rotation
                    );

                    charControl.GetComponent<CharacterSpriteId>().spriteId = ps.characterSpriteId;
                    charControl.GetComponent<CharacterPlayer>().playerSetting = ps;

                    AutoResizeCamera.instance.Trace(charControl.transform);

                    NameTag nameTag = Instantiate(nameTagPrefab, canvas.transform);
                    nameTag.camera = camera;
                    nameTag.canvasRectTransform = canvasRectTransform;
                    nameTag.targetTransform = charControl.transform;
                    nameTag.playerSetting = ps;

                    waitingPlayers.Add(new ValueTuple<IReadOnlySharedPlayerSetting, CharacterControl, NameTag>(ps, charControl, nameTag));
                }
            }
            return task;
        }

        private void Update()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                for (int i = waitingPlayers.Count - 1; i >= 0; --i)
                {
                    ValueTuple<IReadOnlySharedPlayerSetting, CharacterControl, NameTag> tuple = waitingPlayers[i];

                    bool ok = HasKeyUp(tuple.Item1, PlayerAction.Action1);
                    if (ok)
                    {
                        waitingPlayers.RemoveAt(i);
                        Destroy(tuple.Item3.gameObject);
                        AutoResizeCamera.instance.Untrace(tuple.Item2.transform);
                        Destroy(tuple.Item2.gameObject);
                    }
                }

                if (!poped && waitingPlayers.Count == 0)
                {
                    PopSceneState(null);
                    poped = true;
                }
            }
        }

        public override Task OnBlur()
        {
            Task task = base.OnBlur();
            canvas.gameObject.SetActive(false);

            AutoResizeCamera.instance.UntraceAll();
            foreach (var tuple in waitingPlayers)
            {
                Destroy(tuple.Item3.gameObject);
                Destroy(tuple.Item2.gameObject);
            }
            waitingPlayers.Clear();

            return task;
        }

        private bool HasKeyUp(IReadOnlySharedPlayerSetting player, PlayerAction action)
        {
            KeyCode code = player.GetKeyForAction(action);
            return code != KeyCode.None && Input.GetKeyUp(code);
        }
    }
}
