using ScramblyFoxDefense.Gameplay;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>Top band: phase label, demo coin balance and the wave banner.</summary>
    public sealed class HudView
    {
        readonly HudLayout _layout;
        readonly Economy _economy;
        readonly TextMesh _coins;
        readonly TextMesh _phase;
        readonly TextMesh _banner;
        float _bannerTimer;

        public HudView(HudLayout layout, Economy economy, TextMesh coins, TextMesh phase, TextMesh banner)
        {
            _layout = layout;
            _economy = economy;
            _coins = coins;
            _phase = phase;
            _banner = banner;
            _economy.Changed += RefreshCoins;
            RefreshCoins();
            _banner.gameObject.SetActive(false);
        }

        public void Dispose() => _economy.Changed -= RefreshCoins;

        public void SetPhase(string text) => _phase.text = text;

        public void ShowBanner(string text, float seconds)
        {
            _banner.text = text;
            _banner.gameObject.SetActive(true);
            _bannerTimer = seconds;
        }

        public void Tick(float deltaTime)
        {
            if (_bannerTimer <= 0f) return;
            _bannerTimer -= deltaTime;
            if (_bannerTimer <= 0f) _banner.gameObject.SetActive(false);
        }

        public void Layout()
        {
            _phase.anchor = TextAnchor.MiddleLeft;
            _coins.anchor = TextAnchor.MiddleRight;
            _layout.PlaceText(_phase, new Vector2(0f, 1f), new Vector2(14f, -24f), 18f);
            _layout.PlaceText(_coins, new Vector2(1f, 1f), new Vector2(-14f, -24f), 18f);
            _layout.PlaceText(_banner, new Vector2(0.5f, 1f), new Vector2(0f, -120f), 34f);
        }

        void RefreshCoins() => _coins.text = $"Demo coins {_economy.Wallet}";
    }
}
