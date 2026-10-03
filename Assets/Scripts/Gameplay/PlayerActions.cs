using System;
using ScramblyFoxDefense.Audio;
using ScramblyFoxDefense.Config;
using ScramblyFoxDefense.Input;
using ScramblyFoxDefense.Presentation;
using UnityEngine;

namespace ScramblyFoxDefense.Gameplay
{
    /// <summary>
    /// Routes every tap (GDD section 4): card selects or deselects a tower type; a free slot builds the
    /// selection; a tower upgrades; anything else cancels. Not enough coins shakes the card.
    /// </summary>
    public sealed class PlayerActions
    {
        readonly GameConfig _config;
        readonly InputRouter _input;
        readonly CardBarView _cards;
        readonly SlotManager _slots;
        readonly TowerSystem _towers;
        readonly Economy _economy;
        readonly IAudioService _audio;

        public bool Enabled { get; set; }
        public int SelectedCard { get; private set; } = -1;

        /// <summary>Raised on any accepted tap; drives the tutorial hand's idle timer.</summary>
        public event Action Acted;

        public PlayerActions(GameConfig config, InputRouter input, CardBarView cards, SlotManager slots, TowerSystem towers, Economy economy, IAudioService audio)
        {
            _audio = audio;
            _config = config;
            _input = input;
            _cards = cards;
            _slots = slots;
            _towers = towers;
            _economy = economy;
            _input.Register(OnTapped);
            _input.Canceled += ClearSelection;
        }

        public void Dispose()
        {
            _input.Unregister(OnTapped);
            _input.Canceled -= ClearSelection;
        }

        public void ClearSelection() => Select(-1);

        /// <summary>Board and card taps. Always consumes the tap while enabled.</summary>
        bool OnTapped(Vector2 screenPoint)
        {
            if (!Enabled) return false;
            Acted?.Invoke();

            int card = _cards.HitTest(screenPoint);
            if (card >= 0)
            {
                OnCard(card);
                return true;
            }

            var slot = _slots.Pick(screenPoint);
            if (slot == null)
            {
                ClearSelection();
                return true;
            }

            if (!slot.IsFree)
            {
                if (!_towers.TryUpgrade(slot.Tower) && !slot.Tower.IsMaxLevel) _audio.Play(Sound.Deny);
                ClearSelection();
                return true;
            }

            if (SelectedCard < 0) return true;
            if (_towers.TryBuild(slot, SelectedCard)) ClearSelection();
            else Deny(SelectedCard);
            return true;
        }

        void OnCard(int card)
        {
            if (card == SelectedCard)
            {
                ClearSelection();
                return;
            }
            if (!_economy.CanAfford(_config.towers[card].levels[0].cost))
            {
                Deny(card);
                ClearSelection();
                return;
            }
            Select(card);
            _audio.Play(Sound.Select);
        }

        void Deny(int card)
        {
            _cards.Shake(card);
            _audio.Play(Sound.Deny);
        }

        void Select(int card)
        {
            SelectedCard = card;
            _cards.SetSelected(card);
        }
    }
}
