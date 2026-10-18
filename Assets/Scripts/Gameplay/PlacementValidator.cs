using System.Collections.Generic;
using ScramblyFoxDefense.Config;
using UnityEngine;

namespace ScramblyFoxDefense.Gameplay
{
    /// <summary>
    /// Free placement rules (no scene lookups): inside the arena, clear of the path, of other towers and of the
    /// board decoration, and under the tower cap. Distances come from GameConfig and are measured on XZ.
    /// </summary>
    public sealed class PlacementValidator
    {
        readonly GameConfig _config;
        readonly Rect _arena;
        readonly PathRoute _path;
        readonly IReadOnlyList<Tower> _towers;
        readonly Vector3[] _obstacles;

        public PlacementValidator(GameConfig config, Rect arenaXZ, PathRoute path, IReadOnlyList<Tower> towers, Vector3[] obstacles)
        {
            _config = config;
            _arena = arenaXZ;
            _path = path;
            _towers = towers;
            _obstacles = obstacles;
        }

        public bool HasRoom => _towers.Count < _config.maxTowers;

        public bool IsValid(Vector3 point)
        {
            if (!HasRoom) return false;
            float margin = _config.arenaEdgeMargin;
            if (point.x < _arena.xMin + margin || point.x > _arena.xMax - margin) return false;
            if (point.z < _arena.yMin + margin || point.z > _arena.yMax - margin) return false;
            if (_path.DistanceTo(point) < _config.pathClearance) return false;
            foreach (var tower in _towers)
                if (FlatDistance(tower.Transform.position, point) < _config.towerSpacing) return false;
            foreach (var obstacle in _obstacles)
                if (FlatDistance(obstacle, point) < _config.decorationSpacing) return false;
            return true;
        }

        static float FlatDistance(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }
}
