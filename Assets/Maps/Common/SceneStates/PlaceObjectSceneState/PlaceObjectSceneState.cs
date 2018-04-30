using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace APlusOrFail.Maps.SceneStates.PlaceObjectSceneState
{
    using Objects;
    
    public class PlaceObjectSceneState : SceneStateBehavior<IMapStat, Void>
    {
        private new Camera camera;
        public RectTransform uiScene;
        public ObjectCursor cursorPrefab;

        private readonly List<ObjectCursor> objectCursors = new List<ObjectCursor>();


        private void Start()
        {
            camera = Camera.main;
            HideUI();
        }

        protected override Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            ShowUI();
            return Task.CompletedTask;
        }

        protected override Task OnBlur()
        {
            HideUI();
            return Task.CompletedTask;
        }

        private void ShowUI()
        {
            uiScene.gameObject.SetActive(true);

            for (int i = 0; i < arg.playerStats.Count; ++i)
            {
                IReadOnlyRoundPlayerStat roundPlayerStat = arg.roundPlayerStats[arg.currentRound, i];
                if (roundPlayerStat.selectedObjectPrefab != null)
                {
                    ObjectCursor cursor = Instantiate(cursorPrefab, uiScene);
                    cursor.player = arg.playerStats[i];
                    cursor.objectPrefab = roundPlayerStat.selectedObjectPrefab;
                    cursor.camera = camera;
                    cursor.onCursorDestroyed += OnObjectCursorDestroyed;
                    objectCursors.Add(cursor);
                }
            }
            if (objectCursors.Count == 0)
            {
                SceneStateManager.instance.Pop(this, null);
            }
        }

        private void HideUI()
        {
            uiScene.gameObject.SetActive(false);
            for (int i = objectCursors.Count - 1; i >= 0; --i)
            {
                ObjectCursor cursor = objectCursors[i];
                RemoveObjectCursor(cursor);
                Destroy(cursor);
            }
        }

        private void RemoveObjectCursor(ObjectCursor cursor)
        {
            cursor.onCursorDestroyed -= OnObjectCursorDestroyed;
            objectCursors.Remove(cursor);
        }

        private void OnObjectCursorDestroyed(ObjectCursor cursor)
        {
            RemoveObjectCursor(cursor);
            if (objectCursors.Count == 0)
            {
                SceneStateManager.instance.Pop(this, null);
            }
        }
    }
}
