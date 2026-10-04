using ScramblyFoxDefense.Gameplay;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>
    /// Health bar floating above the fox on the vault. The fill shrinks from the right, turns from orange to red
    /// when low, and the fox flashes white when hit. Built from the 9-sliced rounded sprite (no uGUI).
    /// </summary>
    public sealed class FoxHealthBar
    {
        const float Width = 1.1f;
        const float Height = 0.16f;
        const float Border = 0.035f;
        const float Lift = 0.95f;
        const float FlashTime = 0.15f;
        // The rounded sprite has 22 px slice borders: draw at 512 px per unit so a 0.16-unit bar is 82 px tall
        // and its corners do not overlap (at 64 px per unit the bar collapsed into an ellipse).
        const float PixelsPerUnit = 512f;

        static readonly Color Back = new Color(0.13f, 0.07f, 0.22f, 0.9f);
        static readonly Color Healthy = new Color32(0xF5, 0x83, 0x24, 0xFF);
        static readonly Color Low = new Color(0.9f, 0.22f, 0.22f);

        readonly FoxHealth _health;
        readonly Transform _fox;
        readonly Transform _camera;
        readonly Transform _root;
        readonly SpriteRenderer _fill;
        readonly Renderer[] _foxRenderers;
        float _flash;

        public FoxHealthBar(FoxHealth health, Transform fox, Sprite rounded, Transform camera)
        {
            _health = health;
            _fox = fox;
            _camera = camera;
            _foxRenderers = fox.GetComponentsInChildren<Renderer>();

            _root = new GameObject("Fox Health Bar").transform;
            _root.SetParent(fox.parent, false);
            var back = Bar("Back", rounded, Width, Height, Back, -7);
            _fill = Bar("Fill", rounded, Width - 2f * Border, Height - 2f * Border, Healthy, -6);
            back.transform.localPosition = Vector3.zero;

            _health.Changed += OnChanged;
            Refresh();
        }

        public void Dispose() => _health.Changed -= OnChanged;

        public void Tick(float deltaTime)
        {
            var bounds = _foxRenderers.Length > 0 ? _foxRenderers[0].bounds : new Bounds(_fox.position, Vector3.zero);
            foreach (var r in _foxRenderers) bounds.Encapsulate(r.bounds);
            _root.position = new Vector3(bounds.center.x, bounds.max.y + Lift * 0.45f, bounds.center.z);
            _root.rotation = _camera.rotation;

            if (_flash <= 0f) return;
            _flash -= deltaTime;
            float amount = Mathf.Max(0f, _flash / FlashTime) * 0.85f;
            foreach (var r in _foxRenderers) Tint.Flash(r, amount);
        }

        void OnChanged()
        {
            _flash = FlashTime;
            Refresh();
        }

        void Refresh()
        {
            float fraction = (float)_health.Current / _health.Max;
            float inner = Width - 2f * Border;
            _fill.size = new Vector2(Mathf.Max(0.001f, inner * fraction) * PixelsPerUnit, (Height - 2f * Border) * PixelsPerUnit);
            _fill.transform.localPosition = new Vector3(-inner * (1f - fraction) * 0.5f, 0f, -0.001f);
            _fill.color = Color.Lerp(Low, Healthy, Mathf.InverseLerp(0.25f, 0.6f, fraction));
            _fill.enabled = _health.Current > 0;
        }

        SpriteRenderer Bar(string name, Sprite sprite, float width, float height, Color color, int order)
        {
            var bar = new GameObject(name, typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            bar.transform.SetParent(_root, false);
            bar.transform.localScale = Vector3.one / PixelsPerUnit;
            bar.sprite = sprite;
            bar.drawMode = SpriteDrawMode.Sliced;
            bar.size = new Vector2(width * PixelsPerUnit, height * PixelsPerUnit);
            bar.color = color;
            bar.sortingOrder = order;
            return bar;
        }
    }
}
