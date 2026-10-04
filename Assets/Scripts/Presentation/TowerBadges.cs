using System.Collections.Generic;
using ScramblyFoxDefense.Gameplay;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>
    /// Upgrade cost badge above each tower. When the wallet covers the next level the tower calls for attention:
    /// the badge pulses, an orange ring pulses on the ground and an arrow bobs above it. Greyed out otherwise.
    /// </summary>
    public sealed class TowerBadges
    {
        const float Height = 1.25f;
        const float ArrowGap = 0.32f;

        static readonly Color Affordable = new Color32(0xF5, 0x83, 0x24, 0xFF);
        static readonly Color TooExpensive = new Color(0.6f, 0.58f, 0.65f);

        sealed class Badge
        {
            public TextMesh Text;
            public Vector3 TextScale;
            public SpriteRenderer Ring;
            public SpriteRenderer Arrow;
            public bool Ready;
        }

        readonly TowerSystem _towers;
        readonly Economy _economy;
        readonly GameObject _badgePrefab;
        readonly Sprite _ringSprite;
        readonly Sprite _arrowSprite;
        readonly Transform _camera;
        readonly Dictionary<Tower, Badge> _badges = new Dictionary<Tower, Badge>();
        bool _hidden;
        float _time;

        public TowerBadges(TowerSystem towers, Economy economy, GameObject badgePrefab, Sprite ringSprite, Sprite arrowSprite, Transform camera)
        {
            _towers = towers;
            _economy = economy;
            _badgePrefab = badgePrefab;
            _ringSprite = ringSprite;
            _arrowSprite = arrowSprite;
            _camera = camera;
            _towers.Built += OnBuilt;
            _towers.Upgraded += Refresh;
            _economy.Changed += RefreshAll;
        }

        public void Dispose()
        {
            _towers.Built -= OnBuilt;
            _towers.Upgraded -= Refresh;
            _economy.Changed -= RefreshAll;
        }

        /// <summary>Hidden for Redeem and the end card so world text never shows through the panel.</summary>
        public void HideAll()
        {
            _hidden = true;
            foreach (var badge in _badges.Values) SetVisible(badge, false, false);
        }

        public void Tick(float deltaTime)
        {
            if (_hidden) return;
            _time += deltaTime;
            float pulse = 0.5f + 0.5f * Mathf.Sin(_time * 7f);
            foreach (var pair in _badges)
            {
                var badge = pair.Value;
                if (!badge.Ready) continue;
                badge.Text.transform.localScale = badge.TextScale * (1f + 0.22f * pulse);
                badge.Ring.transform.localScale = Vector3.one * (1.15f + 0.25f * pulse) / 64f;
                var ringColor = Affordable;
                ringColor.a = 0.45f + 0.5f * pulse;
                badge.Ring.color = ringColor;
                var arrow = badge.Arrow.transform;
                arrow.localPosition = Vector3.up * (Height + pair.Key.Level * pair.Key.BaseHeight + ArrowGap + 0.12f * pulse);
                arrow.rotation = _camera.rotation;
            }
        }

        void OnBuilt(Tower tower)
        {
            var text = Object.Instantiate(_badgePrefab, tower.Transform).GetComponent<TextMesh>();
            text.transform.localPosition = Vector3.up * Height;
            text.transform.rotation = _camera.rotation;

            var ring = new GameObject("Upgrade Ring", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            ring.transform.SetParent(tower.Transform, false);
            ring.transform.localPosition = Vector3.up * 0.06f;
            ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // flat on the ground
            ring.sprite = _ringSprite;
            ring.sortingOrder = -10;

            var arrow = new GameObject("Upgrade Arrow", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            arrow.transform.SetParent(tower.Transform, false);
            arrow.transform.localScale = Vector3.one * 0.32f / 64f;
            arrow.sprite = _arrowSprite;
            arrow.color = Affordable;
            arrow.sortingOrder = -8;

            _badges[tower] = new Badge { Text = text, TextScale = text.transform.localScale, Ring = ring, Arrow = arrow };
            Refresh(tower);
        }

        void Refresh(Tower tower)
        {
            if (_hidden || !_badges.TryGetValue(tower, out var badge)) return;
            badge.Text.transform.localPosition = Vector3.up * (Height + tower.Level * tower.BaseHeight);
            badge.Text.transform.localScale = badge.TextScale;
            bool maxed = tower.IsMaxLevel;
            badge.Ready = !maxed && _economy.CanAfford(tower.NextLevelCost);
            SetVisible(badge, !maxed, badge.Ready);
            if (maxed) return;
            badge.Text.text = $"UP {tower.NextLevelCost}";
            badge.Text.color = badge.Ready ? Affordable : TooExpensive;
        }

        static void SetVisible(Badge badge, bool text, bool ready)
        {
            badge.Text.gameObject.SetActive(text);
            badge.Ring.gameObject.SetActive(ready);
            badge.Arrow.gameObject.SetActive(ready);
        }

        void RefreshAll()
        {
            foreach (var tower in _towers.Towers) Refresh(tower);
        }
    }
}
