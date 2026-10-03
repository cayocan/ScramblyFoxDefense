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
