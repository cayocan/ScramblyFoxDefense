using ScramblyFoxDefense.Audio;
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
        [SerializeField] GameObject poofPrefab;
        [SerializeField] Sprite sparkSprite;
        [SerializeField] Sprite ringSprite;
        [SerializeField] Sprite arrowSprite;

        [Header("Scene roots")]
        [SerializeField] Transform enemyRoot;
        [SerializeField] Transform towerRoot;
        [SerializeField] Transform fxRoot;

        [Header("HUD")]
        [SerializeField] TextMesh coinsText;
        [SerializeField] TextMesh phaseText;
        [SerializeField] TextMesh bannerText;
        [SerializeField] SpriteRenderer coinIcon;
        [SerializeField] SpriteRenderer topBand;
        [SerializeField] SpriteRenderer bannerPill;
        [SerializeField] SpriteRenderer tutorialHand;
        [SerializeField] CardView[] cards;
        [SerializeField] SpriteRenderer[] locks;
        [SerializeField] Sprite lockOpenSprite;
        [SerializeField] HudButton restartButton;
        [SerializeField] HudButton muteButton;
        [SerializeField] SpriteRenderer muteIcon;
        [SerializeField] Sprite soundOnSprite;
        [SerializeField] Sprite soundOffSprite;

        [Header("Redeem and end card")]
        [SerializeField] Transform vault;
        [SerializeField] Transform endCardPanel;
        [SerializeField] TextMesh endTitle;
        [SerializeField] TextMesh endCollected;
        [SerializeField] TextMesh ctaToast;
        [SerializeField] Transform[] rewardCards;
        [SerializeField] HudButton ctaButton;
        [SerializeField] HudButton playAgainButton;

        InputRouter _input;
        EnemySystem _enemies;
        TowerSystem _towers;
        PlayerActions _actions;
        CoinPopFx _coinFx;
        FeedbackFx _feedback;
        SparkFx _sparks;
        TutorialHand _tutorial;
        HudLayout _layout;
        HudView _hud;
        CardBarView _cardBar;
        LockBarView _lockBar;
        TowerBadges _badges;
        SlotHighlighter _slotHighlighter;
        CameraFit _cameraFit;
        RestartController _restart;
        PagePause _pagePause;
        SoundCues _soundCues;
        MuteToggle _mute;
        RedeemSequence _redeem;
        EndCardView _endCard;
        GameStateMachine _machine;

        void Awake()
        {
            Time.timeScale = 1f;
            Time.maximumDeltaTime = config.maxDeltaTime;

            var economy = new Economy(config.startingCoins);
            var path = new PathRoute(pathWaypoints);
            var session = new GameSession();

            _pagePause = new PagePause(new WebPageState());
            _input = new InputRouter();
            _enemies = new EnemySystem(config, path, economy, enemyRoot);
            var spawner = new WaveSpawner(_enemies);
            _towers = new TowerSystem(config, economy, _enemies, towerRoot, projectilePrefab);
            var slots = new SlotManager(slotTransforms, mainCamera, config.pickRadiusCssPixels);

            var tutorialGate = new TutorialGate(_towers);
            _layout = new HudLayout(mainCamera);
            // HUD buttons first: the first tap handler that accepts a tap consumes it.
            _restart = new RestartController(restartButton, mainCamera);
            _restart.BlockDuring(tutorialGate);
            _input.Register(_restart.HandleTap);
            var audio = new WebAudioService();
            _mute = new MuteToggle(audio, muteButton, muteIcon, soundOnSprite, soundOffSprite, mainCamera);
            _input.Register(_mute.HandleTap);
            _hud = new HudView(_layout, economy, coinsText, phaseText, bannerText, coinIcon, topBand, bannerPill);
            _cardBar = new CardBarView(cards, config, economy, _layout, mainCamera);
            _lockBar = new LockBarView(locks, lockOpenSprite, _layout);
            _actions = new PlayerActions(config, _input, _cardBar, slots, _towers, economy, audio, tutorialGate);
            _badges = new TowerBadges(_towers, economy, badgePrefab, ringSprite, arrowSprite, mainCamera.transform);
            _slotHighlighter = new SlotHighlighter(slots, _actions);
            _tutorial = new TutorialHand(tutorialHand, _layout, mainCamera, config, _actions, cards, slots, _towers, _enemies, economy);
            _coinFx = new CoinPopFx(_enemies, coinPrefab, fxRoot);
            _soundCues = new SoundCues(audio, _enemies, _towers);
            _feedback = new FeedbackFx(_enemies, _towers, poofPrefab, fxRoot, mainCamera.transform);
            _sparks = new SparkFx(_towers, sparkSprite, fxRoot, mainCamera.transform);
            _cameraFit = new CameraFit(mainCamera, boardCenter, boardSize.x, boardSize.y);
            _redeem = new RedeemSequence(coinPrefab, fxRoot, vault, pathWaypoints);
            _endCard = new EndCardView(endCardPanel, endTitle, endCollected, ctaToast, rewardCards, ctaButton, playAgainButton, _layout, mainCamera);

            _machine = new GameStateMachine();
            _machine.Register(new IntroState(_machine, config, _towers, _actions, _hud));
            _machine.Register(new WaveState(_machine, config, session, spawner, _hud, _lockBar, audio, economy));
            _machine.Register(new BreatherState(_machine, config, session, _hud));
            _machine.Register(new RedeemState(_machine, _actions, _hud, _redeem, _input, _cardBar, _badges, audio));
            _machine.Register(new EndCardState(economy, _hud, _endCard, _restart, _input, audio, session, config.waves.Length));
        }

        void Start() => _machine.Enter<IntroState>();

        void Update()
        {
            float deltaTime = _pagePause.Filter(Time.deltaTime);
            if (_cameraFit.Tick())
            {
                _layout.Refresh();
                _hud.Layout();
                _cardBar.Layout();
                _lockBar.Layout();
                _endCard.Layout();
                _layout.Place(restartButton.root, new Vector2(0f, 1f), new Vector2(46f, -26f), 1f);
                _layout.Place(muteButton.root, new Vector2(0f, 1f), new Vector2(112f, -26f), 1f);
            }

            if (!_pagePause.IsPaused) _input.Tick();
            _machine.Tick(deltaTime);
            _enemies.Tick(deltaTime);
            _towers.Tick(deltaTime);
            _coinFx.Tick(deltaTime);
            _feedback.Tick(deltaTime);
            _sparks.Tick(deltaTime);
            _badges.Tick(deltaTime);
            _hud.Tick(deltaTime);
            _cardBar.Tick(deltaTime);
            _lockBar.Tick(deltaTime);
            _slotHighlighter.Tick(deltaTime);
            _tutorial.Tick(deltaTime);
            _endCard.Tick(deltaTime);
        }

        void OnDestroy()
        {
            _machine?.Stop();
            if (_restart != null) _input.Unregister(_restart.HandleTap);
            if (_mute != null) _input.Unregister(_mute.HandleTap);
            _soundCues?.Dispose();
            _actions?.Dispose();
            _badges?.Dispose();
            _cardBar?.Dispose();
            _coinFx?.Dispose();
            _feedback?.Dispose();
            _sparks?.Dispose();
            _tutorial?.Dispose();
            _hud?.Dispose();
        }
    }
}
