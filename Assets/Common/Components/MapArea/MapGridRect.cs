using UnityEngine;

namespace APlusOrFail.Components
{
    public class MapGridRect : MonoBehaviour
    {
        [SerializeField] private RectInt _gridLocalRect;
        public RectInt gridLocalRect { get { return _gridLocalRect; } set { SetProperty(ref _gridLocalRect, value); } }

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
            if (Application.isPlaying)
            {
                MapGridPlacer placer = GetComponentInParent<MapGridPlacer>();
                placer?.UpdateProperties();
            }
        }
    }
}
