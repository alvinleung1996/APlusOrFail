using System;
using UnityEngine;
using UnityEngine.UI;

namespace APlusOrFail.Maps.SceneStates.PlaceObjectSceneState
{
    using Components;
    using Objects;

    public class ObjectCursor : KeyCursor
    {
        private class KeyTracker
        {
            public readonly PlayerAction action;
            private readonly KeyCode code;
            public bool downed { get; private set; }
            public bool pressed { get; private set; }
            public bool uped { get; private set; }

            private bool cancelUp;

            public KeyTracker(IReadOnlySharedPlayerSetting player, PlayerAction action)
            {
                this.action = action;
                code = player.GetKeyForAction(action);
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

        public Image forbiddenSign;
        [NonSerialized] public new Camera camera;
        [NonSerialized] public ObjectPrefabInfo objectPrefab;

        private ObjectPrefabInfo attachedObject;
        private ICustomizableObject customizableObject;
        private MapGridPlacer objectPlacer;

        private KeyTracker action1Key;
        private KeyTracker action2Key;
        private KeyTracker upKey;
        private KeyTracker leftKey;
        private KeyTracker rightKey;
        private KeyTracker downKey;

        public event EventHandler<ObjectCursor> onCursorDestroyed;


        private void Start()
        {
            attachedObject = Instantiate(objectPrefab, MapArea.instance.transform);

            IObjectPlayerSource playerSource = attachedObject.GetComponent<IObjectPlayerSource>();
            if (playerSource != null) playerSource.player = player;

            customizableObject = attachedObject.GetComponent<ICustomizableObject>();
            objectPlacer = attachedObject.GetComponent<MapGridPlacer>();
            objectPlacer.registerInGrid = false;

            action1Key = new KeyTracker(player, PlayerAction.Action1);
            action2Key = new KeyTracker(player, PlayerAction.Action2);
            upKey = new KeyTracker(player, PlayerAction.Up);
            leftKey = new KeyTracker(player, PlayerAction.Left);
            rightKey = new KeyTracker(player, PlayerAction.Right);
            downKey = new KeyTracker(player, PlayerAction.Down);
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
                objectPlacer.rotation = (MapGridRectExtensions.Rotation)(((int)objectPlacer.rotation + 1) % 4);
            }
            else if (!action2Key.pressed)
            {
                base.Update();
            }
            

            objectPlacer.gridPosition = MapArea.instance.WorldToGridPosition(camera.ViewportToWorldPoint(viewportLocation));
            bool placable = objectPlacer.IsRegisterable();
            forbiddenSign.gameObject.SetActive(!placable);

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
