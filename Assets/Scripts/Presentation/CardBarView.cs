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
        float _shakeTime;

        public CardBarView(CardView[] cards, GameConfig config, Economy economy, HudLayout layout, Camera camera)
        {
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

        bool IsDimmed(int i) => _focus >= 0 && i != _focus;

        void Repaint()
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                bool dimmed = IsDimmed(i);
                Tint.Set(_cards[i].background, dimmed ? Dimmed : i == _selected ? Selected : Idle);
                var title = _cards[i].title.color;
                title.a = dimmed ? 0.4f : 1f;
                _cards[i].title.color = title;
                var face = _cards[i].root.Find("Face");
                if (face != null)
                    foreach (var renderer in face.GetComponentsInChildren<Renderer>()) Tint.Set(renderer, dimmed ? FaceDimmed : Color.white);
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
