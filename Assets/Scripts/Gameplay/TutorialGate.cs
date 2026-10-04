namespace ScramblyFoxDefense.Gameplay
{
    /// <summary>
    /// Opening tutorial lock: until the first tower is built, the session does not advance and only the tutorial
    /// target accepts taps (first card, then the first slot). The mute button stays available (brief).
    /// </summary>
    public sealed class TutorialGate
    {
        public const int Card = 0;
        public const int SlotIndex = 0;

        readonly TowerSystem _towers;

        public TutorialGate(TowerSystem towers)
        {
            _towers = towers;
        }

        public bool Active => _towers.Towers.Count == 0;
    }
}
