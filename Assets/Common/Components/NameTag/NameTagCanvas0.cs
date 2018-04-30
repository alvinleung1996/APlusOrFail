using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace APlusOrFail.NameTagCanvas
{
    [RequireComponent(typeof(RectTransform))]
    [System.Obsolete]
    public class NameTagCanvas0 : MonoBehaviour
    {
        public class NameTagInfo
        {
            public readonly Transform targetTransform;
            public Vector2 worldOffset = Vector2.zero;
            
            public string name {
                get
                {
                    return nameTagText.text;
                }
                set
                {
                    value = value?.Trim() ?? "";
                    nameTagText.text = value;
                    nameTagRectTrasform.gameObject.SetActive(value.Length > 0);
                }
            }

            public Color color
            {
                get
                {
                    return nameTagText.color;
                }
                set
                {
                    nameTagText.color = value;
                }
            }

            private readonly NameTagCanvas0 nameTagCanvas;
            private readonly RectTransform nameTagRectTrasform;
            private readonly Text nameTagText;

            public NameTagInfo(NameTagCanvas0 nameTagCanvas, Transform targetTransform)
            {
                this.targetTransform = targetTransform;
                this.nameTagCanvas = nameTagCanvas;
                nameTagRectTrasform = Instantiate(nameTagCanvas.nameTagPrefab, nameTagCanvas.transform);
                nameTagText = nameTagRectTrasform.GetComponentInChildren<Text>();

                nameTagRectTrasform.gameObject.SetActive(false);
            }

            public void LateUpdate()
            {
                if (nameTagText.IsActive())
                {
                    Vector2 screenPoint = nameTagCanvas.camera.WorldToScreenPoint(targetTransform.position + ((Vector3)worldOffset));
                    Vector2 canvasPoint;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(nameTagCanvas.rectTransform, screenPoint, null, out canvasPoint);
                    Rect rect = nameTagCanvas.rectTransform.rect;
                    Vector2 anchorMinAndMax = new Vector2(canvasPoint.x / rect.width + 0.5f, canvasPoint.y / rect.height + 0.5f);
                    nameTagRectTrasform.anchorMin = nameTagRectTrasform.anchorMax = anchorMinAndMax;
                }
            }

            public void Destroy()
            {
                Object.Destroy(nameTagRectTrasform.gameObject);
            }
        }

        public static NameTagCanvas0 instance { get; private set; }

        public RectTransform nameTagPrefab;

        private new Camera camera;
        private RectTransform rectTransform;
        private readonly Dictionary<Transform, NameTagInfo> nameTagInfos = new Dictionary<Transform, NameTagInfo>();
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                Debug.LogError("Found another name tag canvas!");
            }
        }

        private void Start()
        {
            camera = Camera.main;
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
        
        private void LateUpdate()
        {
            foreach (NameTagInfo info in nameTagInfos.Values)
            {
                info.LateUpdate();
            }
        }

        public NameTagInfo AddNameTag(Transform targetTransform)
        {
            NameTagInfo info;
            if (!nameTagInfos.TryGetValue(targetTransform, out info))
            {
                info = new NameTagInfo(this, targetTransform);
                nameTagInfos.Add(targetTransform, info);
            }
            return info;
        }

        public void RemoveNameTag(Transform targetTransform)
        {
            NameTagInfo info;
            if (nameTagInfos.TryGetValue(targetTransform, out info))
            {
                info.Destroy();
                nameTagInfos.Remove(targetTransform);
            }
        }
    }
}
