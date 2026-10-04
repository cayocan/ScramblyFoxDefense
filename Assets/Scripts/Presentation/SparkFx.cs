using System.Collections.Generic;
using ScramblyFoxDefense.Gameplay;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>
    /// Lightweight particles: pooled camera-facing sprites with velocity, gravity, shrink and fade. Used for the
    /// muzzle flash when a tower fires and the spark burst when a shot hits. Not Unity's ParticleSystem: its
    /// module would add native code to the WebGL build. Scaled time only, so it pauses with the page.
    /// </summary>
    public sealed class SparkFx
    {
        const int PoolSize = 160;
        const float Gravity = -6f;
        // Bright warm core: white would vanish on the cream ground.
        static readonly Color Core = new Color(1f, 0.92f, 0.45f);

        struct Particle
        {
            public Vector3 Velocity;
            public float Age, Life, Size;
            public Color Color;
        }

        readonly TowerSystem _towers;
        readonly Transform _camera;
        readonly SpriteRenderer[] _sprites;
        readonly Particle[] _particles;
        readonly Stack<int> _free = new Stack<int>();
        readonly List<int> _live = new List<int>();
        readonly System.Random _random = new System.Random(3);

        public SparkFx(TowerSystem towers, Sprite spark, Transform root, Transform camera)
        {
            _towers = towers;
            _camera = camera;
            _sprites = new SpriteRenderer[PoolSize];
            _particles = new Particle[PoolSize];
            for (int i = PoolSize - 1; i >= 0; i--)
            {
                var sprite = new GameObject("Spark", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
                sprite.transform.SetParent(root, false);
                sprite.sprite = spark;
                sprite.sortingOrder = -9; // over other world effects, under every HUD element
                sprite.gameObject.SetActive(false);
                _sprites[i] = sprite;
                _free.Push(i);
            }
            _towers.Shot += OnShot;
            _towers.Impact += OnImpact;
        }

        public void Dispose()
        {
            _towers.Shot -= OnShot;
            _towers.Impact -= OnImpact;
        }

        public void Tick(float deltaTime)
        {
            var rotation = _camera.rotation;
            for (int n = _live.Count - 1; n >= 0; n--)
            {
                int i = _live[n];
                ref var p = ref _particles[i];
                p.Age += deltaTime;
                var sprite = _sprites[i];
                if (p.Age >= p.Life)
                {
                    sprite.gameObject.SetActive(false);
                    _live.RemoveAt(n);
                    _free.Push(i);
                    continue;
                }
                p.Velocity.y += Gravity * deltaTime;
                var transform = sprite.transform;
                transform.position += p.Velocity * deltaTime;
                transform.rotation = rotation;
                float t = p.Age / p.Life;
                transform.localScale = Vector3.one * p.Size * (1f - t * t);
                var color = p.Color;
                color.a *= 1f - t * t; // stays bright, fades at the end
                sprite.color = color;
            }
        }

        /// <summary>Muzzle flash: a tight cone of sparks toward the target in the shot's colour.</summary>
        void OnShot(Vector3 muzzle, Vector3 direction, Color color)
        {
            Burst(muzzle, color, 7, 1.6f, 0.26f, 0.55f, direction * 2f);
            Burst(muzzle, Core, 3, 0.8f, 0.18f, 0.7f, Vector3.zero);
        }

        /// <summary>Hit: a bright burst of sparks bouncing off the predator.</summary>
        void OnImpact(Vector3 position, Color color)
        {
            Burst(position, color, 12, 3f, 0.45f, 0.6f, Vector3.up * 1.4f);
            Burst(position, Core, 5, 2f, 0.3f, 0.5f, Vector3.up * 1f);
        }

        void Burst(Vector3 position, Color color, int count, float speed, float life, float size, Vector3 bias)
        {
            for (int k = 0; k < count && _free.Count > 0; k++)
            {
                int i = _free.Pop();
                var random = RandomDirection() * speed * (0.5f + (float)_random.NextDouble() * 0.5f);
                _particles[i] = new Particle
                {
                    Velocity = random + bias,
                    Age = 0f,
                    Life = life * (0.7f + (float)_random.NextDouble() * 0.6f),
                    // Sprite is 64 px at 1 px/unit.
                    Size = size / 64f * (0.7f + (float)_random.NextDouble() * 0.6f),
                    Color = color
                };
                var sprite = _sprites[i];
                sprite.transform.position = position;
                sprite.gameObject.SetActive(true);
                _live.Add(i);
            }
        }

        Vector3 RandomDirection()
        {
            var v = new Vector3((float)_random.NextDouble() * 2f - 1f, (float)_random.NextDouble() * 2f - 1f, (float)_random.NextDouble() * 2f - 1f);
            return v.sqrMagnitude > 1e-4f ? v.normalized : Vector3.up;
        }
    }
}
