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
        readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();
        readonly Vector3[] _rest;
        int _shaking = -1;
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
            for (int i = 0; i < _cards.Length; i++)
            {
                _block.SetColor("_Color", i == index ? Selected : Idle);
                _cards[i].background.SetPropertyBlock(_block);
            }
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
                _cards[i].price.color = _economy.CanAfford(_config.towers[i].levels[0].cost) ? Affordable : TooExpensive;
        }
    }
}
