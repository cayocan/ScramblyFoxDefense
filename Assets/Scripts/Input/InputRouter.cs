using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScramblyFoxDefense.Input
{
    /// <summary>
    /// Single input primitive: a tap. Only the first pointer counts; a canceled touch or focus loss
    /// raises Canceled so the current selection is dropped (GDD section 4, interrupted input).
    /// Tap handlers run in registration order and the first one returning true consumes the tap,
    /// so HUD buttons registered first win over board actions.
    /// </summary>
    public sealed class InputRouter
    {
        readonly List<Func<Vector2, bool>> _handlers = new List<Func<Vector2, bool>>();
        int _activeFingerId = -1;
        bool _hadFocus = true;

        public event Action Canceled;

        public void Register(Func<Vector2, bool> handler) => _handlers.Add(handler);

        public void Unregister(Func<Vector2, bool> handler) => _handlers.Remove(handler);

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
            if (UnityEngine.Input.GetMouseButtonDown(0)) Dispatch(UnityEngine.Input.mousePosition);
        }

        void ReadTouches()
        {
            for (int i = 0; i < UnityEngine.Input.touchCount; i++)
            {
                var touch = UnityEngine.Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began && _activeFingerId < 0)
                {
                    _activeFingerId = touch.fingerId;
                    Dispatch(touch.position);
                }
                else if (touch.fingerId == _activeFingerId)
                {
                    if (touch.phase == TouchPhase.Canceled) Cancel();
                    else if (touch.phase == TouchPhase.Ended) _activeFingerId = -1;
                }
            }
        }

        void Dispatch(Vector2 screenPoint)
        {
            // Copy: a handler may register or unregister others (e.g. a state change).
            foreach (var handler in _handlers.ToArray())
                if (handler(screenPoint)) return;
        }

        void Cancel()
        {
            _activeFingerId = -1;
            Canceled?.Invoke();
        }
    }
}
