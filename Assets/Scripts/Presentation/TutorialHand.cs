using ScramblyFoxDefense.Config;
using ScramblyFoxDefense.Gameplay;
using ScramblyFoxDefense.States;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>
    /// Text-free tutorial (GDD section 4): 1) point at the first card, 2) point at the slot nearest the
    /// path start, 3) the first time the wallet covers an upgrade, point at that tower. After that it only
    /// returns as an idle hint (5 s without a tap while no predators are on the board).
    /// </summary>
    public sealed class TutorialHand
    {
        const float IdleHintSeconds = 5f;
        const float HandSizePixels = 44f;

        readonly SpriteRenderer _hand;
        readonly HudLayout _layout;
        readonly Camera _camera;
        readonly GameConfig _config;
        readonly PlayerActions _actions;
        readonly CardView[] _cards;
        readonly SlotManager _slots;
        readonly TowerSystem _towers;
        readonly EnemySystem _enemies;
        readonly Economy _economy;
        readonly GameSession _session;
        bool _upgradeHintDone;

        // The first waves are free play (build more towers); upgrades are taught from wave 3.
        const int UpgradeHintFromWave = 2;
        float _idle;
        float _time;

        public TutorialHand(SpriteRenderer hand, HudLayout layout, Camera camera, GameConfig config, PlayerActions actions,
            CardView[] cards, SlotManager slots, TowerSystem towers, EnemySystem enemies, Economy economy, GameSession session)
        {
            _session = session;
            _hand = hand;
            _layout = layout;
            _camera = camera;
            _config = config;
            _actions = actions;
            _cards = cards;
            _slots = slots;
            _towers = towers;
            _enemies = enemies;
            _economy = economy;
            _actions.Acted += OnActed;
            _towers.Upgraded += OnUpgraded;
            _hand.gameObject.SetActive(false);
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

            Vector3? target = _actions.Enabled ? FindTarget() : null;
            _hand.gameObject.SetActive(target.HasValue);
            if (!target.HasValue) return;

            // Fingertip on the target, with a gentle tap bob.
            Vector3 viewport = _camera.WorldToViewportPoint(target.Value);
            float bob = Mathf.Abs(Mathf.Sin(_time * 4f)) * 10f;
            _layout.Place(_hand.transform, viewport, new Vector2(6f, -4f - bob), HandSizePixels / 64f);
            // In front of the 3D pet faces on the cards (they sit slightly closer than the HUD plane).
            _hand.transform.localPosition += Vector3.back * 0.1f;
        }

        Vector3? FindTarget()
        {
            if (_towers.Towers.Count == 0)
            {
                // Steps 1 and 2: first card, then the slot nearest the path start.
                if (_actions.SelectedCard < 0) return CardFace(0);
                return _slots.Slots[0].Transform.position;
            }

            var upgradable = _session.WaveIndex >= UpgradeHintFromWave ? CheapestAffordableUpgrade() : null;
            if (!_upgradeHintDone && upgradable != null) return upgradable.Transform.position + Vector3.up * 0.6f;

            // Idle hint between waves: the best affordable action.
            if (_idle < IdleHintSeconds || _enemies.Active.Count > 0) return null;
            if (_actions.SelectedCard >= 0) return FreeSlot()?.Transform.position;
            if (upgradable != null) return upgradable.Transform.position + Vector3.up * 0.6f;
            int card = CheapestAffordableCard();
            return card >= 0 && FreeSlot() != null ? CardFace(card) : (Vector3?)null;
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

        Slot FreeSlot()
        {
            foreach (var slot in _slots.Slots)
                if (slot.IsFree) return slot;
            return null;
        }

        void OnActed() => _idle = 0f;

        void OnUpgraded(Tower tower) => _upgradeHintDone = true;
    }
}
