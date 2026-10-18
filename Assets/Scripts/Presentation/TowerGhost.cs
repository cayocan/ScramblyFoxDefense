using ScramblyFoxDefense.Config;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>
    /// Drag preview: a translucent copy of each tower prefab plus its range ring, green where it can be built and
    /// red where it cannot. One copy per tower type is built up front and only shown or hidden, so a cancel never
    /// leaves an object behind.
    /// </summary>
    public sealed class TowerGhost
    {
        static readonly Color Valid = new Color(0.35f, 0.95f, 0.45f, 0.55f);
        static readonly Color Invalid = new Color(1f, 0.3f, 0.3f, 0.55f);
        const float RingAlpha = 0.6f;

        readonly GameObject[] _ghosts;
        readonly Renderer[][] _renderers;
        readonly SpriteRenderer _ring;
        readonly float[] _ranges;
        int _shown = -1;

        public TowerGhost(GameConfig config, Material ghostMaterial, Sprite ringSprite, Transform root)
        {
            int count = config.towers.Length;
            _ghosts = new GameObject[count];
            _renderers = new Renderer[count][];
            _ranges = new float[count];
            for (int i = 0; i < count; i++)
            {
                var ghost = Object.Instantiate(config.towers[i].prefab, root);
                ghost.name = "Ghost " + config.towers[i].displayName;
                _renderers[i] = ghost.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in _renderers[i])
                {
                    var materials = new Material[renderer.sharedMaterials.Length];
                    for (int m = 0; m < materials.Length; m++) materials[m] = ghostMaterial;
                    renderer.sharedMaterials = materials;
                }
                ghost.SetActive(false);
                _ghosts[i] = ghost;
                _ranges[i] = config.towers[i].levels[0].range;
            }

            _ring = new GameObject("Ghost Range", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            _ring.transform.SetParent(root, false);
            _ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // flat on the ground
            _ring.sprite = ringSprite;
            _ring.sortingOrder = -10;
            _ring.gameObject.SetActive(false);
        }

        public bool Visible => _shown >= 0;

        public void Show(int towerIndex, Vector3 position, bool valid)
        {
            if (_shown != towerIndex)
            {
                Hide();
                _shown = towerIndex;
                _ghosts[towerIndex].SetActive(true);
                _ring.gameObject.SetActive(true);
                // The ring sprite is 64 px at one pixel per unit: 64 / 64 = 1 unit across.
                _ring.transform.localScale = Vector3.one * (2f * _ranges[towerIndex]) / 64f;
            }
            _ghosts[towerIndex].transform.position = position;
            _ring.transform.position = position + Vector3.up * 0.06f;

            var color = valid ? Valid : Invalid;
            foreach (var renderer in _renderers[towerIndex]) Tint.Set(renderer, color);
            color.a = RingAlpha;
            _ring.color = color;
        }

        public void Hide()
        {
            if (_shown < 0) return;
            _ghosts[_shown].SetActive(false);
            _ring.gameObject.SetActive(false);
            _shown = -1;
        }
    }
}
