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
        readonly SpriteRenderer _topBand;
        readonly SpriteRenderer _bannerPill;
        float _bannerTimer;
        float _bannerAge;
        Vector3 _bannerScale;
        Vector3 _pillScale;

        const float BannerPop = 0.25f;

        public HudView(HudLayout layout, Economy economy, TextMesh coins, TextMesh phase, TextMesh banner, SpriteRenderer coinIcon,
            SpriteRenderer topBand, SpriteRenderer bannerPill)
        {
            _coinIcon = coinIcon;
            _topBand = topBand;
            _bannerPill = bannerPill;
            _layout = layout;
            _economy = economy;
            _coins = coins;
            _phase = phase;
            _banner = banner;
            _economy.Changed += RefreshCoins;
            RefreshCoins();
            _banner.gameObject.SetActive(false);
            _bannerPill.gameObject.SetActive(false);
        }

        public void Dispose() => _economy.Changed -= RefreshCoins;

        public void SetPhase(string text) => _phase.text = text;

        public void ShowBanner(string text, float seconds)
        {
            _banner.text = text;
            _banner.gameObject.SetActive(true);
            _bannerPill.gameObject.SetActive(true);
            _bannerTimer = seconds;
            _bannerAge = 0f;
        }

        public void HideBanner()
        {
            _bannerTimer = 0f;
            _banner.gameObject.SetActive(false);
            _bannerPill.gameObject.SetActive(false);
        }

        public void Tick(float deltaTime)
        {
            PlaceCoinIcon(); // TextMesh rebuilds its bounds a frame after a text change
            if (_bannerTimer <= 0f) return;
            _bannerTimer -= deltaTime;
            _bannerAge += deltaTime;
            // Pop in with a small overshoot, then hold.
            float t = Mathf.Clamp01(_bannerAge / BannerPop);
            float pop = t < 1f ? Mathf.Lerp(0.6f, 1f, t) + Mathf.Sin(t * Mathf.PI) * 0.15f : 1f;
            _banner.transform.localScale = _bannerScale * pop;
            FitPill(pop);
            if (_bannerTimer <= 0f) HideBanner();
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
            _bannerScale = _banner.transform.localScale;
            _layout.Place(_bannerPill.transform, new Vector2(0.5f, 1f), new Vector2(0f, -124f), 1f);
            _pillScale = _bannerPill.transform.localScale;
            // Band covers the two top rows and bleeds past the screen edges and top.
            _layout.Place(_topBand.transform, new Vector2(0.5f, 1f), new Vector2(0f, -44f), 1f);
            _topBand.size = new Vector2(_layout.ScreenWidthPixels + 40f, 136f);
        }

        /// <summary>Pill hugs the banner text: its width follows the rendered text bounds.</summary>
        void FitPill(float pop)
        {
            var bounds = _banner.GetComponent<Renderer>().bounds;
            float widthPixels = bounds.size.x / Mathf.Max(_layout.PixelUnit, 1e-5f);
            _bannerPill.size = new Vector2(Mathf.Max(120f, widthPixels / Mathf.Max(pop, 0.01f) + 36f), 42f);
            _bannerPill.transform.localScale = _pillScale * pop;
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
