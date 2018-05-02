using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace APlusOrFail.Components
{
    [RequireComponent(typeof(RectTransform))]
    public class NameTag : MonoBehaviour
    {
        private RectTransform rectTransform;
        public Text nameText;
        public Vector2 worldOffset;

        public new Camera camera { get; set; }
        public RectTransform canvasRectTransform { get; set; }
        public Transform targetTransform { get; set; }

        private IReadOnlySharedPlayerSetting _playerSetting;
        public IReadOnlySharedPlayerSetting playerSetting { get { return _playerSetting; } set { SetProperty(ref _playerSetting, value); } }

        private void SetProperty<T>(ref T property, T value)
        {
            if (!Equals(property, value))
            {
                property = value;
                ApplyProperties();
            }
        }


        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplyProperties();
        }

        public void ApplyProperties()
        {
            gameObject.SetActive(targetTransform != null);
            if (targetTransform != null)
            {
                nameText.text = playerSetting?.name ?? "";
                nameText.color = playerSetting?.color ?? Color.white;
            }
        }

        private void Update()
        {
            Vector2 screenPoint = camera.WorldToScreenPoint(targetTransform.transform.position + ((Vector3)worldOffset));
            Vector2 canvasPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPoint, null, out canvasPoint);
            Rect rect = canvasRectTransform.rect;
            Vector2 anchorMinAndMax = new Vector2(canvasPoint.x / rect.width + 0.5f, canvasPoint.y / rect.height + 0.5f);
            rectTransform.anchorMin = rectTransform.anchorMax = anchorMinAndMax;
        }
    }
}
