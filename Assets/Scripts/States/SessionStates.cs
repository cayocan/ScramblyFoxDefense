using ScramblyFoxDefense.Audio;
using ScramblyFoxDefense.Config;
using ScramblyFoxDefense.Core;
using ScramblyFoxDefense.Gameplay;
using ScramblyFoxDefense.Input;
using ScramblyFoxDefense.Presentation;
using UnityEngine;

namespace ScramblyFoxDefense.States
{
    /// <summary>Per-session data shared by the state handlers.</summary>
    public sealed class GameSession
    {
        public int WaveIndex;
        /// <summary>Waves cleared with no predator reaching the vault (one star each).</summary>
        public int PerfectWaves;
        public int WavesCleared;
    }

    /// <summary>Discover: wave 1 starts on the first build or after the intro timeout.</summary>
    public sealed class IntroState : IGameState
    {
        readonly GameStateMachine _machine;
        readonly GameConfig _config;
        readonly TowerSystem _towers;
        readonly PlayerActions _build;
        readonly HudView _hud;
        readonly IAudioService _audio;
        float _elapsed;
        bool _startRequested;

        public IntroState(GameStateMachine machine, GameConfig config, TowerSystem towers, PlayerActions build, HudView hud, IAudioService audio)
        {
            _audio = audio;
            _machine = machine;
            _config = config;
            _towers = towers;
            _build = build;
            _hud = hud;
        }

        public void Enter()
        {
            _elapsed = 0f;
            _startRequested = false;
            _build.Enabled = true;
            _hud.SetPhase("Discover");
            _audio.SetMusicLevel(0); // the page music outlives a restart: back to the calm intro loop
            _towers.Built += OnBuilt;
        }

        public void Tick(float deltaTime)
        {
            _elapsed += deltaTime;
            // The opening tutorial must be completed: wave 1 starts with the first tower, never on a timer.
            if (_startRequested) _machine.Enter<WaveState>();
        }

        public void Exit() => _towers.Built -= OnBuilt;

        void OnBuilt(Tower tower) => _startRequested = true;
    }

    /// <summary>Play: runs the current wave until every predator died or leaked.</summary>
    public sealed class WaveState : IGameState
    {
        readonly GameStateMachine _machine;
        readonly GameConfig _config;
        readonly GameSession _session;
        readonly WaveSpawner _spawner;
        readonly HudView _hud;
        readonly LockBarView _locks;
        readonly IAudioService _audio;
        readonly Economy _economy;
        readonly FoxHealth _fox;
        int _leaksAtStart;

        public WaveState(GameStateMachine machine, GameConfig config, GameSession session, WaveSpawner spawner, HudView hud, LockBarView locks, IAudioService audio, Economy economy, FoxHealth fox)
        {
            _fox = fox;
            _audio = audio;
            _economy = economy;
            _machine = machine;
            _config = config;
            _session = session;
            _spawner = spawner;
            _hud = hud;
            _locks = locks;
        }

        public void Enter()
        {
            bool last = _session.WaveIndex == _config.waves.Length - 1;
            _hud.SetPhase("Play");
            _hud.ShowBanner(last ? "Final wave!" : $"Wave {_session.WaveIndex + 1}", 1.5f);
            _leaksAtStart = _economy.Leaks;
            _spawner.Begin(_config.waves[_session.WaveIndex]);
            _audio.Play(Sound.Wave);
            _audio.SetMusicLevel(_session.WaveIndex + 1);
        }

