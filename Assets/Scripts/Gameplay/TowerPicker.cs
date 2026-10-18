using UnityEngine;

namespace ScramblyFoxDefense.Gameplay
{
    /// <summary>Screen-space picking (no physics in the build): built towers for upgrade taps, and screen to ground.</summary>
    public sealed class TowerPicker
    {
        readonly TowerSystem _towers;
        readonly Camera _camera;
        readonly float _radiusCssPixels;
        readonly Plane _ground;

        public TowerPicker(TowerSystem towers, Camera camera, float radiusCssPixels, float groundHeight)
        {
            _towers = towers;
            _camera = camera;
            _radiusCssPixels = radiusCssPixels;
            _ground = new Plane(Vector3.up, new Vector3(0f, groundHeight, 0f));
        }

        /// <summary>Closest tower within the pick radius of the screen point, or null.</summary>
        public Tower Pick(Vector2 screenPoint)
        {
            float radius = _radiusCssPixels * CssToScreenPixels();
            Tower best = null;
            float bestDistance = radius * radius;
            foreach (var tower in _towers.Towers)
            {
                Vector3 projected = _camera.WorldToScreenPoint(tower.Transform.position);
                if (projected.z <= 0f) continue;
                float distance = ((Vector2)projected - screenPoint).sqrMagnitude;
                if (distance > bestDistance) continue;
                bestDistance = distance;
                best = tower;
            }
            return best;
        }

        /// <summary>Where the screen point meets the board, or null when it misses (above the horizon).</summary>
        public Vector3? Ground(Vector2 screenPoint)
        {
            var ray = _camera.ScreenPointToRay(screenPoint);
            return _ground.Raycast(ray, out float enter) ? ray.GetPoint(enter) : (Vector3?)null;
        }

        /// <summary>Unity reports Screen.dpi as 96 * devicePixelRatio on the web.</summary>
        public static float CssToScreenPixels() => Screen.dpi > 0f ? Screen.dpi / 96f : 1f;
    }
}
