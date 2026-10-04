using System;

namespace ScramblyFoxDefense.Gameplay
{
    /// <summary>
    /// The fox guards the Reward Vault: predators that reach it hurt the fox instead of taking demo coins.
    /// At zero the session ends early (the end card still invites the player to try again).
    /// </summary>
    public sealed class FoxHealth
    {
        public int Max { get; }
        public int Current { get; private set; }
        public bool Defeated => Current <= 0;

        public event Action Changed;

        public FoxHealth(int max)
        {
            Max = Math.Max(1, max);
            Current = Max;
        }

        public void Damage(int amount)
        {
            if (Defeated || amount <= 0) return;
            Current = Math.Max(0, Current - amount);
            Changed?.Invoke();
        }
    }
}