        public void Tick(float deltaTime)
        {
            if (_fox.Defeated)
            {
                _machine.Enter<EndCardState>();
                return;
            }
            _spawner.Tick(deltaTime);
            if (!_spawner.Finished) return;

            _session.WavesCleared++;
            // Reward that grows with skill: a perfect wave earns a star (gold lock) on top of the clear bonus.
            bool perfect = _economy.Leaks == _leaksAtStart;
            if (perfect) _session.PerfectWaves++;
            var wave = _config.waves[_session.WaveIndex];
            if (wave.clearBonus > 0) _economy.Earn(wave.clearBonus);
            string bonus = wave.clearBonus > 0 ? $"  +{wave.clearBonus}" : "";
            bool last = _session.WaveIndex >= _config.waves.Length - 1;
            // Mid-run the breather follows with the unlock celebration, so the clear banner stays short.
            _hud.ShowBanner(perfect ? $"Perfect wave!{bonus}" : $"Wave cleared{bonus}", last ? 1.6f : BreatherState.ClearBannerSeconds);
            _locks.Unlock(_session.WaveIndex, perfect);
            _audio.Play(Sound.WaveClear);
            if (last) _machine.Enter<RedeemState>();
            else _machine.Enter<BreatherState>();
        }

        public void Exit() { }
    }

    /// <summary>
    /// Pause between waves. Timeline (scaled time): the short clear banner, then the milestone unlock
    /// (next tower card: banner, card slide-in, sparks, screen flash, cue), then a preview of the next wave.
    /// Nothing here outlives the state: restart reloads the scene and a hidden page freezes scaled time.
    /// </summary>
    public sealed class BreatherState : IGameState
    {
        public const float ClearBannerSeconds = 0.8f;
        const float CelebrationSeconds = 1.2f;

        readonly GameStateMachine _machine;
        readonly GameConfig _config;
        readonly GameSession _session;
        readonly HudView _hud;
        readonly TowerUnlocks _unlocks;
        readonly CardBarView _cards;
        readonly SparkFx _sparks;
        readonly FeedbackFx _feedback;
        readonly HudLayout _layout;
        readonly IAudioService _audio;
        float _elapsed;
        bool _celebrated;
        bool _previewShown;

        public BreatherState(GameStateMachine machine, GameConfig config, GameSession session, HudView hud, TowerUnlocks unlocks, CardBarView cards,
            SparkFx sparks, FeedbackFx feedback, HudLayout layout, IAudioService audio)
        {
            _machine = machine;
            _config = config;
            _session = session;
            _hud = hud;
            _unlocks = unlocks;
            _cards = cards;
            _sparks = sparks;
            _feedback = feedback;
            _layout = layout;
            _audio = audio;
        }

        public void Enter()
        {
            _elapsed = 0f;
            _celebrated = false;
            _previewShown = false;
            _session.WaveIndex++;
        }

        public void Tick(float deltaTime)
        {
            _elapsed += deltaTime;
            if (!_celebrated && _elapsed >= ClearBannerSeconds)
            {
                _celebrated = true;
                int card = _unlocks.UnlockNext();
                if (card >= 0)
                {
                    _hud.ShowBanner($"New friend: {_config.towers[card].displayName}!", CelebrationSeconds);
                    _cards.PlayUnlock(card);
                    _sparks.Celebrate(_cards.FacePosition(card), _layout.PixelUnit);
                    _feedback.ScreenFlash();
                    _audio.Play(Sound.Unlock); // jingle; the page music ducks under it
                    Debug.Log($"[Unlock] card={card} {_config.towers[card].displayName} at wave {_session.WaveIndex + 1}");
                }
            }
            // Then tell the player what the next wave brings, so they can prepare.
            float previewAt = ClearBannerSeconds + CelebrationSeconds;
            if (!_previewShown && _elapsed >= previewAt)
            {
                _previewShown = true;
                string intro = _config.waves[_session.WaveIndex].intro;
                if (!string.IsNullOrEmpty(intro)) _hud.ShowBanner(intro, Mathf.Max(0.4f, _config.breatherSeconds - previewAt));
            }
            if (_elapsed >= _config.breatherSeconds) _machine.Enter<WaveState>();
        }

        public void Exit() { }
    }

    /// <summary>Redeem: coins fly into the Reward Vault; a tap speeds it up. Then the end card.</summary>
    public sealed class RedeemState : IGameState
    {
        readonly GameStateMachine _machine;
        readonly PlayerActions _actions;
        readonly HudView _hud;
        readonly RedeemSequence _sequence;
        readonly InputRouter _input;
        readonly CardBarView _cards;
        readonly TowerBadges _badges;
        readonly IAudioService _audio;

