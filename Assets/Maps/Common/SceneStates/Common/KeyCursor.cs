using UnityEngine;
using UnityEngine.UI;

namespace APlusOrFail.Maps.SceneStates
{
    [RequireComponent(typeof(RectTransform))]
    public class KeyCursor : MonoBehaviour
    {
        protected RectTransform rectTransform;
        public RectTransform nameBackground;
        public Text nameText;

        private IReadOnlySharedPlayerSetting _player;
        public IReadOnlySharedPlayerSetting player { get { return _player; } set { SetProperty(ref _player, value); } }

        public float speed { get; set; } = 0.3f;
        
        public Vector2 viewportLocation => rectTransform != null ? rectTransform.anchorMin : Vector2.zero;


        private void SetProperty<T>(ref T property, T value)
        {
            if (!Equals(property, value))
            {
                property = value;
                ApplyProperties();
            }
        }

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplyProperties();
        }

        protected void ApplyProperties()
        {
            nameBackground.gameObject.SetActive(player != null);
            nameText.text = player?.name ?? "";
            nameText.color = player?.color ?? Color.white;
        }

        protected virtual void Update()
        {
            if (player != null)
            {
                bool leftPressed = HasKeyPressed(player, PlayerAction.Left);
                bool rightPressed = HasKeyPressed(player, PlayerAction.Right);
                bool upPressed = HasKeyPressed(player, PlayerAction.Up);
                bool downPressed = HasKeyPressed(player, PlayerAction.Down);

                bool left = leftPressed && !rightPressed;
                bool right = rightPressed && !leftPressed;
                bool up = upPressed && !downPressed;
                bool down = downPressed && !upPressed;

                Vector2 currentLocation = rectTransform.anchorMin;

                if (left)
                {
                    currentLocation.x = Mathf.Max(currentLocation.x - speed * Time.deltaTime, 0);
                }
                else if (right)
                {
                    currentLocation.x = Mathf.Min(currentLocation.x + speed * Time.deltaTime, 1);
                }

                if (up)
                {
                    currentLocation.y = Mathf.Min(currentLocation.y + speed * Time.deltaTime, 1);
                }
                else if (down)
                {
                    currentLocation.y = Mathf.Max(currentLocation.y - speed * Time.deltaTime, 0);
                }

                rectTransform.anchorMin = rectTransform.anchorMax = currentLocation;
            }
        }

        protected bool HasKeyPressed(IReadOnlySharedPlayerSetting player, PlayerAction action)
        {
            KeyCode key = player.GetKeyForAction(action);
            return key != KeyCode.None && Input.GetKey(key);
        }
    }
}
