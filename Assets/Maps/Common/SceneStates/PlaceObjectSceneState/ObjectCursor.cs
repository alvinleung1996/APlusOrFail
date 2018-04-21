﻿using System;
using UnityEngine;

namespace APlusOrFail.Maps.SceneStates.PlaceObjectSceneState
{
    using Objects;

    public class ObjectCursor : KeyCursor
    {
        private class KeyTracker
        {
            public readonly Player.Action action;
            private readonly KeyCode code;
            public bool downed { get; private set; }
            public bool pressed { get; private set; }
            public bool uped { get; private set; }

            private bool cancelUp;

            public KeyTracker(Player player, Player.Action action)
            {
                this.action = action;
                code = player.GetKeyForAction(action) ?? KeyCode.None;
            }

            public void Update()
            {
                if (code != KeyCode.None)
                {
                    if (pressed)
                    {
                        downed = false;
                        if (Input.GetKeyUp(code))
                        {
                            pressed = false;
                            uped = !cancelUp;
                            cancelUp = false;
                        }
                    }
                    else
                    {
                        uped = false;
                        if (Input.GetKeyDown(code))
                        {
                            pressed = true;
                            downed = true;
                        }
                    }
                }
            }

            public void CancelUp()
            {
                cancelUp = true;
            }
        }

        [NonSerialized] public new Camera camera;
        [NonSerialized] public ObjectPrefabInfo objectPrefab;

        private ObjectPrefabInfo attachedObject;
        private ICustomizableObject customizableObject;
        private ObjectGridPlacer objectPlacer;

        private KeyTracker action1Key;
        private KeyTracker action2Key;
        private KeyTracker upKey;
        private KeyTracker leftKey;
        private KeyTracker rightKey;
        private KeyTracker downKey;

        public event EventHandler<ObjectCursor> onCursorDestroyed;

        protected override void Start()
        {
            base.Start();

            attachedObject = Instantiate(objectPrefab);
            customizableObject = attachedObject.GetComponent<ICustomizableObject>();
            objectPlacer = attachedObject.GetComponent<ObjectGridPlacer>();
            objectPlacer.registerInGrid = false;

            action1Key = new KeyTracker(player, Player.Action.Action1);
            action2Key = new KeyTracker(player, Player.Action.Action2);
            upKey = new KeyTracker(player, Player.Action.Up);
            leftKey = new KeyTracker(player, Player.Action.Left);
            rightKey = new KeyTracker(player, Player.Action.Right);
            downKey = new KeyTracker(player, Player.Action.Down);
        }

        protected override void Update()
        {
            action1Key.Update();
            action2Key.Update();
            upKey.Update();
            leftKey.Update();
            rightKey.Update();
            downKey.Update();

            int customizeAction = -1;
            if (action2Key.pressed)
            {
                if (upKey.uped) customizeAction = 0;
                else if (leftKey.uped) customizeAction = 1;
                else if (downKey.uped) customizeAction = 2;
                else if (rightKey.uped) customizeAction = 3;
            }
            if (customizeAction >= 0)
            {
                action2Key.CancelUp();
                customizableObject.NextSetting(customizeAction);
            }
            else if (action2Key.uped)
            {
                objectPlacer.rotation = (ObjectGridRects.Rotation)(((int)objectPlacer.rotation + 1) % 4);
            }

            if (!action2Key.pressed)
            {
                base.Update();
            }
            
            objectPlacer.gridPosition = MapManager.mapStat.mapArea.WorldToGridPosition(camera.ViewportToWorldPoint(viewportLocation));
            bool placable = objectPlacer.IsRegisterable();

            if (placable)
            {
                Debug.LogFormat($"Player {player.id} can place!");
            }
            else
            {
                Debug.LogFormat($"Player {player.id} cannot place!");
            }

            if (action1Key.uped && placable)
            {
                objectPlacer.registerInGrid = true;
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            onCursorDestroyed?.Invoke(this);
        }
    }
}
