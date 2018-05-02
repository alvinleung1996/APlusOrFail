using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace APlusOrFail.Maps.SceneStates.ObjectSelectionSceneState
{
    using Components;
    using Objects;
    
    public class ObjectSelectionSceneState : SceneStateBehavior<IMapStat, Void>
    {
        private new Camera camera;
        public Canvas backgroundCanvas;
        public Canvas objectCanvas;
        public Canvas foregroundCanvas;
        public KeyCursor keyCursorPrefab;
        public Vector2 gridUnitScale = new Vector2(200, 200);
        public float radius = 300;

        private readonly List<ObjectPrefabInfo> attachedPrefabInfos = new List<ObjectPrefabInfo>();
        private readonly List<KeyCursor> keyCursors = new List<KeyCursor>();
        

        private void Start()
        {
            camera = Camera.main;
            HideUI();
        }

        protected override Task OnLoad()
        {
            backgroundCanvas.worldCamera = objectCanvas.worldCamera = AutoResizeCamera.instance.GetComponent<Camera>();
            backgroundCanvas.sortingLayerID = objectCanvas.sortingLayerID = SortingLayerId.UI;
            return Task.CompletedTask;
        }

        protected override Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            if (unloadedSceneState == null)
            {
                ShowUI();
            }
            return Task.CompletedTask;
        }

        protected override Task OnBlur()
        {
            HideUI();
            return Task.CompletedTask;
        }

        private List<SpriteRenderer> objSpriteRenderers = new List<SpriteRenderer>();
        private void ShowUI()
        {
            backgroundCanvas.gameObject.SetActive(true);
            objectCanvas.gameObject.SetActive(true);
            foregroundCanvas.gameObject.SetActive(true);

            RectTransform objectCanvasRectTransform = objectCanvas.GetComponent<RectTransform>();
            IReadOnlyList<ObjectPrefabInfo> usableObjects = arg.roundSettings[arg.currentRound].usableObjects;

            float angleInterval = 2 * Mathf.PI / usableObjects.Count;
            for (int i = 0; i < usableObjects.Count; ++i)
            {
                // https://answers.unity.com/questions/1007585/reading-and-setting-asn-objects-global-scale-with.html

                ObjectPrefabInfo prefabInfo = Instantiate(usableObjects[i], objectCanvas.transform);
                prefabInfo.transform.localScale = Multiply(prefabInfo.transform.localScale, new Vector3(gridUnitScale.x, gridUnitScale.y, 1));

                prefabInfo.GetComponent<MapGridPlacer>().enabled = false;
                prefabInfo.gameObject.SetLayerRecursively(LayerId.SelectableObjects);

                prefabInfo.GetComponentsInChildren(objSpriteRenderers);
                foreach (SpriteRenderer sr in objSpriteRenderers)
                {
                    sr.sortingLayerID = SortingLayerId.UI;
                    sr.sortingOrder = 1;
                }

                RectInt objLocalGridBound = prefabInfo.GetComponentsInChildren<MapGridRect>().GetLocalRects().GetOuterBound();

                float angle = Mathf.PI / 2 - angleInterval * i;
                Vector2 position = new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
                position += objectCanvasRectTransform.rect.center - Multiply(objLocalGridBound.center, gridUnitScale);
                prefabInfo.transform.localPosition = position;

                attachedPrefabInfos.Add(prefabInfo);
            }

            foreach (IReadOnlySharedPlayerSetting player in arg.playerStats)
            {
                AddKeyCursor(player);
            }
        }

        private void HideUI()
        {
            backgroundCanvas.gameObject.SetActive(false);
            objectCanvas.gameObject.SetActive(false);
            foregroundCanvas.gameObject.SetActive(false);

            foreach (ObjectPrefabInfo attachedPrefabInfo in attachedPrefabInfos)
            {
                Destroy(attachedPrefabInfo.gameObject);
            }
            attachedPrefabInfos.Clear();

            for (int i = keyCursors.Count - 1; i >= 0; --i)
            {
                RemoveKeyCursor(keyCursors[i]);
            }
        }

        private void AddKeyCursor(IReadOnlySharedPlayerSetting player)
        {
            KeyCursor keyCursor = Instantiate(keyCursorPrefab, foregroundCanvas.transform);
            keyCursor.player = player;
            keyCursors.Add(keyCursor);
        }

        private void RemoveKeyCursor(KeyCursor keyCursor)
        {
            keyCursors.Remove(keyCursor);
            Destroy(keyCursor.gameObject);
        }

        private void Update()
        {
            if (phase.IsAtLeast(SceneStatePhase.Focused))
            {
                for (int i = keyCursors.Count - 1; i >= 0; --i)
                {
                    KeyCursor keyCursor = keyCursors[i];
                    if (HasKeyUp(keyCursor.player, PlayerAction.Action1))
                    {
                        ObjectPrefabInfo prefabInfo = Physics2D.OverlapPoint(camera.ViewportToWorldPoint(keyCursor.viewportLocation), 1 << LayerId.SelectableObjects)?.gameObject.GetComponentInParent<ObjectPrefabInfo>();
                        if (prefabInfo != null)
                        {
                            arg.roundPlayerStats[arg.currentRound, arg.playerStats.FindIndex(ps => ps ==  keyCursor.player)]
                                .selectedObjectPrefab = prefabInfo.prefab;

                            RemoveKeyCursor(keyCursor);

                            Destroy(prefabInfo.gameObject);
                            attachedPrefabInfos.Remove(prefabInfo);

                            if (keyCursors.Count == 0 || attachedPrefabInfos.Count == 0)
                            {
                                SceneStateManager.instance.Pop(this, null);
                            }
                        }
                    }
                }
                
            }
        }

        private bool HasKeyUp(IReadOnlySharedPlayerSetting player, PlayerAction action)
        {
            KeyCode code = player.GetKeyForAction(action);
            return code != KeyCode.None && Input.GetKeyUp(code);
        }

        private Vector3 Multiply(Vector3 a, Vector3 b) => new Vector3(
            a.x * b.x,
            a.y * b.y,
            a.z * b.z
        );

        private Vector2 Multiply(Vector2 a, Vector2 b) => new Vector2(
            a.x * b.x,
            a.y * b.y
        );

    }
}
