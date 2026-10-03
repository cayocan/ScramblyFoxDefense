using ScramblyFoxDefense.Gameplay;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>Free slots pulse while a card is selected; occupied slots hide their marker.</summary>
    public sealed class SlotHighlighter
    {
        readonly SlotManager _slots;
        readonly PlayerActions _actions;
        readonly Vector3[] _baseScale;
        readonly Renderer[][] _markers;
        float _time;

        public SlotHighlighter(SlotManager slots, PlayerActions actions)
        {
            _slots = slots;
            _actions = actions;
            int count = slots.Slots.Length;
            _baseScale = new Vector3[count];
            _markers = new Renderer[count][];
            for (int i = 0; i < count; i++)
            {
                _baseScale[i] = slots.Slots[i].Transform.localScale;
                _markers[i] = slots.Slots[i].Transform.GetComponentsInChildren<Renderer>(true);
            }
        }

        public void Tick(float deltaTime)
        {
            _time += deltaTime;
            bool selecting = _actions.SelectedCard >= 0;
            float pulse = selecting ? 1f + Mathf.Sin(_time * 8f) * 0.12f : 1f;
            for (int i = 0; i < _slots.Slots.Length; i++)
            {
                var slot = _slots.Slots[i];
                foreach (var marker in _markers[i])
                    if (marker.enabled != slot.IsFree) marker.enabled = slot.IsFree;
                slot.Transform.localScale = _baseScale[i] * (slot.IsFree ? pulse : 1f);
            }
        }
    }
}
