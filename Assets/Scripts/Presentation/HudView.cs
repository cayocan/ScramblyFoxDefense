using ScramblyFoxDefense.Gameplay;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>Greybox HUD: TextMesh labels parented to the camera, re-anchored when the aspect changes.</summary>
    public sealed class HudView
    {
        const float PlaneDistance = 2f;

        readonly Camera _camera;
        readonly Economy _economy;
        readonly TextMesh _coins;
        readonly TextMesh _phase;
        readonly TextMesh _banner;
        float _bannerTimer;

        public HudView(Camera camera, Economy economy, TextMesh coins, TextMesh phase, TextMesh banner)
        {
            _camera = camera;
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

        /// <summary>Anchors labels to the top of the view frustum at the HUD plane.</summary>
        public void Layout()
        {
            float halfHeight = Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * PlaneDistance;
            float halfWidth = halfHeight * _camera.aspect;
            _coins.transform.localPosition = new Vector3(halfWidth * 0.55f, halfHeight * 0.88f, PlaneDistance);
            _phase.transform.localPosition = new Vector3(-halfWidth * 0.55f, halfHeight * 0.88f, PlaneDistance);
            _banner.transform.localPosition = new Vector3(0f, halfHeight * 0.65f, PlaneDistance);
            float scale = halfWidth / 0.45f;
            _coins.transform.localScale = _phase.transform.localScale = _banner.transform.localScale = Vector3.one * scale;
        }

        void RefreshCoins() => _coins.text = $"Demo coins: {_economy.Wallet}";
    }
}
