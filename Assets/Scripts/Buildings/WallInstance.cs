using System.Collections.Generic;
using UnityEngine;
using Gorkha.Graphics;
using Gorkha.Audio;

namespace Gorkha.Buildings
{
    /// <summary>
    /// Dynamic 3D Fortification Wall Component (Clash of Clans & Age of Empires style).
    /// Detects orthogonal neighbours (North, South, East, West) and automatically connects
    /// crenellated ramparts, corner bastions, and T-junctions in real time.
    /// </summary>
    [ExecuteAlways]
    public class WallInstance : MonoBehaviour
    {
        public static readonly Dictionary<Vector2Int, WallInstance> AllWalls = new();

        private BuildingInstance _building;
        private GameObject _wallVisualRoot;
        private Vector2Int _currentPos = new Vector2Int(-999, -999);

        public Vector2Int GridPos => _currentPos;

        private void OnEnable()
        {
            _building = GetComponent<BuildingInstance>();
            RegisterAndRefresh();
        }

        private void OnDisable()
        {
            UnregisterAndRefreshNeighbours();
        }

        private void Start()
        {
            RegisterAndRefresh();
        }

        public void RegisterAndRefresh()
        {
            if (_building == null) _building = GetComponent<BuildingInstance>();
            if (_building == null) return;

            Vector2Int newPos = new Vector2Int(_building.GridCol, _building.GridRow);
            if (_currentPos != newPos)
            {
                if (AllWalls.ContainsKey(_currentPos) && AllWalls[_currentPos] == this)
                {
                    AllWalls.Remove(_currentPos);
                }
                _currentPos = newPos;
            }

            AllWalls[_currentPos] = this;

            RebuildVisuals();
            NotifyAdjacentWalls();
        }

        private void UnregisterAndRefreshNeighbours()
        {
            if (AllWalls.ContainsKey(_currentPos) && AllWalls[_currentPos] == this)
            {
                AllWalls.Remove(_currentPos);
            }
            NotifyAdjacentWalls();
        }

        public void RebuildVisuals()
        {
            if (_building == null) _building = GetComponent<BuildingInstance>();

            // Destroy existing wall sub-mesh
            if (_wallVisualRoot != null)
            {
                DestroyImmediate(_wallVisualRoot);
                _wallVisualRoot = null;
            }

            bool north = AllWalls.ContainsKey(_currentPos + Vector2Int.up);
            bool south = AllWalls.ContainsKey(_currentPos + Vector2Int.down);
            bool east = AllWalls.ContainsKey(_currentPos + Vector2Int.right);
            bool west = AllWalls.ContainsKey(_currentPos + Vector2Int.left);

            int level = _building != null ? _building.Level : 1;
            _wallVisualRoot = Nepali3DMeshBuilder.BuildWallSegment(transform, level, north, south, east, west);
            _wallVisualRoot.name = "WallGeometry_3D";
        }

        private void NotifyAdjacentWalls()
        {
            Vector2Int[] offsets = { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left };
            foreach (var off in offsets)
            {
                Vector2Int nPos = _currentPos + off;
                if (AllWalls.TryGetValue(nPos, out WallInstance neighbour) && neighbour != null && neighbour != this)
                {
                    neighbour.RebuildVisuals();
                }
            }
        }
    }
}
