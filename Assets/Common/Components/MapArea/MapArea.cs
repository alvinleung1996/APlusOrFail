using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace APlusOrFail.Components
{
    using Character;
    using Maps;

    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    [RequireComponent(typeof(BoxCollider2D))]
    public class MapArea : MonoBehaviour
    {
        private class GridObject
        {
            public readonly ReadOnlyCollection<RectInt> rects;
            public readonly GameObject obj;

            public GridObject(IEnumerable<RectInt> rects, GameObject obj)
            {
                this.rects = new ReadOnlyCollection<RectInt>(rects.ToList());
                this.obj = obj;
            }
        }


        [SerializeField] private Vector2 _gridPosition;
        public Vector2 gridPosition { get { return _gridPosition; } set { SetProperty(ref _gridPosition, value); } }

        [SerializeField] private Vector2Int _gridSize;
        public Vector2Int gridSize { get { return _gridSize; } set { SetProperty(ref _gridSize, new Vector2Int(Mathf.Max(value.x, 0), Mathf.Max(value.y, 0))); } }
        
        [SerializeField] private Vector2 _gridCellSize;
        public Vector2 gridCellSize { get { return _gridCellSize; } set { SetProperty(ref _gridCellSize, new Vector2(Mathf.Max(value.x, 0.01f), Mathf.Max(value.y, 0.01f))); } }

        private GridObject[,] grid;
        private readonly Dictionary<GameObject, GridObject> objectToGridMap = new Dictionary<GameObject, GridObject>();

        private BoxCollider2D areaCollider;


        private void Awake()
        {
            if (Application.isPlaying)
            {
                grid = new GridObject[gridSize.x, gridSize.y];
            }
        }

        private void OnEnable()
        {
            ApplyProperties();
        }

        private void Start()
        {
            if (Application.isPlaying)
            {
                areaCollider = GetComponent<BoxCollider2D>();
                ApplyProperties();
            }
        }

        private void OnValidate()
        {
            ApplyProperties();
        }

        private void SetProperty<T>(ref T property, T value)
        {
            if (!property.Equals(value))
            {
                property = value;
                ApplyProperties();
            }
        }

        private void ApplyProperties()
        {
            BoxCollider2D areaCollider = Application.isPlaying ? this.areaCollider : GetComponent<BoxCollider2D>();
            if (areaCollider != null)
            {
                transform.localPosition = gridPosition;
                areaCollider.offset = ((Vector2)gridSize) / 2;
                areaCollider.size = gridSize;
                transform.localScale = new Vector3(gridCellSize.x, gridCellSize.y, 1);
            }
        }


        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.layer == LayerId.Characters)
            {
                CharacterControl charControl = collision.gameObject.GetComponentInParent<CharacterControl>();
                charControl.ChangeHealth(new PlayerHealthChange(PlayerHealthChangeReason.ExitArea, -charControl.health, gameObject));
            }
        }


        public Vector2 WorldToLocalPosition(Vector2 worldPosition) => transform.InverseTransformPoint(worldPosition);
        
        public Vector2Int LocalToGridPosition(Vector2 localPosition) => new Vector2Int(
                Mathf.FloorToInt(localPosition.x / gridCellSize.x),
                Mathf.FloorToInt(localPosition.y / gridCellSize.y)
        );

        public Vector2Int WorldToGridPosition(Vector2 worldPosition) => LocalToGridPosition(WorldToLocalPosition(worldPosition));

        public Vector2 GridToLocalPosition(Vector2Int gridPosition) => new Vector2(
                gridPosition.x * gridCellSize.x,
                gridPosition.y * gridCellSize.y
        );

        public Vector2 LocalToWorldPosition(Vector2 localPosition) => transform.TransformPoint(localPosition);

        public Vector2 GridToWorldPosition(Vector2Int gridPosition) => LocalToWorldPosition(GridToLocalPosition(gridPosition));

        public Vector2 GridToLocalSize(Vector2Int gridSize) => new Vector2(
                gridSize.x * gridCellSize.x,
                gridSize.y * gridCellSize.y
        );

        public Rect GridToLocalRect(RectInt gridRect)
        {
            return new Rect(GridToLocalPosition(gridRect.position), GridToLocalSize(gridRect.size));
        }

        public bool IsPlaceable(IEnumerable<RectInt> gridRects)
        {
            return gridRects.All(r => IsPlaceable(r));
        }

        public bool IsPlaceable(RectInt gridRect)
        {
            if (!IsWithinRange(gridRect))
            {
                return false;
            }
            else
            {
                for (int x = gridRect.xMin; x < gridRect.xMax; ++x)
                {
                    for (int y = gridRect.yMin; y < gridRect.yMax; ++y)
                    {
                        if (grid[x, y] != null)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        public void AddToGrid(IEnumerable<RectInt> gridRects, GameObject obj)
        {
            RectInt? tempRect;
            if ((tempRect = gridRects.Where(r => !IsWithinRange(r)).Select(r => (RectInt?)r).FirstOrDefault()) != null)
            {
                throw new ArgumentOutOfRangeException($"({tempRect.Value}) is not in range");
            }
            if ((tempRect = gridRects.Where(r => !IsPlaceable(r)).Select(r => (RectInt?)r).FirstOrDefault()) != null)
            {
                throw new ArgumentException($"({tempRect.Value}) has already been occupied");
            }
            
            GridObject gridObject = new GridObject(gridRects, obj);

            foreach (RectInt gridRect in gridRects)
            {
                for (int x = gridRect.xMin; x < gridRect.xMax; ++x)
                {
                    for (int y = gridRect.yMin; y < gridRect.yMax; ++y)
                    {
                        grid[x, y] = gridObject;
                    }
                }
            }

            objectToGridMap.Add(obj, gridObject);
        }

        public void RemoveFromGrid(GameObject obj)
        {
            GridObject gridObject;
            if (objectToGridMap.TryGetValue(obj, out gridObject))
            {
                foreach (RectInt objRect in gridObject.rects)
                {
                    for (int i = objRect.xMin; i < objRect.xMax; ++i)
                    {
                        for (int j = objRect.yMin; j < objRect.yMax; ++j)
                        {
                            grid[i, j] = null;
                        }
                    }
                }
                objectToGridMap.Remove(obj);
            }
        }

        public void RemoveFromGrid(IEnumerable<RectInt> gridRects)
        {
            foreach (RectInt gridRect in gridRects)
            {
                for (int x = gridRect.xMin; x < gridRect.xMax; ++x)
                {
                    for (int y = gridRect.yMin; y < gridRect.yMax; ++y)
                    {
                        GridObject gridObject = grid[x, y];
                        if (gridObject != null)
                        {
                            foreach (RectInt objRect in gridObject.rects)
                            {
                                for (int i = objRect.xMin; i < objRect.xMax; ++i)
                                {
                                    for (int j = objRect.yMin; j < objRect.yMax; ++j)
                                    {
                                        grid[i, j] = null;
                                    }
                                }
                            }
                            objectToGridMap.Remove(gridObject.obj);
                        }
                    }
                }
            }
        }

        private bool IsWithinRange(RectInt gridRect)
        {
            return gridRect.xMin >= 0 && gridRect.yMin >= 0 &&
                   gridRect.xMax <= grid.GetLength(0) && gridRect.yMax <= grid.GetLength(1);
        }
    }
}
