using System;
using System.Collections.Generic;
using ScramblyFoxDefense.Config;
using ScramblyFoxDefense.Core;
using UnityEngine;

namespace ScramblyFoxDefense.Gameplay
{
    public sealed class Enemy
    {
        public EnemyDefinition Definition;
        public GameObject GameObject;
        public Transform Transform;
        public Animation Animation;
        public float Health;
        public float Distance;
        public bool Alive;
    }

    /// <summary>Spawns, moves and removes predators. Kills pay coins, leaks cost coins.</summary>
    public sealed class EnemySystem
    {
        readonly GameConfig _config;
        readonly PathRoute _path;
        readonly Economy _economy;
        readonly FoxHealth _fox;
        readonly ObjectPool[] _pools;
        readonly List<Enemy> _active = new List<Enemy>();

        public IReadOnlyList<Enemy> Active => _active;

        public event Action<Enemy> Hit;
        public event Action<Enemy> Killed;
        public event Action<Enemy> Leaked;

        public EnemySystem(GameConfig config, PathRoute path, Economy economy, FoxHealth fox, Transform poolRoot)
        {
            _fox = fox;
            _config = config;
            _path = path;
            _economy = economy;
            _pools = new ObjectPool[config.enemies.Length];
            for (int i = 0; i < _pools.Length; i++) _pools[i] = new ObjectPool(config.enemies[i].prefab, poolRoot, 6);
        }

        public void Spawn(int enemyIndex)
        {
            var definition = _config.enemies[enemyIndex];
            var go = _pools[enemyIndex].Get();
            var enemy = new Enemy
            {
                Definition = definition,
                GameObject = go,
                Transform = go.transform,
                Animation = go.GetComponentInChildren<Animation>(),
                Health = definition.health,
                Distance = 0f,
                Alive = true
            };
            Place(enemy);
            PlayLoop(enemy.Animation, "walk");
            _active.Add(enemy);
        }

        public void Tick(float deltaTime)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var enemy = _active[i];
                enemy.Distance += enemy.Definition.speed * deltaTime;
                if (enemy.Distance >= _path.Length)
                {
                    _economy.RegisterLeak();
                    _fox.Damage(enemy.Definition.foxDamage);
                    Leaked?.Invoke(enemy);
                    Remove(i);
                    continue;
                }
                Place(enemy);
            }
        }

        public void Damage(Enemy enemy, float amount)
        {
            if (!enemy.Alive) return;
            enemy.Health -= amount;
            if (enemy.Health > 0f)
            {
                Hit?.Invoke(enemy);
                return;
            }

            _economy.Earn(enemy.Definition.coinReward);
            Killed?.Invoke(enemy);
            Remove(_active.IndexOf(enemy));
        }

        /// <summary>Session over (the fox fell): every predator leaves the board at once.</summary>
        public void ClearAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--) Remove(i);
        }

        void Remove(int index)
        {
            var enemy = _active[index];
            enemy.Alive = false;
            _active.RemoveAt(index);
            _pools[Array.IndexOf(_config.enemies, enemy.Definition)].Release(enemy.GameObject);
        }

        void Place(Enemy enemy)
        {
            enemy.Transform.position = _path.Evaluate(enemy.Distance, out var direction);
            if (direction.sqrMagnitude > 0f) enemy.Transform.rotation = Quaternion.LookRotation(direction);
        }

        static void PlayLoop(Animation animation, string clip)
        {
            if (animation == null || animation.GetClip(clip) == null) return;
            animation[clip].wrapMode = WrapMode.Loop;
            animation.Play(clip);
        }
    }
}
