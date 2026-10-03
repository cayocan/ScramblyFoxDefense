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
        [SerializeField] Vector2 boardSize = new Vector2(6f, 10f);

        [Header("Prefabs")]
        [SerializeField] GameObject projectilePrefab;
        [SerializeField] GameObject coinPrefab;

        [Header("Scene roots")]
        [SerializeField] Transform enemyRoot;
        [SerializeField] Transform towerRoot;
        [SerializeField] Transform fxRoot;

        [Header("HUD")]
        [SerializeField] TextMesh coinsText;
        [SerializeField] TextMesh phaseText;
        [SerializeField] TextMesh bannerText;

        InputRouter _input;
        EnemySystem _enemies;
        TowerSystem _towers;
        BuildController _build;
        CoinPopFx _coinFx;
        HudView _hud;
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
            _build = new BuildController(_input, slots, _towers);
            _coinFx = new CoinPopFx(_enemies, coinPrefab, fxRoot);
            _hud = new HudView(mainCamera, economy, coinsText, phaseText, bannerText);
            _cameraFit = new CameraFit(mainCamera, Vector3.zero, boardSize.x, boardSize.y);

            _machine = new GameStateMachine();
            _machine.Register(new IntroState(_machine, config, _towers, _build, _hud));
            _machine.Register(new WaveState(_machine, config, session, spawner, _hud));
            _machine.Register(new BreatherState(_machine, config, session));
            _machine.Register(new RedeemState(_machine, _build, _hud));
            _machine.Register(new EndCardState(economy, _hud));
        }

        void Start() => _machine.Enter<IntroState>();

        void Update()
        {
            float deltaTime = Time.deltaTime;
            if (_cameraFit.Tick()) _hud.Layout();

            _input.Tick();
            _machine.Tick(deltaTime);
            _enemies.Tick(deltaTime);
            _towers.Tick(deltaTime);
            _coinFx.Tick(deltaTime);
            _hud.Tick(deltaTime);
        }

        void OnDestroy()
        {
            _machine?.Stop();
            _build?.Dispose();
            _coinFx?.Dispose();
            _hud?.Dispose();
        }
    }
}
