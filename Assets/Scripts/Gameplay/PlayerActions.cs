using System;
using ScramblyFoxDefense.Audio;
using ScramblyFoxDefense.Config;
using ScramblyFoxDefense.Input;
using ScramblyFoxDefense.Presentation;
using UnityEngine;

namespace ScramblyFoxDefense.Gameplay
{
    /// <summary>
    /// Routes every press (GDD section 4, Stage 2 free placement): pressing an affordable card drags a ghost
    /// tower; releasing on a valid point builds it, anywhere else cancels. A tap on a tower upgrades it.
    /// A locked card, not enough coins, a full map or an invalid drop shakes the card; coins are only spent on a build.
    /// </summary>
    public sealed class PlayerActions : IDragTarget
    {
        readonly GameConfig _config;
        readonly InputRouter _input;
        readonly CardBarView _cards;
        readonly TowerPicker _picker;
        readonly PlacementValidator _placement;
        readonly TowerGhost _ghost;
        readonly TowerSystem _towers;
        readonly Economy _economy;
        readonly IAudioService _audio;
        readonly TutorialGate _tutorial;
        readonly TowerUnlocks _unlocks;
        bool _enabled;
        Vector3? _ghostPoint;
        bool _ghostValid;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                if (!value) ClearSelection();
            }
        }

        /// <summary>The card being dragged, or -1.</summary>
        public int SelectedCard { get; private set; } = -1;

        /// <summary>Raised on any accepted press; drives the tutorial hand's idle timer.</summary>
        public event Action Acted;

        public PlayerActions(GameConfig config, InputRouter input, CardBarView cards, TowerPicker picker, PlacementValidator placement, TowerGhost ghost,
            TowerSystem towers, Economy economy, IAudioService audio, TutorialGate tutorial, TowerUnlocks unlocks)
        {
            _unlocks = unlocks;
            _tutorial = tutorial;
            _audio = audio;
            _config = config;
            _input = input;
            _cards = cards;
            _picker = picker;
            _placement = placement;
            _ghost = ghost;
            _towers = towers;
            _economy = economy;
            _input.Register(OnPressed);
            _input.Canceled += ClearSelection;
        }

        public void Dispose()
        {
            _input.Unregister(OnPressed);
            _input.Canceled -= ClearSelection;
        }

        /// <summary>Drops any drag in progress: the ghost hides and nothing is spent.</summary>
        public void ClearSelection()
        {
            if (SelectedCard >= 0 && _input.Dragging) _input.CancelDrag(); // calls back into DragCanceled
            EndDrag();
        }

        /// <summary>Board and card presses. Always consumes the press while enabled.</summary>
        bool OnPressed(Vector2 screenPoint)
        {
            if (!Enabled) return false;

            int card = _cards.HitTest(screenPoint);
            // Opening tutorial: only the first card reacts; every other press is swallowed.
            if (_tutorial.Active && card != TutorialGate.Card) return true;
            Acted?.Invoke();

            if (card >= 0)
            {
                StartDrag(card);
                return true;
            }

            var tower = _picker.Pick(screenPoint);
            if (tower != null && !_towers.TryUpgrade(tower) && !tower.IsMaxLevel) _audio.Play(Sound.Deny);
            return true;
        }

        void StartDrag(int card)
        {
            if (!_unlocks.IsUnlocked(card) || !_economy.CanAfford(_config.towers[card].levels[0].cost) || !_placement.HasRoom)
            {
                Deny(card);
                return;
            }
            SelectedCard = card;
            _cards.SetSelected(card);
            _cards.MarkUsed(card);
            _audio.Play(Sound.Select);
            _input.Capture(this);
            Log($"start card={card}");
        }

        public void DragMoved(Vector2 screenPoint, bool touch)
        {
            if (SelectedCard < 0) return;
            // Over the card band the drag reads as "not yet on the map": no ghost.
            if (_cards.HitTest(screenPoint) >= 0)
            {
                _ghost.Hide();
                _ghostPoint = null;
                return;
            }
            if (touch) screenPoint.y += _config.touchGhostOffsetCssPixels * TowerPicker.CssToScreenPixels();
            _ghostPoint = _picker.Ground(screenPoint);
            if (!_ghostPoint.HasValue)
            {
                _ghost.Hide();
                return;
            }
            _ghostValid = _placement.IsValid(_ghostPoint.Value);
            _ghost.Show(SelectedCard, _ghostPoint.Value, _ghostValid);
        }

        public void DragReleased(Vector2 screenPoint, bool touch)
        {
            int card = SelectedCard;
            if (card < 0) return;
            DragMoved(screenPoint, touch);
            bool onCanvas = screenPoint.x >= 0f && screenPoint.y >= 0f && screenPoint.x <= Screen.width && screenPoint.y <= Screen.height;
            bool shown = _ghost.Visible && _ghostPoint.HasValue;
            Vector3 point = _ghostPoint.GetValueOrDefault();
            bool valid = _ghostValid;
            EndDrag();

            // Released back on the cards or off the canvas: a plain cancel, no penalty cue.
            if (!onCanvas || !shown)
            {
                Log("released off the map, canceled");
                return;
            }
            if (valid && _towers.TryBuild(point, card))
            {
                Log($"built card={card} at ({point.x:F2}, {point.z:F2})");
                return;
            }
            Deny(card);
            Log($"invalid drop at ({point.x:F2}, {point.z:F2}), canceled");
        }

        public void DragCanceled()
        {
            EndDrag();
            Log("canceled (interrupted)");
        }

        // One console line per drag outcome: the browser checks read coins and ghost state from it.
        void Log(string what) => Debug.Log($"[Drag] {what} wallet={_economy.Wallet} ghost={(_ghost.Visible ? "visible" : "hidden")} towers={_towers.Towers.Count}");

        void EndDrag()
        {
            _ghost.Hide();
            _ghostPoint = null;
            if (SelectedCard < 0) return;
            SelectedCard = -1;
            _cards.SetSelected(-1);
        }

        void Deny(int card)
        {
            _cards.Shake(card);
            _audio.Play(Sound.Deny);
        }
    }
}
