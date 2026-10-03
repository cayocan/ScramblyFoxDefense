using System.Collections.Generic;
using ScramblyFoxDefense.Core;
using ScramblyFoxDefense.Gameplay;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>Pooled coin that hops out of a defeated predator (greybox feedback).</summary>
    public sealed class CoinPopFx
    {
        const float Duration = 0.6f;
        const float Height = 0.8f;

        sealed class Pop
        {
            public GameObject GameObject;
            public Vector3 Origin;
            public float Time;
        }

        readonly EnemySystem _enemies;
        readonly ObjectPool _pool;
        readonly List<Pop> _pops = new List<Pop>();

        public CoinPopFx(EnemySystem enemies, GameObject coinPrefab, Transform root)
        {
            _enemies = enemies;
            _pool = new ObjectPool(coinPrefab, root, 8);
            _enemies.Killed += OnKilled;
        }

        public void Dispose() => _enemies.Killed -= OnKilled;

        public void Tick(float deltaTime)
        {
            for (int i = _pops.Count - 1; i >= 0; i--)
            {
                var pop = _pops[i];
                pop.Time += deltaTime;
                float t = pop.Time / Duration;
                if (t >= 1f)
                {
                    _pool.Release(pop.GameObject);
                    _pops.RemoveAt(i);
                    continue;
                }
                var transform = pop.GameObject.transform;
                transform.position = pop.Origin + Vector3.up * (Height * 4f * t * (1f - t) + 0.3f);
                transform.Rotate(0f, 540f * deltaTime, 0f, Space.World);
                transform.localScale = Vector3.one * (1f - t * t);
            }
        }

        void OnKilled(Enemy enemy)
        {
            var go = _pool.Get();
            go.transform.position = enemy.Transform.position;
            _pops.Add(new Pop { GameObject = go, Origin = enemy.Transform.position });
        }
    }
}
