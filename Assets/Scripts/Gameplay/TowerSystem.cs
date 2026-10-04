using System;
using System.Collections.Generic;
using ScramblyFoxDefense.Config;
using ScramblyFoxDefense.Core;
using UnityEngine;

namespace ScramblyFoxDefense.Gameplay
{
    public sealed class Tower
    {
        public TowerDefinition Definition;
        public Slot Slot;
        public Transform Transform;
        public Transform Pet;
        public Animation PetAnimation;
        public Transform Base;
        public float BaseHeight;
        public int Level;
        public float Cooldown;

        public TowerLevel Stats => Definition.levels[Level];
        public bool IsMaxLevel => Level >= Definition.levels.Length - 1;
        public int NextLevelCost => IsMaxLevel ? 0 : Definition.levels[Level + 1].cost;
    }

    sealed class Projectile
    {
        public GameObject GameObject;
        public Transform Transform;
        public Enemy Target;
        public Vector3 Destination;
        public float Damage;
        public float SplashRadius;
        public float Speed;
    }

    /// <summary>Builds towers on slots, picks targets and fires pooled projectiles.</summary>
    public sealed class TowerSystem
    {
        const float MuzzleHeight = 0.25f;

        readonly GameConfig _config;
        readonly Economy _economy;
        readonly EnemySystem _enemies;
        readonly Transform _towerRoot;
        readonly ObjectPool _projectilePool;
        readonly List<Tower> _towers = new List<Tower>();
        readonly List<Projectile> _projectiles = new List<Projectile>();

        public IReadOnlyList<Tower> Towers => _towers;

        public event Action<Tower> Built;
        public event Action<Tower> Upgraded;
        public event Action<Tower> Fired;
        public event Action<Vector3, float> Splashed;

        public TowerSystem(GameConfig config, Economy economy, EnemySystem enemies, Transform towerRoot, GameObject projectilePrefab)
        {
            _config = config;
            _economy = economy;
            _enemies = enemies;
            _towerRoot = towerRoot;
            _projectilePool = new ObjectPool(projectilePrefab, towerRoot, 16);
        }

        public bool TryBuild(Slot slot, int towerIndex)
        {
            var definition = _config.towers[towerIndex];
            if (!slot.IsFree || !_economy.TrySpend(definition.levels[0].cost)) return false;

            var go = UnityEngine.Object.Instantiate(definition.prefab, slot.Transform.position, Quaternion.identity, _towerRoot);
            var pet = go.transform.Find("Pet");
            var towerBase = go.transform.Find("Base");
            var tower = new Tower
            {
                Definition = definition,
                Slot = slot,
                Transform = go.transform,
                Pet = pet,
                PetAnimation = pet != null ? pet.GetComponentInChildren<Animation>() : null,
                Base = towerBase,
                BaseHeight = pet != null && towerBase != null ? pet.localPosition.y - towerBase.localPosition.y : 0.5f
            };
            slot.Tower = tower;
            _towers.Add(tower);
            Built?.Invoke(tower);
            return true;
        }

        /// <summary>One level up: stacks another base piece and lifts the pet (visible progress, GDD section 4).</summary>
        public bool TryUpgrade(Tower tower)
        {
            if (tower.IsMaxLevel || !_economy.TrySpend(tower.NextLevelCost)) return false;
            tower.Level++;
            if (tower.Base != null)
            {
                var piece = UnityEngine.Object.Instantiate(tower.Base.gameObject, tower.Transform);
                piece.transform.localPosition = tower.Base.localPosition + Vector3.up * (tower.BaseHeight * tower.Level);
                piece.transform.localScale = tower.Base.localScale * (1f - 0.12f * tower.Level);
            }
            if (tower.Pet != null)
            {
                tower.Pet.localPosition += Vector3.up * tower.BaseHeight;
                tower.Pet.localScale *= 1.12f; // the pet grows with its tower
            }
            Upgraded?.Invoke(tower);
            return true;
        }

