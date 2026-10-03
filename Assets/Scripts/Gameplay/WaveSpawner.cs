using System.Collections.Generic;
using ScramblyFoxDefense.Config;

namespace ScramblyFoxDefense.Gameplay
{
    /// <summary>Spawns one wave on a fixed interval. A wave ends when everything spawned died or leaked.</summary>
    public sealed class WaveSpawner
    {
        readonly EnemySystem _enemies;
        readonly List<int> _sequence = new List<int>();
        float _interval;
        float _timer;
        int _next;

        public WaveSpawner(EnemySystem enemies)
        {
            _enemies = enemies;
        }

        public bool Finished => _next >= _sequence.Count && _enemies.Active.Count == 0;

        public void Begin(WaveDefinition wave)
        {
            _sequence.Clear();
            BuildInterleaved(wave.groups, _sequence);
            _interval = wave.spawnInterval;
            _timer = 0f;
            _next = 0;
        }

        public void Tick(float deltaTime)
        {
            if (_next >= _sequence.Count) return;
            _timer -= deltaTime;
            if (_timer > 0f) return;
            _enemies.Spawn(_sequence[_next++]);
            _timer += _interval;
        }

        /// <summary>Round-robin across groups so mixed waves alternate enemy types.</summary>
        static void BuildInterleaved(SpawnGroup[] groups, List<int> output)
        {
            var remaining = new int[groups.Length];
            for (int i = 0; i < groups.Length; i++) remaining[i] = groups[i].count;
            bool added = true;
            while (added)
            {
                added = false;
                for (int i = 0; i < groups.Length; i++)
                {
                    if (remaining[i] <= 0) continue;
                    output.Add(groups[i].enemyIndex);
                    remaining[i]--;
                    added = true;
                }
            }
        }
    }
}
