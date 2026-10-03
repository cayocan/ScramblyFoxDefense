using ScramblyFoxDefense.Input;
using UnityEngine;

namespace ScramblyFoxDefense.Gameplay
{
    /// <summary>
    /// Greybox build input: a tap on a free slot builds the selected tower. Card selection and
    /// upgrades arrive in the next feature; states toggle Enabled.
    /// </summary>
    public sealed class BuildController
    {
        readonly InputRouter _input;
        readonly SlotManager _slots;
        readonly TowerSystem _towers;

        public bool Enabled { get; set; }
        public int SelectedTower { get; set; }

        public BuildController(InputRouter input, SlotManager slots, TowerSystem towers)
        {
            _input = input;
            _slots = slots;
            _towers = towers;
            _input.Tapped += OnTapped;
        }

        public void Dispose() => _input.Tapped -= OnTapped;

        void OnTapped(Vector2 screenPoint)
        {
            if (!Enabled) return;
            var slot = _slots.Pick(screenPoint);
            if (slot != null && slot.IsFree) _towers.TryBuild(slot, SelectedTower);
        }
    }
}
