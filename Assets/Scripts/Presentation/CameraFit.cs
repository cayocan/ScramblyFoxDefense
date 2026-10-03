using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>
    /// Keeps the board in view between the HUD bands at any aspect ratio: fixed tilt, the distance
    /// adapts and the camera slides so the board centre sits in the middle of the free area.
    /// </summary>
    public sealed class CameraFit
    {
        // Reserved bands in reference pixels (GDD section 6: ~90 top, ~150 bottom of 844).
        const float TopBand = 70f;
        const float BottomBand = 160f;
        const float ReferenceHeight = 844f;

        readonly Camera _camera;
        readonly Vector3 _target;
        readonly float _halfWidth;
        readonly float _depth;
        float _aspect = -1f;

        public CameraFit(Camera camera, Vector3 target, float boardWidth, float boardDepth)
        {
            _camera = camera;
            _target = target;
            _halfWidth = boardWidth * 0.5f;
            _depth = boardDepth;
        }

        public bool Tick()
        {
            if (Mathf.Approximately(_aspect, _camera.aspect)) return false;
            _aspect = _camera.aspect;

            float tanHalfVertical = Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float freeFraction = 1f - (TopBand + BottomBand) / ReferenceHeight;
            float centreOffset = (BottomBand - TopBand) / ReferenceHeight; // viewport shift of the free area's centre

            float distanceForWidth = _halfWidth / (tanHalfVertical * _aspect);
            // Tilted board seen at ~55 degrees: projected depth is roughly depth * sin(tilt).
            float projectedHalfDepth = _depth * 0.5f * Mathf.Sin(_camera.transform.eulerAngles.x * Mathf.Deg2Rad);
            float distanceForDepth = projectedHalfDepth / (tanHalfVertical * freeFraction);
            float distance = Mathf.Max(distanceForWidth, distanceForDepth);

            var transform = _camera.transform;
            float shift = centreOffset * tanHalfVertical * distance;
            transform.position = _target - transform.forward * distance - transform.up * shift;
            return true;
        }
    }
}
