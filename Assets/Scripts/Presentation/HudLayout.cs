using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>
    /// Canvas-Scaler-like placement for camera-parented HUD objects (no uGUI in the build).
    /// Positions are given in reference pixels (390 x 844 portrait) from a viewport anchor.
    /// </summary>
    public sealed class HudLayout
    {
        public const float PlaneDistance = 2f;
        const float ReferenceWidth = 390f;
        const float ReferenceHeight = 844f;
        // TextMesh height in units for fontSize 64 and characterSize 0.01.
        const float TextUnitHeight = 0.064f;

        readonly Camera _camera;
        float _halfWidth;
        float _halfHeight;

        /// <summary>World units per reference pixel at the HUD plane.</summary>
        public float PixelUnit { get; private set; }

        public HudLayout(Camera camera)
        {
            _camera = camera;
            Refresh();
        }

        public void Refresh()
        {
            _halfHeight = Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * PlaneDistance;
            _halfWidth = _halfHeight * _camera.aspect;
            PixelUnit = Mathf.Min(2f * _halfWidth / ReferenceWidth, 2f * _halfHeight / ReferenceHeight);
        }

        /// <summary>Local position for an anchor in viewport space (0..1) plus an offset in reference pixels.</summary>
        public Vector3 Position(Vector2 anchor, Vector2 offsetPixels) => new Vector3(
            (anchor.x - 0.5f) * 2f * _halfWidth + offsetPixels.x * PixelUnit,
            (anchor.y - 0.5f) * 2f * _halfHeight + offsetPixels.y * PixelUnit,
            PlaneDistance);

        public void Place(Transform transform, Vector2 anchor, Vector2 offsetPixels, float sizePixels)
        {
            transform.localPosition = Position(anchor, offsetPixels);
            transform.localScale = Vector3.one * sizePixels * PixelUnit;
        }

        /// <summary>Scale for a TextMesh so its line height is the given reference pixels.</summary>
        public void PlaceText(TextMesh text, Vector2 anchor, Vector2 offsetPixels, float heightPixels)
        {
            text.transform.localPosition = Position(anchor, offsetPixels);
            text.transform.localScale = Vector3.one * heightPixels * PixelUnit / TextUnitHeight;
        }
    }
}
