using System;
using ScramblyFoxDefense.Config;
using ScramblyFoxDefense.Gameplay;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>One "game card" in the bottom band: background quad, pet face, name and price.</summary>
    [Serializable]
    public sealed class CardView
    {
        public Transform root;
        public Renderer background;
        public TextMesh title;
        public TextMesh price;
    }

    /// <summary>Bottom band with the tower cards (Discover). Hit-tested in screen space.</summary>
    public sealed class CardBarView
    {
        const float CardWidth = 112f;
        const float CardHeight = 128f;
        const float Gap = 10f;
        const float BottomMargin = 14f;
        const float ShakeDuration = 0.35f;

        static readonly Color Idle = new Color32(0xFF, 0xF6, 0xE8, 0xFF);
        static readonly Color Selected = new Color32(0xF5, 0x83, 0x24, 0xFF);
        static readonly Color Affordable = new Color32(0x20, 0x13, 0x38, 0xFF);
        static readonly Color TooExpensive = new Color(0.55f, 0.5f, 0.6f);

        readonly CardView[] _cards;
        readonly GameConfig _config;
        readonly Economy _economy;
        readonly HudLayout _layout;
        readonly Camera _camera;
        readonly Vector3[] _rest;
        int _shaking = -1;
        int _focus = -1;
        int _selected = -1;
        static readonly Color Dimmed = new Color(0.62f, 0.6f, 0.68f, 0.45f);
        static readonly Color FaceDimmed = new Color(0.55f, 0.53f, 0.6f);
        static readonly Color BorderIdle = new Color32(0x78, 0x45, 0xD8, 0xFF);
        static readonly Color BorderSelected = new Color(1f, 0.85f, 0.35f);
        static readonly Color Silhouette = new Color(0.16f, 0.1f, 0.27f);
        const float UnlockDuration = 0.65f;
        float _shakeTime;

        readonly TowerUnlocks _unlocks;
        readonly SpriteRenderer[] _lockIcons;
        readonly float[] _unlockTime;
        readonly bool[] _pulsing;
        float _time;

        public CardBarView(CardView[] cards, GameConfig config, Economy economy, HudLayout layout, Camera camera, TowerUnlocks unlocks, Sprite lockSprite)
        {
            _unlocks = unlocks;
            _lockIcons = new SpriteRenderer[cards.Length];
            _unlockTime = new float[cards.Length];
            _pulsing = new bool[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                _unlockTime[i] = -1f;
                // Padlock over the pet face of a locked card (card roots use one reference pixel per unit).
                var icon = new GameObject("Lock", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
                icon.transform.SetParent(cards[i].root, false);
                // In front of the 3D pet face, which stands closer to the camera than the card (like the tutorial hand).
                icon.transform.localPosition = new Vector3(0f, 22f, -140f);
                icon.transform.localScale = Vector3.one * 46f / 64f;
                icon.sprite = lockSprite;
                icon.color = new Color32(0xFF, 0xF6, 0xE8, 0xFF);
                icon.sortingOrder = 8;
                _lockIcons[i] = icon;
            }
            _cards = cards;
            _config = config;
            _economy = economy;
            _layout = layout;
            _camera = camera;
            _rest = new Vector3[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].title.text = config.towers[i].displayName;
                cards[i].price.text = config.towers[i].levels[0].cost.ToString();
            }
            _economy.Changed += RefreshPrices;
            RefreshPrices();
            SetSelected(-1);
        }

        public void Dispose() => _economy.Changed -= RefreshPrices;

        public void SetVisible(bool visible)
        {
            foreach (var card in _cards) card.root.gameObject.SetActive(visible);
        }

        public void Layout()
        {
            float totalWidth = _cards.Length * CardWidth + (_cards.Length - 1) * Gap;
            for (int i = 0; i < _cards.Length; i++)
            {
                float x = -totalWidth * 0.5f + CardWidth * 0.5f + i * (CardWidth + Gap);
                _layout.Place(_cards[i].root, new Vector2(0.5f, 0f), new Vector2(x, BottomMargin + CardHeight * 0.5f), 1f);
                _rest[i] = _cards[i].root.localPosition;
            }
        }

        /// <summary>Index of the card under the screen point, or -1.</summary>
        public int HitTest(Vector2 screenPoint)
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                if (!_cards[i].root.gameObject.activeInHierarchy) continue;
                Vector3 center = _camera.WorldToScreenPoint(_cards[i].root.position);
                // Card roots are scaled to one reference pixel per local unit (see Layout).
                Vector3 corner = _camera.WorldToScreenPoint(_cards[i].root.TransformPoint(new Vector3(CardWidth * 0.5f, CardHeight * 0.5f, 0f)));
                Vector2 half = new Vector2(Mathf.Abs(corner.x - center.x), Mathf.Abs(corner.y - center.y));
                if (Mathf.Abs(screenPoint.x - center.x) <= half.x && Mathf.Abs(screenPoint.y - center.y) <= half.y) return i;
            }
            return -1;
        }

        public void SetSelected(int index)
        {
            _selected = index;
            Repaint();
        }

        /// <summary>Tutorial: every card except the focused one is greyed out (-1 = all normal).</summary>
        public void SetFocus(int index)
        {
            _focus = index;
            Repaint();
        }

        bool IsDimmed(int i) => (_focus >= 0 && i != _focus) || !_unlocks.IsUnlocked(i);

        /// <summary>Milestone: the card loses its lock, slides in with an overshoot and pulses until first used.</summary>
        public void PlayUnlock(int index)
        {
            _unlockTime[index] = 0f;
            _pulsing[index] = true;
            Repaint();
        }

        public void MarkUsed(int index) => _pulsing[index] = false;

        /// <summary>World position of a card's pet face (for effects).</summary>
        public Vector3 FacePosition(int index) => _cards[index].root.TransformPoint(new Vector3(0f, 18f, 0f));

        void Repaint()
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                bool dimmed = IsDimmed(i);
                bool locked = !_unlocks.IsUnlocked(i);
                _lockIcons[i].gameObject.SetActive(locked);
                Tint.Set(_cards[i].background, dimmed ? Dimmed : i == _selected ? Selected : Idle);
                var border = _cards[i].root.Find("Border");
                if (border != null) Tint.Set(border.GetComponent<Renderer>(), dimmed ? Dimmed : i == _selected ? BorderSelected : BorderIdle);
                var title = _cards[i].title.color;
                title.a = dimmed ? 0.4f : 1f;
                _cards[i].title.color = title;
                var face = _cards[i].root.Find("Face");
                if (face != null)
                    foreach (var renderer in face.GetComponentsInChildren<Renderer>()) Tint.Set(renderer, locked ? Silhouette : dimmed ? FaceDimmed : Color.white);
            }
            RefreshPrices();
        }

        /// <summary>Not enough coins: the card shakes (GDD section 4).</summary>
        public void Shake(int index)
        {
            if (_shaking >= 0) _cards[_shaking].root.localPosition = _rest[_shaking];
            _shaking = index;
            _shakeTime = 0f;
        }

        public void Tick(float deltaTime)
        {
            _time += deltaTime;
            for (int i = 0; i < _cards.Length; i++)
            {
                float scale = 1f;
                if (_unlockTime[i] >= 0f)
                {
                    _unlockTime[i] += deltaTime;
                    float t = Mathf.Clamp01(_unlockTime[i] / UnlockDuration);
                    // Slides up from below with a back-out overshoot on the scale.
                    float back = 1f + 2.7f * Mathf.Pow(t - 1f, 3f) + 1.7f * Mathf.Pow(t - 1f, 2f);
                    scale = Mathf.LerpUnclamped(0.6f, 1f, back);
                    _cards[i].root.localPosition = _rest[i] + Vector3.down * (1f - t) * 90f * _layout.PixelUnit;
                    if (t >= 1f) _unlockTime[i] = -1f;
                }
                else if (_pulsing[i]) scale = 1f + 0.05f * (0.5f + 0.5f * Mathf.Sin(_time * 7f));
                _cards[i].root.localScale = Vector3.one * _layout.PixelUnit * scale;
            }
            if (_shaking < 0) return;
            _shakeTime += deltaTime;
            var card = _cards[_shaking];
            if (_shakeTime >= ShakeDuration)
            {
                card.root.localPosition = _rest[_shaking];
                _shaking = -1;
                return;
            }
            float offset = Mathf.Sin(_shakeTime * 60f) * (1f - _shakeTime / ShakeDuration) * 6f * _layout.PixelUnit;
            card.root.localPosition = _rest[_shaking] + Vector3.right * offset;
        }

        void RefreshPrices()
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                var color = _economy.CanAfford(_config.towers[i].levels[0].cost) ? Affordable : TooExpensive;
                if (IsDimmed(i)) color.a = 0.4f;
                _cards[i].price.color = color;
            }
        }
    }
}
