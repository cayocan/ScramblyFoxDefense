using ScramblyFoxDefense.Config;
using ScramblyFoxDefense.Gameplay;
using ScramblyFoxDefense.States;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>
    /// Text-free tutorial (GDD section 4). 1) Until the first tower stands, a grabbing hand demonstrates the drag
    /// in a loop: open over the first card, grab, carry a ghost tower along an arc to the suggested spot
    /// (GameConfig.tutorialSpot), release, fade. It hides the moment the player starts dragging. 2) The first time
    /// the wallet covers an upgrade, a pointing hand shows that tower. After that it only returns as an idle hint
    /// (5 s without a press while no predators are on the board).
    /// </summary>
    public sealed class TutorialHand
    {
        const float IdleHintSeconds = 5f;
        const float HandSizePixels = 44f;
        const float GrabSizePixels = 52f;
        // Demo loop, in seconds: open over the card, grab, carry, release, fade.
        const float OpenEnd = 0.5f, GrabEnd = 0.75f, CarryEnd = 1.95f, ReleaseEnd = 2.3f, LoopEnd = 2.8f;

        readonly SpriteRenderer _hand;
        readonly Sprite _pointSprite;
        readonly Sprite _openSprite;
        readonly Sprite _grabSprite;
        readonly TowerGhost _ghost;
        readonly TowerPicker _picker;
        bool _demoGhost;
        float _demoTime;
        readonly HudLayout _layout;
        readonly Camera _camera;
        readonly GameConfig _config;
        readonly PlayerActions _actions;
        readonly CardView[] _cards;
        readonly PlacementValidator _placement;
        readonly Vector3 _tutorialSpot;
        readonly TowerSystem _towers;
        readonly EnemySystem _enemies;
        readonly Economy _economy;
        readonly GameSession _session;
        bool _upgradeHintDone;

        // Wave 1 is free play (build more towers); upgrades are taught from wave 2.
        const int UpgradeHintFromWave = 1;
        float _idle;
        float _time;

        public TutorialHand(SpriteRenderer hand, Sprite openSprite, Sprite grabSprite, TowerGhost ghost, TowerPicker picker, HudLayout layout, Camera camera, GameConfig config, PlayerActions actions,
            CardView[] cards, PlacementValidator placement, float groundHeight, TowerSystem towers, EnemySystem enemies, Economy economy, GameSession session)
        {
            _session = session;
            _hand = hand;
            _pointSprite = hand.sprite;
            _openSprite = openSprite;
            _grabSprite = grabSprite;
            _ghost = ghost;
            _picker = picker;
            _layout = layout;
            _camera = camera;
            _config = config;
            _actions = actions;
            _cards = cards;
            _placement = placement;
            _tutorialSpot = new Vector3(config.tutorialSpot.x, groundHeight, config.tutorialSpot.z);
            _towers = towers;
            _enemies = enemies;
            _economy = economy;
            _actions.Acted += OnActed;
            _towers.Upgraded += OnUpgraded;
            _hand.gameObject.SetActive(false);
            if (!placement.IsValid(_tutorialSpot)) Debug.LogWarning("[Tutorial] GameConfig.tutorialSpot breaks the placement rules");
        }

        public void Dispose()
        {
            _actions.Acted -= OnActed;
            _towers.Upgraded -= OnUpgraded;
        }

        public void Tick(float deltaTime)
        {
            _time += deltaTime;
            _idle += deltaTime;

            if (_actions.Enabled && _towers.Towers.Count == 0)
            {
                TickDemo(deltaTime);
                return;
            }
            StopDemo();

            Vector3? target = _actions.Enabled ? FindTarget() : null;
            _hand.gameObject.SetActive(target.HasValue);
            if (!target.HasValue) return;

            // Fingertip on the target, with a gentle tap bob.
            _hand.sprite = _pointSprite;
            _hand.color = Color.white;
            Vector3 viewport = _camera.WorldToViewportPoint(target.Value);
            float bob = Mathf.Abs(Mathf.Sin(_time * 4f)) * 10f;
            _layout.Place(_hand.transform, viewport, new Vector2(6f, -4f - bob), HandSizePixels / 64f);
            // In front of the 3D pet faces on the cards (they sit slightly closer than the HUD plane).
            _hand.transform.localPosition += Vector3.back * 0.1f;
        }

        /// <summary>The grab demo. The player's own drag owns the ghost, so the demo yields as soon as it starts.</summary>
        void TickDemo(float deltaTime)
        {
            if (_actions.SelectedCard >= 0)
            {
                _demoGhost = false; // the player's drag now drives the ghost; do not hide it
                _demoTime = 0f;
                _hand.gameObject.SetActive(false);
                return;
            }
            _demoTime = (_demoTime + deltaTime) % LoopEnd;
            float t = _demoTime;

            Vector3 from = _camera.WorldToViewportPoint(CardFace(TutorialGate.Card));
            Vector3 to = _camera.WorldToViewportPoint(_tutorialSpot);
            float carry = Mathf.Clamp01((t - GrabEnd) / (CarryEnd - GrabEnd));
            carry = carry * carry * (3f - 2f * carry); // ease in and out
            Vector3 viewport = Vector3.Lerp(from, to, carry);
            viewport.y += Mathf.Sin(carry * Mathf.PI) * 0.08f; // arc over the board

            bool grabbing = t >= OpenEnd && t < CarryEnd;
            _hand.gameObject.SetActive(true);
            _hand.sprite = grabbing ? _grabSprite : _openSprite;
            float alpha = t < ReleaseEnd ? 1f : 1f - (t - ReleaseEnd) / (LoopEnd - ReleaseEnd);
            _hand.color = new Color(1f, 1f, 1f, alpha);
            float size = grabbing ? GrabSizePixels : HandSizePixels;
            // The palm sits just above the ghost it carries, so the tower stays visible.
            _layout.Place(_hand.transform, viewport, new Vector2(0f, 26f), size / 64f);
            _hand.transform.localPosition += Vector3.back * 0.1f;

            // The ghost rides under the hand once it has left the card band, until the hand fades.
            bool showGhost = carry > 0.2f && t < ReleaseEnd;
            if (showGhost)
            {
                var ground = _picker.Ground(_camera.ViewportToScreenPoint(viewport));
                if (ground.HasValue)
                {
                    _ghost.Show(TutorialGate.Card, ground.Value, _placement.IsValid(ground.Value));
                    _demoGhost = true;
                    return;
                }
            }
            HideDemoGhost();
        }

        void StopDemo()
        {
            HideDemoGhost();
            _demoTime = 0f;
        }

        void HideDemoGhost()
        {
            if (!_demoGhost) return;
            _ghost.Hide();
            _demoGhost = false;
        }

        Vector3? FindTarget()
        {
            var upgradable = _session.WaveIndex >= UpgradeHintFromWave ? CheapestAffordableUpgrade() : null;
            if (!_upgradeHintDone && upgradable != null) return upgradable.Transform.position + Vector3.up * 0.6f;

            // Idle hint between waves: the best affordable action.
            if (_idle < IdleHintSeconds || _enemies.Active.Count > 0) return null;
            if (_actions.SelectedCard >= 0) return null; // already dragging
            if (upgradable != null) return upgradable.Transform.position + Vector3.up * 0.6f;
            int card = CheapestAffordableCard();
            return card >= 0 && _placement.HasRoom ? CardFace(card) : (Vector3?)null;
        }

        /// <summary>The pet face on a card (card roots are laid out in reference pixels), so the title stays readable.</summary>
        Vector3 CardFace(int index) => _cards[index].root.TransformPoint(new Vector3(0f, 18f, 0f));

        Tower CheapestAffordableUpgrade()
        {
            Tower best = null;
            foreach (var tower in _towers.Towers)
                if (!tower.IsMaxLevel && _economy.CanAfford(tower.NextLevelCost) && (best == null || tower.NextLevelCost < best.NextLevelCost))
                    best = tower;
            return best;
        }

        int CheapestAffordableCard()
        {
            int best = -1;
            for (int i = 0; i < _config.towers.Length; i++)
            {
                int cost = _config.towers[i].levels[0].cost;
                if (_economy.CanAfford(cost) && (best < 0 || cost < _config.towers[best].levels[0].cost)) best = i;
            }
            return best;
        }

        void OnActed() => _idle = 0f;

        void OnUpgraded(Tower tower) => _upgradeHintDone = true;
    }
}
