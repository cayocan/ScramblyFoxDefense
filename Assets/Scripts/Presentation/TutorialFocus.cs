using ScramblyFoxDefense.Gameplay;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>
    /// While the opening tutorial locks input, everything that cannot be tapped is greyed out and faded:
    /// the other cards, the other slots and the Restart button. Restored as soon as the first tower stands.
    /// </summary>
    public sealed class TutorialFocus
    {
        static readonly Color Dimmed = new Color(0.62f, 0.6f, 0.68f, 0.45f);
        static readonly Color SlotIdle = new Color32(0x78, 0x45, 0xD8, 0xFF);
        static readonly Color SlotDimmed = new Color(0.62f, 0.6f, 0.68f, 0.5f);

        readonly TutorialGate _gate;
        readonly CardBarView _cards;
        readonly SlotManager _slots;
        readonly HudButton _restart;
        readonly Color _restartColor;
        bool? _applied;

        public TutorialFocus(TutorialGate gate, CardBarView cards, SlotManager slots, HudButton restart)
        {
            _gate = gate;
            _cards = cards;
            _slots = slots;
            _restart = restart;
            _restartColor = restart.background is SpriteRenderer sprite ? sprite.color : Color.white;
        }

        public void Tick()
        {
            bool active = _gate.Active;
            if (_applied == active) return;
            _applied = active;

            _cards.SetFocus(active ? TutorialGate.Card : -1);
            for (int i = 0; i < _slots.Slots.Length; i++)
            {
                var color = active && i != TutorialGate.SlotIndex ? SlotDimmed : SlotIdle;
                foreach (var renderer in _slots.Slots[i].Transform.GetComponentsInChildren<Renderer>(true)) Tint.Set(renderer, color);
            }
            Tint.Set(_restart.background, active ? Dimmed : _restartColor);
            var label = _restart.label.color;
            label.a = active ? 0.45f : 1f;
            _restart.label.color = label;
        }
    }
}
