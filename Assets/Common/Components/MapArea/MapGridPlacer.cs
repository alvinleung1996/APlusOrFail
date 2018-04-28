using System.Collections.Generic;
using UnityEngine;

namespace APlusOrFail.Components
{
    using Maps;

    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class MapGridPlacer : MonoBehaviour
    {
        [SerializeField] private Vector2Int _gridPosition;
        public Vector2Int gridPosition { get { return _gridPosition; } set { SetProperty(ref _gridPosition, value); } }

        [SerializeField] private MapGridRectExtensions.Rotation _rotation;
        public MapGridRectExtensions.Rotation rotation { get { return _rotation; } set { SetProperty(ref _rotation, value); } }

        [SerializeField] private bool _registerInGrid;
        public bool registerInGrid { get { return _registerInGrid; } set { SetProperty(ref _registerInGrid, value); } }


        private bool started;
        private bool registeredInGrid;
        private readonly List<MapGridRect> objectGridRects = new List<MapGridRect>();
        private IEnumerable<RectInt> worldGridRects => objectGridRects.GetLocalRects().Rotate(rotation).Move(gridPosition);
        

        private void OnEnable()
        {
            UpdateProperties();
        }

        private void Start()
        {
            started = true;
            UpdateProperties();
        }

        private void OnValidate()
        {
            UpdateProperties();
        }

        private void OnDestroy()
        {
            if (registeredInGrid)
            {
                MapManager.mapStat?.mapArea?.RemoveFromGrid(gameObject);
                registeredInGrid = false;
            }
        }

        private void SetProperty<T>(ref T property, T value)
        {
            if (!property.Equals(value))
            {
                property = value;
                UpdateProperties();
            }
        }

        public void UpdateProperties()
        {
            if (Application.isPlaying)
            {
                if (started && enabled)
                {
                    if (registeredInGrid)
                    {
                        MapManager.mapStat.mapArea.RemoveFromGrid(gameObject);
                        registeredInGrid = false;
                    }

                    GetComponentsInChildren(true, objectGridRects);
                    if (registerInGrid)
                    {
                        MapManager.mapStat.mapArea.AddToGrid(worldGridRects, gameObject);
                        registeredInGrid = true;
                    }
                    transform.position = MapManager.mapStat.mapArea.GridToWorldPosition(gridPosition);
                    transform.rotation = Quaternion.Euler(0, 0, 90 * (byte)rotation);
                }
            }
            else
            {
                MapArea objectGrid = FindObjectOfType<MapArea>();
                if (objectGrid != null)
                {
                    GetComponentsInChildren(true, objectGridRects);
                    transform.position = objectGrid.GridToWorldPosition(gridPosition);
                    transform.rotation = Quaternion.Euler(0, 0, 90 * (byte)rotation);
                }
            }
        }

        public bool IsRegisterable() => started && enabled && MapManager.mapStat.mapArea.IsPlaceable(worldGridRects);
    }
}
