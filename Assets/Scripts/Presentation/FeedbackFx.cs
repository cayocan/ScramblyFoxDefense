using System.Collections.Generic;
using ScramblyFoxDefense.Core;
using ScramblyFoxDefense.Gameplay;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>
    /// Hit and progress feedback (GDD section 4): white flash on hit, a soft "poof" when a predator is
    /// defeated, and the pet cheers when its tower is built or upgraded. Pooled, scaled time only.
    /// No ParticleSystem: its module would add native code to the WebGL build.
    /// </summary>
    public sealed class FeedbackFx
    {
        const float FlashTime = 0.09f;
        const float PoofTime = 0.3f;

        sealed class Flash { public Renderer[] Renderers; public float Time; }
        sealed class Poof { public GameObject GameObject; public float Time; public float Size; public Color Color; }

        readonly EnemySystem _enemies;
        readonly TowerSystem _towers;
        readonly ObjectPool _poofPool;
        readonly Transform _camera;
        readonly Dictionary<Enemy, Flash> _flashes = new Dictionary<Enemy, Flash>();
        readonly List<Enemy> _flashDone = new List<Enemy>();
        readonly List<Poof> _poofs = new List<Poof>();

        public FeedbackFx(EnemySystem enemies, TowerSystem towers, GameObject poofPrefab, Transform root, Transform camera)
        {
            _enemies = enemies;
            _towers = towers;
            _camera = camera;
            _poofPool = new ObjectPool(poofPrefab, root, 6);
            _enemies.Hit += OnHit;
            _enemies.Killed += OnKilled;
            _towers.Built += Cheer;
            _towers.Upgraded += Cheer;
            _towers.Splashed += OnSplashed;
        }

        public void Dispose()
        {
            _enemies.Hit -= OnHit;
            _enemies.Killed -= OnKilled;
            _towers.Built -= Cheer;
            _towers.Upgraded -= Cheer;
            _towers.Splashed -= OnSplashed;
        }

        public void Tick(float deltaTime)
        {
            foreach (var pair in _flashes)
            {
                var flash = pair.Value;
                flash.Time -= deltaTime;
                if (flash.Time > 0f) continue;
                foreach (var renderer in flash.Renderers) Tint.Flash(renderer, 0f);
                _flashDone.Add(pair.Key);
            }
            foreach (var enemy in _flashDone) _flashes.Remove(enemy);
            _flashDone.Clear();

            for (int i = _poofs.Count - 1; i >= 0; i--)
            {
                var poof = _poofs[i];
                poof.Time += deltaTime;
                float t = poof.Time / PoofTime;
                if (t >= 1f)
                {
                    _poofPool.Release(poof.GameObject);
                    _poofs.RemoveAt(i);
                    continue;
                }
                var transform = poof.GameObject.transform;
                transform.rotation = _camera.rotation;
                transform.localScale = Vector3.one * poof.Size * Mathf.Lerp(0.25f, 1f, 1f - (1f - t) * (1f - t));
                var sprite = poof.GameObject.GetComponent<SpriteRenderer>();
                var color = poof.Color;
                color.a *= 1f - t;
                sprite.color = color;
            }
        }

        void OnHit(Enemy enemy)
        {
            if (!_flashes.TryGetValue(enemy, out var flash))
            {
                flash = new Flash { Renderers = enemy.GameObject.GetComponentsInChildren<Renderer>() };
                _flashes[enemy] = flash;
            }
            flash.Time = FlashTime;
            foreach (var renderer in flash.Renderers) Tint.Flash(renderer, 0.85f);
        }

        void OnKilled(Enemy enemy)
        {
            // The enemy goes back to its pool now: clear any running flash so it respawns clean.
            if (_flashes.TryGetValue(enemy, out var flash))
            {
                foreach (var renderer in flash.Renderers) Tint.Flash(renderer, 0f);
                _flashes.Remove(enemy);
            }
            Spawn(enemy.Transform.position + Vector3.up * 0.3f, 0.016f, new Color(1f, 0.96f, 0.91f));
        }

        /// <summary>Area hit (Puzzle Pulse): a purple ring the size of the blast.</summary>
        void OnSplashed(Vector3 center, float radius)
        {
            // Poof sprite is 64 px at 1 px/unit: diameter = 2 * radius world units.
            Spawn(center + Vector3.up * 0.1f, radius * 2f / 64f, new Color(0.47f, 0.27f, 0.85f, 0.8f));
        }

        void Spawn(Vector3 position, float size, Color color)
        {
            var go = _poofPool.Get();
            go.transform.position = position;
            // Sprites sort by order before distance: keep world effects under every HUD element.
            go.GetComponent<SpriteRenderer>().sortingOrder = -10;
            _poofs.Add(new Poof { GameObject = go, Size = size, Color = color });
        }

        static void Cheer(Tower tower)
        {
            var animation = tower.PetAnimation;
            if (animation == null || animation.GetClip("gesture-positive") == null) return;
            animation["gesture-positive"].wrapMode = WrapMode.Once;
            animation.CrossFade("gesture-positive", 0.1f);
            animation.CrossFadeQueued("idle", 0.15f);
        }
    }
}
