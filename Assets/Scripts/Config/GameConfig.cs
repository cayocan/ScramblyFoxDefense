using System;
using UnityEngine;

namespace ScramblyFoxDefense.Config
{
    /// <summary>All tunable numbers (GDD sections 3 and 5). Nothing gameplay-related is hard-coded.</summary>
    [CreateAssetMenu(menuName = "Scrambly/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Economy")]
        public int startingCoins = 70;
        public int leakPenalty = 5;

        [Header("Flow (seconds of scaled time)")]
        public float introMaxSeconds = 8f;
        public float breatherSeconds = 3f;
        public float maxDeltaTime = 0.1f;

        [Header("Input")]
        [Tooltip("Slot pick radius in CSS pixels (GDD: 44).")]
        public float pickRadiusCssPixels = 44f;

        [Header("Projectiles")]
        public float projectileSpeed = 9f;

        [Header("Content")]
        public TowerDefinition[] towers = Array.Empty<TowerDefinition>();
        public EnemyDefinition[] enemies = Array.Empty<EnemyDefinition>();
        public WaveDefinition[] waves = Array.Empty<WaveDefinition>();
    }

    [Serializable]
    public sealed class TowerDefinition
    {
        public string displayName;
        public GameObject prefab;
        public TowerLevel[] levels = Array.Empty<TowerLevel>();
    }

    [Serializable]
    public sealed class TowerLevel
    {
        public int cost;
        public float damage;
        public float fireRate;
        public float range;
        [Tooltip("0 = single target.")]
        public float splashRadius;
    }

    [Serializable]
    public sealed class EnemyDefinition
    {
        public string displayName;
        public GameObject prefab;
        public float health;
        public float speed;
        public int coinReward;
    }

    [Serializable]
    public sealed class WaveDefinition
    {
        public SpawnGroup[] groups = Array.Empty<SpawnGroup>();
        public float spawnInterval = 1f;
    }

    [Serializable]
    public sealed class SpawnGroup
    {
        [Tooltip("Index into GameConfig.enemies.")]
        public int enemyIndex;
        public int count;
    }
}
