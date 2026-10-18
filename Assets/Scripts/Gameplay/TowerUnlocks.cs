namespace ScramblyFoxDefense.Gameplay
{
    /// <summary>
    /// Milestone unlocks (Stage 2): the session starts with the first tower card only; each cleared wave
    /// (except the last) opens the next one. Rebuilt with the scene on restart.
    /// </summary>
    public sealed class TowerUnlocks
    {
        readonly int _total;

        public TowerUnlocks(int total)
        {
            _total = total;
            UnlockedCount = 1;
        }

        public int UnlockedCount { get; private set; }

        public bool HasLocked => UnlockedCount < _total;

        public bool IsUnlocked(int card) => card < UnlockedCount;

        /// <summary>Opens the next card and returns its index, or -1 when every card is open.</summary>
        public int UnlockNext() => HasLocked ? UnlockedCount++ : -1;
    }
}
