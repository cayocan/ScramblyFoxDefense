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
        public float breatherSeconds = 4f;
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

        [Header("Feel")]
        public TargetMode targeting = TargetMode.First;
        public Color projectileColor = new Color(0.96f, 0.51f, 0.14f);
        public float projectileScale = 1f;
        [Tooltip("0 = GameConfig.projectileSpeed.")]
        public float projectileSpeed;
    }

    /// <summary>First = furthest along the path (stops leaks); Strongest = most health left (sniper).</summary>
    public enum TargetMode { First, Strongest }

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
        [Tooltip("Shown in the breather before this wave: what is new.")]
        public string intro = "";
        [Tooltip("Demo coins paid when the wave is cleared.")]
        public int clearBonus;
    }

    [Serializable]
    public sealed class SpawnGroup
    {
        [Tooltip("Index into GameConfig.enemies.")]
        public int enemyIndex;
        public int count;
    }
}
