using UnityEngine;

namespace APlusOrFail.Decroations
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class Tile : MonoBehaviour {
        
        [SerializeField] private int _width = 1;
        public int width { get { return _width; } set { SetProperty(ref _width, value); } }

        [SerializeField] private int _height = 1;
        public int height { get { return _height; } set { SetProperty(ref _height, value); } }


        public Transform tileHolder;


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
            _width = Mathf.Max(_width, 1);
            _height = Mathf.Max(_height, 1);

            if (enabled)
            {
                Vector3 scale = tileHolder.transform.localScale;
                scale.x = _width;
                scale.y = _height;
                tileHolder.transform.localScale = scale;
            }
        }
    }
}
