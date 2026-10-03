using System;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>
    /// Quad + label button on the HUD plane. Children are laid out in reference pixels; hit-tested in
    /// screen space against its own size (at least 48 x 48 CSS px per GDD section 4).
    /// </summary>
    [Serializable]
    public sealed class HudButton
    {
        public Transform root;
        public Renderer background;
        public TextMesh label;
        public Vector2 sizePixels;

        public bool Visible => root.gameObject.activeInHierarchy;

        public void SetVisible(bool visible) => root.gameObject.SetActive(visible);

        public bool HitTest(Camera camera, Vector2 screenPoint)
        {
            if (!Visible) return false;
            // Touch target grows to 48 px if the drawn button is smaller.
            Vector2 half = new Vector2(Mathf.Max(sizePixels.x, 48f), Mathf.Max(sizePixels.y, 48f)) * 0.5f;
            Vector3 center = camera.WorldToScreenPoint(root.position);
            Vector3 corner = camera.WorldToScreenPoint(root.TransformPoint(new Vector3(half.x, half.y, 0f)));
            return Mathf.Abs(screenPoint.x - center.x) <= Mathf.Abs(corner.x - center.x)
                && Mathf.Abs(screenPoint.y - center.y) <= Mathf.Abs(corner.y - center.y);
        }
    }
}
