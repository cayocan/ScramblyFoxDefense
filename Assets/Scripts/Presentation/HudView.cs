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
        readonly SpriteRenderer _coinIcon;
        float _bannerTimer;

        public HudView(HudLayout layout, Economy economy, TextMesh coins, TextMesh phase, TextMesh banner, SpriteRenderer coinIcon)
        {
            _coinIcon = coinIcon;
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

        public void HideBanner()
        {
            _bannerTimer = 0f;
            _banner.gameObject.SetActive(false);
        }

        public void Tick(float deltaTime)
        {
            PlaceCoinIcon(); // TextMesh rebuilds its bounds a frame after a text change
            if (_bannerTimer <= 0f) return;
            _bannerTimer -= deltaTime;
            if (_bannerTimer <= 0f) _banner.gameObject.SetActive(false);
        }

        public void Layout()
        {
            _phase.anchor = TextAnchor.MiddleCenter;
            _coins.anchor = TextAnchor.MiddleRight;
            _layout.PlaceText(_phase, new Vector2(0.5f, 1f), new Vector2(0f, -94f), 14f);
            _layout.PlaceText(_coins, new Vector2(1f, 1f), new Vector2(-14f, -24f), 18f);
            _layout.Place(_coinIcon.transform, new Vector2(1f, 1f), new Vector2(-14f, -24f), 18f / 64f);
            PlaceCoinIcon();
            // 26 px fits the longest banner ("Demo rewards unlocked!") inside the 390 px reference width.
            _layout.PlaceText(_banner, new Vector2(0.5f, 1f), new Vector2(0f, -124f), 26f);
        }

        void RefreshCoins()
        {
            _coins.text = $"Demo coins {_economy.Wallet}";
            PlaceCoinIcon();
        }

        /// <summary>Keeps the coin sprite just left of the right-aligned balance, whatever its width.</summary>
        void PlaceCoinIcon()
        {
            var parent = _coins.transform.parent;
            var bounds = _coins.GetComponent<Renderer>().bounds;
            if (bounds.size.x <= 0f) return;
            float left = parent.InverseTransformPoint(bounds.min).x;
            var position = _coinIcon.transform.localPosition;
            position.x = left - 14f * _layout.PixelUnit;
            position.y = _coins.transform.localPosition.y;
            _coinIcon.transform.localPosition = position;
        }
    }
}