        public void Tick(float deltaTime)
        {
            foreach (var tower in _towers) TickTower(tower, deltaTime);
            for (int i = _projectiles.Count - 1; i >= 0; i--)
                if (TickProjectile(_projectiles[i], deltaTime)) Release(i);
        }

        void TickTower(Tower tower, float deltaTime)
        {
            tower.Cooldown -= deltaTime;
            var stats = tower.Stats;
            var target = FindTarget(tower.Transform.position, stats.range, tower.Definition.targeting);
            if (target == null) return;

            if (tower.Pet != null)
            {
                Vector3 look = target.Transform.position - tower.Pet.position;
                look.y = 0f;
                if (look.sqrMagnitude > 0f) tower.Pet.rotation = Quaternion.LookRotation(look);
            }
            if (tower.Cooldown > 0f) return;

            tower.Cooldown = 1f / stats.fireRate;
            Fire(tower, target, stats);
            Fired?.Invoke(tower);
        }

        /// <summary>Within range: furthest along the path (First) or most health left (Strongest).</summary>
        Enemy FindTarget(Vector3 origin, float range, TargetMode mode)
        {
            Enemy best = null;
            float rangeSqr = range * range;
            foreach (var enemy in _enemies.Active)
            {
                Vector3 offset = enemy.Transform.position - origin;
                offset.y = 0f;
                if (offset.sqrMagnitude > rangeSqr) continue;
                bool better = mode == TargetMode.Strongest
                    ? best == null || enemy.Health > best.Health || (enemy.Health == best.Health && enemy.Distance > best.Distance)
                    : best == null || enemy.Distance > best.Distance;
                if (better) best = enemy;
            }
            return best;
        }

        void Fire(Tower tower, Enemy target, TowerLevel stats)
        {
            var go = _projectilePool.Get();
            go.transform.position = (tower.Pet != null ? tower.Pet.position : tower.Transform.position) + Vector3.up * MuzzleHeight;
            // Each tower reads differently: own colour and size; upgrades grow the shot.
            var definition = tower.Definition;
            go.transform.localScale = Vector3.one * definition.projectileScale * (1f + 0.25f * tower.Level);
            foreach (var renderer in go.GetComponentsInChildren<Renderer>()) Presentation.Tint.Set(renderer, definition.projectileColor);
            _projectiles.Add(new Projectile
            {
                GameObject = go,
                Transform = go.transform,
                Target = target,
                Destination = target.Transform.position,
                Damage = stats.damage,
                SplashRadius = stats.splashRadius,
                Speed = definition.projectileSpeed > 0f ? definition.projectileSpeed : _config.projectileSpeed
            });
        }

        /// <summary>Homes on the target; if it died mid-flight, finishes at its last position. True when done.</summary>
        bool TickProjectile(Projectile projectile, float deltaTime)
        {
            if (projectile.Target.Alive) projectile.Destination = projectile.Target.Transform.position + Vector3.up * 0.25f;

            Vector3 position = Vector3.MoveTowards(projectile.Transform.position, projectile.Destination, projectile.Speed * deltaTime);
            projectile.Transform.position = position;
            if ((position - projectile.Destination).sqrMagnitude > 0.0025f) return false;

            if (projectile.SplashRadius > 0f)
            {
                Splash(position, projectile.SplashRadius, projectile.Damage);
                Splashed?.Invoke(position, projectile.SplashRadius);
            }
            else if (projectile.Target.Alive) _enemies.Damage(projectile.Target, projectile.Damage);
            return true;
        }

        void Splash(Vector3 center, float radius, float damage)
        {
            float radiusSqr = radius * radius;
            var active = _enemies.Active;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                Vector3 offset = active[i].Transform.position - center;
                offset.y = 0f;
                if (offset.sqrMagnitude <= radiusSqr) _enemies.Damage(active[i], damage);
            }
        }

        void Release(int index)
        {
            _projectilePool.Release(_projectiles[index].GameObject);
            _projectiles.RemoveAt(index);
        }
    }
}
