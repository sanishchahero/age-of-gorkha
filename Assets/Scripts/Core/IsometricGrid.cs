using System.Collections.Generic;
using UnityEngine;

namespace Gorkha.Core
{
    /// <summary>
    /// Manages the discrete 2D grid projected onto the 3D X-Z terrain plane.
    /// Handles grid-to-world and world-to-grid coordinate transformations,
    /// building footprint collision tracking, and tile queries.
    /// </summary>
    public class IsometricGrid : MonoBehaviour
    {
        public static IsometricGrid Instance { get; private set; }

        [Header("Grid Configuration")]
        [SerializeField] private int _gridWidth = 24;
        [SerializeField] private int _gridHeight = 24;
        [SerializeField] private float _cellSize = 2.0f; // Each grid cell is 2x2 meters in 3D world
        [SerializeField] private Vector3 _gridOrigin = Vector3.zero;

        // 2D Occupancy matrix: tracks which building (if any) occupies cell (x, y)
        private string[,] _occupancyGrid;

        public int GridWidth => _gridWidth;
        public int GridHeight => _gridHeight;
        public float CellSize => _cellSize;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _occupancyGrid = new string[_gridWidth, _gridHeight];
        }

        /// <summary>
        /// Converts integer grid coordinates (col, row) to world space center (X, Y, Z).
        /// </summary>
        public Vector3 GridToWorldPosition(int col, int row, int width = 1, int height = 1)
        {
            float worldX = _gridOrigin.x + (col + width * 0.5f) * _cellSize;
            float worldZ = _gridOrigin.z + (row + height * 0.5f) * _cellSize;
            return new Vector3(worldX, 0f, worldZ);
        }

        /// <summary>
        /// Converts a 3D world point to the nearest discrete integer grid coordinate.
        /// </summary>
        public Vector2Int WorldToGridCoordinate(Vector3 worldPos)
        {
            int col = Mathf.FloorToInt((worldPos.x - _gridOrigin.x) / _cellSize);
            int row = Mathf.FloorToInt((worldPos.z - _gridOrigin.z) / _cellSize);
            return new Vector2Int(col, row);
        }

        /// <summary>
        /// Checks whether the area spanning (col, row) to (col + width, row + height)
        /// is fully inside the map boundary and completely unoccupied.
        /// </summary>
        public bool CanPlaceAt(int col, int row, int width, int height, string ignoreBuildingId = null)
        {
            if (col < 0 || row < 0 || col + width > _gridWidth || row + height > _gridHeight)
            {
                return false;
            }

            for (int x = col; x < col + width; x++)
            {
                for (int y = row; y < row + height; y++)
                {
                    string occupiedBy = _occupancyGrid[x, y];
                    if (!string.IsNullOrEmpty(occupiedBy) && occupiedBy != ignoreBuildingId)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Registers a building's footprint on the occupancy grid.
        /// </summary>
        public void OccupyArea(string buildingId, int col, int row, int width, int height)
        {
            for (int x = col; x < col + width; x++)
            {
                for (int y = row; y < row + height; y++)
                {
                    if (x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight)
                    {
                        _occupancyGrid[x, y] = buildingId;
                    }
                }
            }
        }

        /// <summary>
        /// Clears a building's footprint from the occupancy grid.
        /// </summary>
        public void ClearArea(int col, int row, int width, int height)
        {
            for (int x = col; x < col + width; x++)
            {
                for (int y = row; y < row + height; y++)
                {
                    if (x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight)
                    {
                        _occupancyGrid[x, y] = null;
                    }
                }
            }
        }

        /// <summary>
        /// Alias for ClearArea to free up territory when structures or obstacles are moved/depleted.
        /// </summary>
        public void FreeArea(int col, int row, int width, int height) => ClearArea(col, row, width, height);

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.25f);
            for (int x = 0; x <= _gridWidth; x++)
            {
                Vector3 start = _gridOrigin + new Vector3(x * _cellSize, 0.05f, 0);
                Vector3 end = _gridOrigin + new Vector3(x * _cellSize, 0.05f, _gridHeight * _cellSize);
                Gizmos.DrawLine(start, end);
            }
            for (int y = 0; y <= _gridHeight; y++)
            {
                Vector3 start = _gridOrigin + new Vector3(0, 0.05f, y * _cellSize);
                Vector3 end = _gridOrigin + new Vector3(_gridWidth * _cellSize, 0.05f, y * _cellSize);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
