using System;

namespace ScramblyFoxDefense.Gameplay
{
    /// <summary>Demo coins only. Wallet is spendable; Collected sums everything earned (Redeem and end card).</summary>
    public sealed class Economy
    {
        public int Wallet { get; private set; }
        public int Collected { get; private set; }
        public int Leaks { get; private set; }

        public event Action Changed;

        public Economy(int startingCoins)
        {
            Wallet = startingCoins;
        }

        public bool CanAfford(int cost) => Wallet >= cost;

        public bool TrySpend(int cost)
        {
            if (!CanAfford(cost)) return false;
            Wallet -= cost;
            Changed?.Invoke();
            return true;
        }

        public void Earn(int amount)
        {
            Wallet += amount;
            Collected += amount;
            Changed?.Invoke();
        }

        /// <summary>A predator reached the vault: lose coins, never below zero.</summary>
        public void Leak(int penalty)
        {
            Leaks++;
            Wallet = Math.Max(0, Wallet - penalty);
            Changed?.Invoke();
        }
    }
}
