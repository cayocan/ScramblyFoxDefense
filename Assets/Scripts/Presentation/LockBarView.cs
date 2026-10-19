using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>
    /// Three reward locks in the top band; one opens after each wave (Play and progress). They double as wave
    /// pips: the current wave's lock is tinted orange and pulses.
    /// </summary>
    public sealed class LockBarView
    {
        const float Size = 30f;
        const float Gap = 10f;
        const float PopDuration = 0.4f;

        static readonly Color Locked = new Color(0.42f, 0.36f, 0.52f);
        static readonly Color Open = new Color32(0xF5, 0x83, 0x24, 0xFF);
        static readonly Color Perfect = new Color(1f, 0.8f, 0.2f);
        static readonly Color Current = new Color32(0xFF, 0xB0, 0x6A, 0xFF);

        readonly SpriteRenderer[] _locks;
        readonly Sprite _openSprite;
        readonly HudLayout _layout;
        readonly float[] _popTime;
        int _current = -1;
        float _time;

        public int UnlockedCount { get; private set; }

        public LockBarView(SpriteRenderer[] locks, Sprite openSprite, HudLayout layout)
        {
            _locks = locks;
            _openSprite = openSprite;
            _layout = layout;
            _popTime = new float[locks.Length];
            for (int i = 0; i < locks.Length; i++)
            {
                _popTime[i] = PopDuration;
                SetColor(i, Locked);
            }
        }

        public void Unlock(int index, bool perfect)
        {
            if (index < 0 || index >= _locks.Length || index < UnlockedCount) return;
            UnlockedCount = index + 1;
            if (index == _current) _current = -1;
            _locks[index].sprite = _openSprite;
            SetColor(index, perfect ? Perfect : Open);
            _popTime[index] = 0f;
        }

        /// <summary>Highlights the wave being played (-1 = none).</summary>
        public void SetCurrent(int index)
        {
            if (_current >= UnlockedCount && _current >= 0 && _current < _locks.Length) SetColor(_current, Locked);
            _current = index;
            if (_current >= UnlockedCount && _current >= 0 && _current < _locks.Length) SetColor(_current, Current);
        }

        public void Layout()
        {
            float total = _locks.Length * Size + (_locks.Length - 1) * Gap;
            for (int i = 0; i < _locks.Length; i++)
            {
                float x = -total * 0.5f + Size * 0.5f + i * (Size + Gap);
                // Sprites are 64 px at 1 px per unit: scale them down to the lock size.
                // Second row of the top band, under the buttons and the balance.
                _layout.Place(_locks[i].transform, new Vector2(0.5f, 1f), new Vector2(x, -64f), Size / 64f);
            }
        }

        public void Tick(float deltaTime)
        {
            _time += deltaTime;
            for (int i = 0; i < _locks.Length; i++)
            {
                if (i == _current && i >= UnlockedCount)
                {
                    float pulse = 1.12f + 0.12f * Mathf.Sin(_time * 6f);
                    _locks[i].transform.localScale = Vector3.one * Size / 64f * _layout.PixelUnit * pulse;
                    continue;
                }
                if (_popTime[i] >= PopDuration) continue;
                _popTime[i] += deltaTime;
                float t = Mathf.Clamp01(_popTime[i] / PopDuration);
                float pop = 1f + Mathf.Sin(t * Mathf.PI) * 0.5f;
                _locks[i].transform.localScale = Vector3.one * Size / 64f * _layout.PixelUnit * pop;
            }
        }

        void SetColor(int index, Color color) => Tint.Set(_locks[index], color);
    }
}
