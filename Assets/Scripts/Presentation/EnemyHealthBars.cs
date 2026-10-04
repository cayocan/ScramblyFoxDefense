using System.Collections.Generic;
using ScramblyFoxDefense.Gameplay;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>
    /// Red health bar above every predator. Bars are children of the pooled enemy objects, so they hide and come
    /// back with them; created the first time an enemy object is seen, then reused.
    /// </summary>
    public sealed class EnemyHealthBars
    {
        const float Width = 0.62f;
        const float Height = 0.09f;
        const float Border = 0.022f;
        const float Lift = 0.12f;
        // Rounded sprite has 22 px slice borders: 512 px per unit keeps a thin bar rectangular.
        const float PixelsPerUnit = 512f;

        static readonly Color Back = new Color(0.13f, 0.07f, 0.22f, 0.85f);
        static readonly Color Fill = new Color(0.9f, 0.16f, 0.2f);

        sealed class Bar
        {
            public Transform Root;
            public SpriteRenderer Fill;
            public float Height;
        }

        readonly EnemySystem _enemies;
        readonly Sprite _sprite;
        readonly Transform _camera;
        readonly Dictionary<GameObject, Bar> _bars = new Dictionary<GameObject, Bar>();

        public EnemyHealthBars(EnemySystem enemies, Sprite sprite, Transform camera)
        {
            _enemies = enemies;
            _sprite = sprite;
            _camera = camera;
        }

        public void Tick()
        {
            var rotation = _camera.rotation;
            foreach (var enemy in _enemies.Active)
            {
                if (!_bars.TryGetValue(enemy.GameObject, out var bar))
                {
                    bar = Create(enemy);
                    _bars[enemy.GameObject] = bar;
                }
                bar.Root.position = enemy.Transform.position + Vector3.up * bar.Height;
                bar.Root.rotation = rotation;

                float fraction = Mathf.Clamp01(enemy.Health / Mathf.Max(1f, enemy.Definition.health));
                float inner = Width - 2f * Border;
                bar.Fill.size = new Vector2(Mathf.Max(0.001f, inner * fraction) * PixelsPerUnit, (Height - 2f * Border) * PixelsPerUnit);
                bar.Fill.transform.localPosition = new Vector3(-inner * (1f - fraction) * 0.5f, 0f, -0.001f);
            }
        }

        Bar Create(Enemy enemy)
        {
            // Height from the model's bounds at spawn (models differ: the polar bear is taller).
            float top = 0.6f;
            var renderers = enemy.GameObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                foreach (var r in renderers) bounds.Encapsulate(r.bounds);
                top = bounds.max.y - enemy.Transform.position.y;
            }

            var root = new GameObject("Health Bar").transform;
            root.SetParent(enemy.Transform, false);
            Make("Back", root, Width, Height, Back, -7);
            var fill = Make("Fill", root, Width - 2f * Border, Height - 2f * Border, Fill, -6);
            return new Bar { Root = root, Fill = fill, Height = top + Lift };
        }

        SpriteRenderer Make(string name, Transform parent, float width, float height, Color color, int order)
        {
            var bar = new GameObject(name, typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            bar.transform.SetParent(parent, false);
            bar.transform.localScale = Vector3.one / PixelsPerUnit;
            bar.sprite = _sprite;
            bar.drawMode = SpriteDrawMode.Sliced;
            bar.size = new Vector2(width * PixelsPerUnit, height * PixelsPerUnit);
            bar.color = color;
            bar.sortingOrder = order;
            return bar;
        }
    }
}
