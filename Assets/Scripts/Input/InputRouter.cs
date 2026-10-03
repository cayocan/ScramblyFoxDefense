using System;
using UnityEngine;

namespace ScramblyFoxDefense.Input
{
    /// <summary>
    /// Single input primitive: a tap. Only the first pointer counts; a canceled touch or focus loss
    /// raises Canceled so the current selection is dropped (GDD section 4, interrupted input).
    /// </summary>
    public sealed class InputRouter
    {
        int _activeFingerId = -1;
        bool _hadFocus = true;

        public event Action<Vector2> Tapped;
        public event Action Canceled;

        public void Tick()
        {
            if (_hadFocus != Application.isFocused)
            {
                _hadFocus = Application.isFocused;
                if (!_hadFocus) Cancel();
            }

            if (UnityEngine.Input.touchCount > 0)
            {
                ReadTouches();
                return;
            }
            if (UnityEngine.Input.GetMouseButtonDown(0)) Tapped?.Invoke(UnityEngine.Input.mousePosition);
        }

        void ReadTouches()
        {
            for (int i = 0; i < UnityEngine.Input.touchCount; i++)
            {
                var touch = UnityEngine.Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began && _activeFingerId < 0)
                {
                    _activeFingerId = touch.fingerId;
                    Tapped?.Invoke(touch.position);
                }
                else if (touch.fingerId == _activeFingerId)
                {
                    if (touch.phase == TouchPhase.Canceled) Cancel();
                    else if (touch.phase == TouchPhase.Ended) _activeFingerId = -1;
                }
            }
        }

        void Cancel()
        {
            _activeFingerId = -1;
            Canceled?.Invoke();
        }
    }
}
