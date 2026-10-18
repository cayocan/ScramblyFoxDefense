using ScramblyFoxDefense.Gameplay;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>
    /// While the opening tutorial locks input, everything that cannot be tapped is greyed out and faded:
    /// the other cards and the Restart button. Restored as soon as the first tower stands.
    /// </summary>
    public sealed class TutorialFocus
    {
        static readonly Color Dimmed = new Color(0.62f, 0.6f, 0.68f, 0.45f);

        readonly TutorialGate _gate;
        readonly CardBarView _cards;
        readonly HudButton _restart;
        readonly Color _restartColor;
        bool? _applied;

        public TutorialFocus(TutorialGate gate, CardBarView cards, HudButton restart)
        {
            _gate = gate;
            _cards = cards;
            _restart = restart;
            _restartColor = restart.background is SpriteRenderer sprite ? sprite.color : Color.white;
        }

        public void Tick()
        {
            bool active = _gate.Active;
            if (_applied == active) return;
            _applied = active;

            _cards.SetFocus(active ? TutorialGate.Card : -1);
            Tint.Set(_restart.background, active ? Dimmed : _restartColor);
            var label = _restart.label.color;
            label.a = active ? 0.45f : 1f;
            _restart.label.color = label;
        }
    }
}
