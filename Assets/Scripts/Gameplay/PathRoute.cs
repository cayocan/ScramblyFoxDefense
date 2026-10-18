using UnityEngine;

namespace ScramblyFoxDefense.Gameplay
{
    /// <summary>Polyline the predators follow, sampled by travelled distance.</summary>
    public sealed class PathRoute
    {
        readonly Vector3[] _points;
        readonly float[] _cumulative;

        public float Length { get; }

        public PathRoute(Transform[] waypoints)
        {
            _points = new Vector3[waypoints.Length];
            _cumulative = new float[waypoints.Length];
            for (int i = 0; i < waypoints.Length; i++)
            {
                _points[i] = waypoints[i].position;
                if (i > 0) _cumulative[i] = _cumulative[i - 1] + Vector3.Distance(_points[i - 1], _points[i]);
            }
            Length = _cumulative[_cumulative.Length - 1];
        }

        /// <summary>Distance on the ground plane (XZ) from a point to the nearest path segment.</summary>
        public float DistanceTo(Vector3 point)
        {
            var p = new Vector2(point.x, point.z);
            float best = float.MaxValue;
            for (int i = 1; i < _points.Length; i++)
            {
                var a = new Vector2(_points[i - 1].x, _points[i - 1].z);
                var b = new Vector2(_points[i].x, _points[i].z);
                var ab = b - a;
                float t = ab.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
                best = Mathf.Min(best, Vector2.Distance(p, a + ab * t));
            }
            return best;
        }

        public Vector3 Evaluate(float distance, out Vector3 direction)
        {
            distance = Mathf.Clamp(distance, 0f, Length);
            int i = 1;
            while (i < _points.Length - 1 && _cumulative[i] < distance) i++;
            float segment = _cumulative[i] - _cumulative[i - 1];
            float t = segment > 0f ? (distance - _cumulative[i - 1]) / segment : 0f;
            direction = (_points[i] - _points[i - 1]).normalized;
            return Vector3.Lerp(_points[i - 1], _points[i], t);
        }
    }
}
