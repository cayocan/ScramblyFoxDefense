using System.Collections.Generic;
using ScramblyFoxDefense.Gameplay;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>Upgrade cost badge above each tower; greyed out when the wallet can't cover it.</summary>
    public sealed class TowerBadges
    {
        const float Height = 1.25f;

        static readonly Color Affordable = new Color32(0xF5, 0x83, 0x24, 0xFF);
        static readonly Color TooExpensive = new Color(0.6f, 0.58f, 0.65f);

        readonly TowerSystem _towers;
        readonly Economy _economy;
        readonly GameObject _badgePrefab;
        readonly Transform _camera;
        readonly Dictionary<Tower, TextMesh> _badges = new Dictionary<Tower, TextMesh>();

        public TowerBadges(TowerSystem towers, Economy economy, GameObject badgePrefab, Transform camera)
        {
            _towers = towers;
            _economy = economy;
            _badgePrefab = badgePrefab;
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

        void OnBuilt(Tower tower)
        {
            var badge = Object.Instantiate(_badgePrefab, tower.Transform).GetComponent<TextMesh>();
            badge.transform.localPosition = Vector3.up * Height;
            badge.transform.rotation = _camera.rotation;
            _badges[tower] = badge;
            Refresh(tower);
        }

        void Refresh(Tower tower)
        {
            if (!_badges.TryGetValue(tower, out var badge)) return;
            badge.transform.localPosition = Vector3.up * (Height + tower.Level * tower.BaseHeight);
            bool maxed = tower.IsMaxLevel;
            badge.gameObject.SetActive(!maxed);
            if (maxed) return;
            int cost = tower.NextLevelCost;
            badge.text = $"UP {cost}";
            badge.color = _economy.CanAfford(cost) ? Affordable : TooExpensive;
        }

        void RefreshAll()
        {
            foreach (var tower in _towers.Towers) Refresh(tower);
        }
    }
}
