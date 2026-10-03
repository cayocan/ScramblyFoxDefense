using ScramblyFoxDefense.Gameplay;

namespace ScramblyFoxDefense.Audio
{
    /// <summary>Maps gameplay events to sounds. States call Play directly for flow cues (waves, redeem, CTA).</summary>
    public sealed class SoundCues
    {
        readonly IAudioService _audio;
        readonly EnemySystem _enemies;
        readonly TowerSystem _towers;

        public SoundCues(IAudioService audio, EnemySystem enemies, TowerSystem towers)
        {
            _audio = audio;
            _enemies = enemies;
            _towers = towers;
            _enemies.Killed += OnKilled;
            _enemies.Leaked += OnLeaked;
            _towers.Built += OnBuilt;
            _towers.Upgraded += OnUpgraded;
            _towers.Fired += OnFired;
        }

        public void Dispose()
        {
            _enemies.Killed -= OnKilled;
            _enemies.Leaked -= OnLeaked;
            _towers.Built -= OnBuilt;
            _towers.Upgraded -= OnUpgraded;
            _towers.Fired -= OnFired;
        }

        void OnKilled(Enemy enemy) => _audio.Play(Sound.Defeat);
        void OnLeaked(Enemy enemy) => _audio.Play(Sound.Leak);
        void OnBuilt(Tower tower) => _audio.Play(Sound.Build);
        void OnUpgraded(Tower tower) => _audio.Play(Sound.Upgrade);
        void OnFired(Tower tower) => _audio.Play(Sound.Shoot);
    }
}
