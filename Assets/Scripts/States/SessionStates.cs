using ScramblyFoxDefense.Config;
using ScramblyFoxDefense.Core;
using ScramblyFoxDefense.Gameplay;
using ScramblyFoxDefense.Presentation;
using UnityEngine;

namespace ScramblyFoxDefense.States
{
    /// <summary>Per-session data shared by the state handlers.</summary>
    public sealed class GameSession
    {
        public int WaveIndex;
    }

    /// <summary>Discover: wave 1 starts on the first build or after the intro timeout.</summary>
    public sealed class IntroState : IGameState
    {
        readonly GameStateMachine _machine;
        readonly GameConfig _config;
        readonly TowerSystem _towers;
        readonly PlayerActions _build;
        readonly HudView _hud;
        float _elapsed;
        bool _startRequested;

        public IntroState(GameStateMachine machine, GameConfig config, TowerSystem towers, PlayerActions build, HudView hud)
        {
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
            _towers.Built += OnBuilt;
        }

        public void Tick(float deltaTime)
        {
            _elapsed += deltaTime;
            if (_startRequested || _elapsed >= _config.introMaxSeconds) _machine.Enter<WaveState>();
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

        public WaveState(GameStateMachine machine, GameConfig config, GameSession session, WaveSpawner spawner, HudView hud, LockBarView locks)
        {
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
            _spawner.Begin(_config.waves[_session.WaveIndex]);
        }

        public void Tick(float deltaTime)
        {
            _spawner.Tick(deltaTime);
            if (!_spawner.Finished) return;

            _locks.Unlock(_session.WaveIndex);
            if (_session.WaveIndex >= _config.waves.Length - 1) _machine.Enter<RedeemState>();
            else _machine.Enter<BreatherState>();
        }

        public void Exit() { }
    }

    /// <summary>Short pause between waves; the next reward lock opens here (later feature).</summary>
    public sealed class BreatherState : IGameState
    {
        readonly GameStateMachine _machine;
        readonly GameConfig _config;
        readonly GameSession _session;
        float _elapsed;

        public BreatherState(GameStateMachine machine, GameConfig config, GameSession session)
        {
            _machine = machine;
            _config = config;
            _session = session;
        }

        public void Enter()
        {
            _elapsed = 0f;
            _session.WaveIndex++;
        }

        public void Tick(float deltaTime)
        {
            _elapsed += deltaTime;
            if (_elapsed >= _config.breatherSeconds) _machine.Enter<WaveState>();
        }

        public void Exit() { }
    }

    /// <summary>Redeem placeholder: the vault sequence arrives in a later feature.</summary>
    public sealed class RedeemState : IGameState
    {
        const float Duration = 2f;

        readonly GameStateMachine _machine;
        readonly PlayerActions _build;
        readonly HudView _hud;
        float _elapsed;

        public RedeemState(GameStateMachine machine, PlayerActions build, HudView hud)
        {
            _machine = machine;
            _build = build;
            _hud = hud;
        }

        public void Enter()
        {
            _elapsed = 0f;
            _build.Enabled = false;
            _build.ClearSelection();
            _hud.SetPhase("Redeem");
            _hud.ShowBanner("Demo rewards unlocked!", Duration);
        }

        public void Tick(float deltaTime)
        {
            _elapsed += deltaTime;
            if (_elapsed >= Duration) _machine.Enter<EndCardState>();
        }

        public void Exit() { }
    }

    /// <summary>End card placeholder: CTA and restart arrive in a later feature.</summary>
    public sealed class EndCardState : IGameState
    {
        readonly Economy _economy;
        readonly HudView _hud;

        public EndCardState(Economy economy, HudView hud)
        {
            _economy = economy;
            _hud = hud;
        }

        public void Enter()
        {
            _hud.ShowBanner(_economy.Leaks == 0 ? "Perfect defense!" : "Nice defense!", float.MaxValue);
            Debug.Log($"[Session] Ended. Collected {_economy.Collected} demo coins, {_economy.Leaks} leaks.");
        }

        public void Tick(float deltaTime) { }

        public void Exit() { }
    }
}
