using ScramblyFoxDefense.Config;
using ScramblyFoxDefense.Gameplay;
using ScramblyFoxDefense.Input;
using ScramblyFoxDefense.Presentation;
using ScramblyFoxDefense.States;
using UnityEngine;

namespace ScramblyFoxDefense.Core
{
    /// <summary>
    /// Composition root (docs/rag/architecture.md): builds every service and state handler with
    /// constructor injection, then forwards scaled time. The only MonoBehaviour with an Update.
    /// Restart reloads the scene, which rebuilds this whole graph from scratch.
    /// </summary>
    public sealed class GameInstaller : MonoBehaviour
    {
        [SerializeField] GameConfig config;
        [SerializeField] Camera mainCamera;
        [SerializeField] Transform[] pathWaypoints;
        [SerializeField] Transform[] slotTransforms;
        [SerializeField] Vector2 boardSize = new Vector2(6f, 11f);
        [SerializeField] Vector3 boardCenter = new Vector3(0f, 0f, -0.45f);

        [Header("Prefabs")]
        [SerializeField] GameObject projectilePrefab;
        [SerializeField] GameObject coinPrefab;
        [SerializeField] GameObject badgePrefab;

        [Header("Scene roots")]
        [SerializeField] Transform enemyRoot;
        [SerializeField] Transform towerRoot;
        [SerializeField] Transform fxRoot;

        [Header("HUD")]
        [SerializeField] TextMesh coinsText;
        [SerializeField] TextMesh phaseText;
        [SerializeField] TextMesh bannerText;
        [SerializeField] CardView[] cards;
        [SerializeField] Renderer[] locks;

        InputRouter _input;
        EnemySystem _enemies;
        TowerSystem _towers;
        PlayerActions _actions;
        CoinPopFx _coinFx;
        HudLayout _layout;
        HudView _hud;
        CardBarView _cardBar;
        LockBarView _lockBar;
        TowerBadges _badges;
        SlotHighlighter _slotHighlighter;
        CameraFit _cameraFit;
        GameStateMachine _machine;

        void Awake()
        {
            Time.timeScale = 1f;
            Time.maximumDeltaTime = config.maxDeltaTime;

            var economy = new Economy(config.startingCoins);
            var path = new PathRoute(pathWaypoints);
            var session = new GameSession();

            _input = new InputRouter();
            _enemies = new EnemySystem(config, path, economy, enemyRoot);
            var spawner = new WaveSpawner(_enemies);
            _towers = new TowerSystem(config, economy, _enemies, towerRoot, projectilePrefab);
            var slots = new SlotManager(slotTransforms, mainCamera, config.pickRadiusCssPixels);

            _layout = new HudLayout(mainCamera);
            _hud = new HudView(_layout, economy, coinsText, phaseText, bannerText);
            _cardBar = new CardBarView(cards, config, economy, _layout, mainCamera);
            _lockBar = new LockBarView(locks, _layout);
            _actions = new PlayerActions(config, _input, _cardBar, slots, _towers, economy);
            _badges = new TowerBadges(_towers, economy, badgePrefab, mainCamera.transform);
            _slotHighlighter = new SlotHighlighter(slots, _actions);
            _coinFx = new CoinPopFx(_enemies, coinPrefab, fxRoot);
            _cameraFit = new CameraFit(mainCamera, boardCenter, boardSize.x, boardSize.y);

            _machine = new GameStateMachine();
            _machine.Register(new IntroState(_machine, config, _towers, _actions, _hud));
            _machine.Register(new WaveState(_machine, config, session, spawner, _hud, _lockBar));
            _machine.Register(new BreatherState(_machine, config, session));
            _machine.Register(new RedeemState(_machine, _actions, _hud));
            _machine.Register(new EndCardState(economy, _hud));
        }

        void Start() => _machine.Enter<IntroState>();

        void Update()
        {
            float deltaTime = Time.deltaTime;
            if (_cameraFit.Tick())
            {
                _layout.Refresh();
                _hud.Layout();
                _cardBar.Layout();
                _lockBar.Layout();
            }

            _input.Tick();
            _machine.Tick(deltaTime);
            _enemies.Tick(deltaTime);
            _towers.Tick(deltaTime);
            _coinFx.Tick(deltaTime);
            _hud.Tick(deltaTime);
            _cardBar.Tick(deltaTime);
            _lockBar.Tick(deltaTime);
            _slotHighlighter.Tick(deltaTime);
        }

        void OnDestroy()
        {
            _machine?.Stop();
            _actions?.Dispose();
            _badges?.Dispose();
            _cardBar?.Dispose();
            _coinFx?.Dispose();
            _hud?.Dispose();
        }
    }
}
