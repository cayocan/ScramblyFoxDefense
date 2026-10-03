using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>Three reward locks in the top band; one opens after each wave (Play and progress).</summary>
    public sealed class LockBarView
    {
        const float Size = 30f;
        const float Gap = 10f;
        const float PopDuration = 0.4f;

        static readonly Color Locked = new Color(0.42f, 0.36f, 0.52f);
        static readonly Color Open = new Color32(0xF5, 0x83, 0x24, 0xFF);

        readonly Renderer[] _locks;
        readonly HudLayout _layout;
        readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();
        readonly float[] _popTime;

        public int UnlockedCount { get; private set; }

        public LockBarView(Renderer[] locks, HudLayout layout)
        {
            _locks = locks;
            _layout = layout;
            _popTime = new float[locks.Length];
            for (int i = 0; i < locks.Length; i++)
            {
                _popTime[i] = PopDuration;
                SetColor(i, Locked);
            }
        }

        public void Unlock(int index)
        {
            if (index < 0 || index >= _locks.Length || index < UnlockedCount) return;
            UnlockedCount = index + 1;
            SetColor(index, Open);
            _popTime[index] = 0f;
        }

        public void Layout()
        {
            float total = _locks.Length * Size + (_locks.Length - 1) * Gap;
            for (int i = 0; i < _locks.Length; i++)
            {
                float x = -total * 0.5f + Size * 0.5f + i * (Size + Gap);
                _layout.Place(_locks[i].transform, new Vector2(0.5f, 1f), new Vector2(x, -26f), Size);
            }
        }

        public void Tick(float deltaTime)
        {
            for (int i = 0; i < _locks.Length; i++)
            {
                if (_popTime[i] >= PopDuration) continue;
                _popTime[i] += deltaTime;
                float t = Mathf.Clamp01(_popTime[i] / PopDuration);
                float pop = 1f + Mathf.Sin(t * Mathf.PI) * 0.5f;
                _locks[i].transform.localScale = Vector3.one * Size * _layout.PixelUnit * pop;
            }
        }

        void SetColor(int index, Color color)
        {
            _block.SetColor("_Color", color);
            _locks[index].SetPropertyBlock(_block);
        }
    }
}
