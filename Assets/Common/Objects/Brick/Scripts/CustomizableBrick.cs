using UnityEngine;

namespace APlusOrFail.Objects
{
    using Components;

    [ExecuteInEditMode]
    public class CustomizableBrick : MonoBehaviour, ICustomizableObject
    {
        [SerializeField] private int _minWidth = 1;
        public int minWidth { get { return _minWidth; } set { SetProperty(ref _minWidth, value); } }

        [SerializeField] private int _maxWidth = int.MaxValue;
        public int maxWidth { get { return _maxWidth; } set { SetProperty(ref _maxWidth, value); } }

        [SerializeField] private int _minHeight = 1;
        public int minHeight { get { return _minHeight; } set { SetProperty(ref _minHeight, value); } }

        [SerializeField] private int _maxHeight = int.MaxValue;
        public int maxHeight { get { return _maxHeight; } set { SetProperty(ref _maxHeight, value); } }

        [SerializeField] private int _width = 1;
        public int width { get { return _width; } set { SetProperty(ref _width, value); } }

        [SerializeField] private int _height = 1;
        public int height { get { return _height; } set { SetProperty(ref _height, value); } }

        [SerializeField] private Color _color = Color.gray;
        public Color color { get { return _color; } set { SetProperty(ref _color, value); } }


        public MapGridRect brickRect;
        public SpriteRenderer brickRenderer;


        private void OnEnable()
        {
            UpdateProperties();
        }

        private void OnValidate()
        {
            UpdateProperties();
        }

        private void SetProperty<T>(ref T property, T value)
        {
            if (!property.Equals(value))
            {
                property = value;
                UpdateProperties();
            }
        }

        private void UpdateProperties()
        {
            _minWidth = Mathf.Max(_minWidth, 1);
            _maxWidth = _maxWidth < 1 ? int.MaxValue : Mathf.Max(_maxWidth, _minWidth);
            _minHeight = Mathf.Max(_minHeight, 1);
            _maxHeight = _maxHeight < 1 ? int.MaxValue : Mathf.Max(_maxHeight, _minHeight);
            _width = Mathf.Clamp(_width, _minWidth, _maxWidth);
            _height = Mathf.Clamp(_height, _minHeight, _maxHeight);

            if (enabled)
            {
                RectInt rect = brickRect.gridLocalRect;
                rect.width = _width;
                rect.height = _height;
                brickRect.gridLocalRect = rect;

                Vector3 scale = brickRect.transform.localScale;
                scale.x = rect.width;
                scale.y = rect.height;
                brickRect.transform.localScale = scale;

                brickRenderer.color = _color;
            }
        }

        bool ICustomizableObject.NextSetting(int option)
        {
            switch (option)
            {
                case 0:
                    float h, s, v;
                    Color.RGBToHSV(color, out h, out s, out v);
                    h = Mathf.Repeat(h + 0.1f, 1);
                    color = Color.HSVToRGB(h, s, v);
                    return false;

                case 1:
                    width = width == maxWidth ? minWidth : (width + 1);
                    return true;

                case 2:
                    height = height == maxHeight ? minHeight : (height + 1);
                    return true;

                default:
                    return false;
            }
        }
    }
}
