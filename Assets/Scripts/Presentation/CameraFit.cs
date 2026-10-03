using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>Keeps the whole board width in view at any aspect ratio (fixed tilt, distance adapts).</summary>
    public sealed class CameraFit
    {
        readonly Camera _camera;
        readonly Vector3 _target;
        readonly float _halfWidth;
        readonly float _halfDepth;
        float _aspect = -1f;

        public CameraFit(Camera camera, Vector3 target, float boardWidth, float boardDepth)
        {
            _camera = camera;
            _target = target;
            _halfWidth = boardWidth * 0.5f;
            _halfDepth = boardDepth * 0.5f;
        }

        public bool Tick()
        {
            if (Mathf.Approximately(_aspect, _camera.aspect)) return false;
            _aspect = _camera.aspect;

            float halfVertical = _camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            float halfHorizontal = Mathf.Atan(Mathf.Tan(halfVertical) * _aspect);
            float distanceForWidth = _halfWidth / Mathf.Tan(halfHorizontal);
            // Rough vertical fit for the tilted board; the HUD bands sit above and below it.
            float distanceForDepth = _halfDepth * 1.25f / Mathf.Tan(halfVertical);
            float distance = Mathf.Max(distanceForWidth, distanceForDepth);

            var transform = _camera.transform;
            transform.position = _target - transform.forward * distance;
            return true;
        }
    }
}