        public RedeemState(GameStateMachine machine, PlayerActions actions, HudView hud, RedeemSequence sequence, InputRouter input, CardBarView cards, TowerBadges badges, IAudioService audio)
        {
            _audio = audio;
            _badges = badges;
            _machine = machine;
            _actions = actions;
            _hud = hud;
            _sequence = sequence;
            _input = input;
            _cards = cards;
        }

        public void Enter()
        {
            _actions.Enabled = false;
            _actions.ClearSelection();
            _cards.SetVisible(false);
            _badges.HideAll();
            _hud.SetPhase("Redeem");
            _hud.ShowBanner("Demo rewards unlocked!", 2.5f);
            _sequence.Begin();
            _audio.Play(Sound.Coin);
            _input.Register(OnTap);
        }

        public void Tick(float deltaTime)
        {
            _sequence.Tick(deltaTime);
            if (_sequence.Finished) _machine.Enter<EndCardState>();
        }

        public void Exit() => _input.Unregister(OnTap);

        bool OnTap(Vector2 screenPoint) => _sequence.SpeedUp();
    }

    /// <summary>Invitation: result, rewards, "Explore Scrambly" CTA (demo only) and Play again.</summary>
    public sealed class EndCardState : IGameState
    {
        readonly Economy _economy;
        readonly HudView _hud;
        readonly EndCardView _endCard;
        readonly RestartController _restart;
        readonly InputRouter _input;
        readonly IAudioService _audio;

        readonly GameSession _session;
        readonly int _waveCount;
        readonly FoxHealth _fox;
        readonly EnemySystem _enemies;
        readonly PlayerActions _actions;
        readonly CardBarView _cards;
        readonly TowerBadges _badges;

        public EndCardState(Economy economy, HudView hud, EndCardView endCard, RestartController restart, InputRouter input, IAudioService audio, GameSession session, int waveCount,
            FoxHealth fox, EnemySystem enemies, PlayerActions actions, CardBarView cards, TowerBadges badges)
        {
            _fox = fox;
            _enemies = enemies;
            _actions = actions;
            _cards = cards;
            _badges = badges;
            _session = session;
            _waveCount = waveCount;
            _audio = audio;
            _economy = economy;
            _hud = hud;
            _endCard = endCard;
            _restart = restart;
            _input = input;
        }

        public void Enter()
        {
            _hud.SetPhase("Redeem");
            _hud.HideBanner(); // the end card panel takes over the centre of the screen
            // The fox may fall before the last wave: the board is cleared and the invitation still shows.
            _enemies.ClearAll();
            _actions.Enabled = false;
            _actions.ClearSelection();
            _cards.SetVisible(false);
            _badges.HideAll();

            string coins = $"You collected {_economy.Collected} demo coins";
            if (_fox.Defeated)
            {
                _endCard.Show("So close!", $"{coins}\nWaves cleared: {_session.WavesCleared}/{_waveCount}");
                _audio.Play(Sound.Unlock);
            }
            else
            {
                int stars = _session.PerfectWaves;
                string title = stars == _waveCount ? "Perfect defense!" : stars > 0 ? "Great defense!" : "Nice defense!";
                _endCard.Show(title, $"{coins}\nPerfect waves: {stars}/{_waveCount}");
                _audio.Play(Sound.Fanfare);
            }
            _endCard.CtaClicked += OnCta;
            _endCard.PlayAgainClicked += _restart.Restart;
            _input.Register(_endCard.HandleTap);
        }

        public void Tick(float deltaTime) { }

        public void Exit()
        {
            _endCard.CtaClicked -= OnCta;
            _endCard.PlayAgainClicked -= _restart.Restart;
            _input.Unregister(_endCard.HandleTap);
        }

        // Brief: local confirmation + console log, never navigate.
        void OnCta()
        {
            _audio.Play(Sound.Cta);
            Debug.Log("CTA clicked — demo only");
        }
    }
}
