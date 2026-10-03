using UnityEngine;

namespace ScramblyFoxDefense.Gameplay
{
    public sealed class Slot
    {
        public Transform Transform;
        public Tower Tower;
        public bool IsFree => Tower == null;
    }

    /// <summary>Build slots, picked by screen distance (no physics in the build).</summary>
    public sealed class SlotManager
    {
        readonly Slot[] _slots;
        readonly Camera _camera;
        readonly float _radiusCssPixels;

        public Slot[] Slots => _slots;

        public SlotManager(Transform[] slotTransforms, Camera camera, float radiusCssPixels)
        {
            _camera = camera;
            _radiusCssPixels = radiusCssPixels;
            _slots = new Slot[slotTransforms.Length];
            for (int i = 0; i < slotTransforms.Length; i++) _slots[i] = new Slot { Transform = slotTransforms[i] };
        }

        /// <summary>Closest slot within the pick radius of the screen point, or null.</summary>
        public Slot Pick(Vector2 screenPoint)
        {
            float radius = _radiusCssPixels * CssToScreenPixels();
            Slot best = null;
            float bestDistance = radius * radius;
            foreach (var slot in _slots)
            {
                Vector3 projected = _camera.WorldToScreenPoint(slot.Transform.position);
                if (projected.z <= 0f) continue;
                float distance = ((Vector2)projected - screenPoint).sqrMagnitude;
                if (distance > bestDistance) continue;
                bestDistance = distance;
                best = slot;
            }
            return best;
        }

        /// <summary>Unity reports Screen.dpi as 96 * devicePixelRatio on the web.</summary>
        static float CssToScreenPixels() => Screen.dpi > 0f ? Screen.dpi / 96f : 1f;
    }
}
