using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace APlusOrFail.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class AutoResizeCamera : MonoBehaviour
    {
        [SerializeField] private Rect _defaultInnerArea;
        public Rect defaultInnerArea { get { return _defaultInnerArea; } set { SetProperty(ref _defaultInnerArea, value); } }

        [SerializeField] private bool _mapAreaAsDefault;
        public bool mapAreaAsDefault { get { return _mapAreaAsDefault; } set { SetProperty(ref _mapAreaAsDefault, value); } }

        [SerializeField] private MapArea _mapArea;
        public MapArea mapArea { get { return _mapArea; } set { SetProperty(ref _mapArea, value); } }

        [SerializeField] private Vector2 _minInnerSize; 
        public Vector2 innerSize { get { return _minInnerSize; } set { SetProperty(ref _minInnerSize, value); } }

        [SerializeField] private RectOffset _padding;
        public RectOffset padding { get { return _padding; } set { SetProperty(ref _padding, value); } }

        [Range(0, float.MaxValue)]
        [SerializeField] private float _lerpScale = 1;
        public float lerpScale { get { return _lerpScale; } set { SetProperty(ref _lerpScale, value); } }


        private new Camera camera;
        private readonly Dictionary<Transform, Rect> charTransforms = new Dictionary<Transform, Rect>();


        private void SetProperty<T>(ref T property, T value)
        {
            if (!property.Equals(value))
            {
                property = value;
            }
        }

        private void Awake()
        {
            camera = GetComponent<Camera>();
        }


        public void Trace(GameObject gameObject, Rect bound = new Rect())
        {
            charTransforms.Add(gameObject.transform, bound);
        }

        public void Untrace(GameObject gameObject)
        {
            charTransforms.Remove(gameObject.transform);
        }

        public void UntraceAll()
        {
            charTransforms.Clear();
        }


        private void LateUpdate()
        {
            UpdateCamera(Application.isPlaying);
        }

        private void UpdateCamera(bool lerp = true)
        {
            Rect area;
            if (charTransforms.Count > 0) {
                area = charTransforms
                    .Select(p => new Rect((Vector2)p.Key.position + p.Value.position, p.Value.size))
                    .Aggregate((ia, b) => {
                        return new Rect
                        {
                            xMin = Mathf.Min(ia.xMin, b.xMin),
                            xMax = Mathf.Max(ia.xMax, b.xMax),
                            yMin = Mathf.Min(ia.yMin, b.yMin),
                            yMax = Mathf.Max(ia.yMax, b.yMax)
                        };
                    });
            }
            else if (mapArea != null && mapAreaAsDefault)
            {
                Vector2 p1 = mapArea.GridToWorldPosition(new Vector2Int(0, 0));
                Vector2 p2 = mapArea.GridToWorldPosition(new Vector2Int(0, mapArea.gridSize.y));
                Vector2 p3 = mapArea.GridToWorldPosition(new Vector2Int(mapArea.gridSize.x, 0));
                Vector2 p4 = mapArea.GridToWorldPosition(mapArea.gridSize);
                area = new Rect
                {
                    xMin = Min(p1.x, p2.x, p3.x, p4.x),
                    xMax = Max(p1.x, p2.x, p3.x, p4.x),
                    yMin = Min(p1.y, p2.y, p3.y, p4.y),
                    yMax = Max(p1.y, p2.y, p3.y, p4.y)
                };
            }
            else
            {
                area = defaultInnerArea;
            }
            Vector2 expand = new Vector2(Mathf.Max(innerSize.x - area.width, 0), Mathf.Max(innerSize.y - area.height, 0));
            area = new Rect(area.position - expand / 2, area.size + expand);
            area = new Rect(area.position - new Vector2(padding.left, padding.bottom), area.size + new Vector2(padding.horizontal, padding.vertical));


            camera.transform.position = Vector3.Lerp(
                camera.transform.position,
                (Vector3)area.center + Vector3.forward * camera.transform.position.z,
                lerp ? (Time.deltaTime * lerpScale) : 1
            );


            float areaAspect = area.width / area.height;
            float cameraAspect = camera.aspect;
            float cameraSize;
            if (areaAspect >= cameraAspect)
            {
                cameraSize = (area.width / cameraAspect) / 2;
            }
            else
            {
                cameraSize = area.height / 2;
            }
            camera.orthographicSize = Mathf.Lerp(
                camera.orthographicSize,
                cameraSize,
                lerp ? (Time.deltaTime * lerpScale) : 1
            );
        }

        private float Min(float a, float b, float c, float d) => Mathf.Min(Mathf.Min(a, b), Mathf.Min(c, d));
        private float Max(float a, float b, float c, float d) => Mathf.Max(Mathf.Max(a, b), Mathf.Max(c, d));
    }
}
