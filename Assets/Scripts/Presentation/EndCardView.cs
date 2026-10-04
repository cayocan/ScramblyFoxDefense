using System;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>
    /// Final screen (Redeem → invitation): result title, demo coins collected, the three generic
    /// reward cards, the product line, "Explore Scrambly" CTA, "Play again" and the demo disclaimer.
    /// </summary>
    public sealed class EndCardView
    {
        const float RevealStagger = 0.25f;
        const float RevealDuration = 0.35f;
        const float ToastSeconds = 2.5f;

        readonly Transform _panel;
        readonly TextMesh _title;
        readonly TextMesh _collected;
        readonly TextMesh _toast;
        readonly Transform[] _rewards;
        readonly HudButton _cta;
        readonly HudButton _playAgain;
        readonly HudLayout _layout;
        readonly Camera _camera;
        float _time = -1f;
        float _toastTimer;

        public event Action CtaClicked;
        public event Action PlayAgainClicked;

        public EndCardView(Transform panel, TextMesh title, TextMesh collected, TextMesh toast, Transform[] rewards,
            HudButton cta, HudButton playAgain, HudLayout layout, Camera camera)
        {
            _panel = panel;
            _title = title;
            _collected = collected;
            _toast = toast;
            _rewards = rewards;
            _cta = cta;
            _playAgain = playAgain;
            _layout = layout;
            _camera = camera;
            _panel.gameObject.SetActive(false);
            _toast.gameObject.SetActive(false);
        }

        public bool IsShown => _panel.gameObject.activeSelf;

        public void Show(string title, string details)
        {
            _title.text = title;
            _collected.text = details;
            _panel.gameObject.SetActive(true);
            foreach (var reward in _rewards) reward.localScale = Vector3.zero;
            _time = 0f;
        }

        public void Layout() => _layout.Place(_panel, new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), 1f);

        /// <summary>Tap handler: CTA and Play again while the end card is up.</summary>
        public bool HandleTap(Vector2 screenPoint)
        {
            if (!IsShown) return false;
            if (_cta.HitTest(_camera, screenPoint))
            {
                ShowToast();
                CtaClicked?.Invoke();
                return true;
            }
            if (_playAgain.HitTest(_camera, screenPoint))
            {
                PlayAgainClicked?.Invoke();
                return true;
            }
            return true; // the panel blocks taps to the board behind it
        }

        public void Tick(float deltaTime)
        {
            if (_time >= 0f)
            {
                _time += deltaTime;
                for (int i = 0; i < _rewards.Length; i++)
                {
                    float t = Mathf.Clamp01((_time - i * RevealStagger) / RevealDuration);
                    // Overshoot pop: 0 → 1.15 → 1.
                    float scale = t < 0.7f ? Mathf.Lerp(0f, 1.15f, t / 0.7f) : Mathf.Lerp(1.15f, 1f, (t - 0.7f) / 0.3f);
                    _rewards[i].localScale = Vector3.one * scale;
                }
            }
            if (_toastTimer > 0f)
            {
                _toastTimer -= deltaTime;
                if (_toastTimer <= 0f) _toast.gameObject.SetActive(false);
            }
        }

        void ShowToast()
        {
            _toast.text = "CTA clicked — demo only";
            _toast.gameObject.SetActive(true);
            _toastTimer = ToastSeconds;
        }
    }
}
