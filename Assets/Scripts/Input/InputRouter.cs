using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScramblyFoxDefense.Input
{
    /// <summary>Receives the rest of a press that a tap handler captured (see InputRouter.Capture).</summary>
    public interface IDragTarget
    {
        void DragMoved(Vector2 screenPoint, bool touch);
        void DragReleased(Vector2 screenPoint, bool touch);
        void DragCanceled();
    }

    /// <summary>
    /// Two primitives: a tap, and a drag that a tap handler can start by capturing the press. Only the first
    /// pointer counts; a canceled touch or focus loss raises Canceled and drops any drag (GDD section 4,
    /// interrupted input). Tap handlers run in registration order and the first one returning true consumes
    /// the tap, so HUD buttons registered first win over board actions.
    /// </summary>
    public sealed class InputRouter
    {
        readonly List<Func<Vector2, bool>> _handlers = new List<Func<Vector2, bool>>();
        int _activeFingerId = -1;
        bool _hadFocus = true;
        bool _pressIsTouch;
        IDragTarget _drag;
        bool _dragIsTouch;

        public event Action Canceled;

        public bool Dragging => _drag != null;

        public void Register(Func<Vector2, bool> handler) => _handlers.Add(handler);

        public void Unregister(Func<Vector2, bool> handler) => _handlers.Remove(handler);

        /// <summary>Called from a tap handler: the current press becomes a drag that reports to the target.</summary>
        public void Capture(IDragTarget target)
        {
            CancelDrag();
            _drag = target;
            _dragIsTouch = _pressIsTouch;
        }

        /// <summary>
        /// Page hidden or phone rotated: Tick does not run, so a touch that ends meanwhile is never seen.
        /// Forget the active finger (or every new touch would be ignored as a second pointer) and drop any drag.
        /// </summary>
        public void Interrupt()
        {
            _activeFingerId = -1;
            CancelDrag();
        }

        /// <summary>Drops the current drag, if any (state change).</summary>
        public void CancelDrag()
        {
            if (_drag == null) return;
            var drag = _drag;
            _drag = null;
            drag.DragCanceled();
        }

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
            // The touch vanished without an Ended phase: never leave a touch drag hanging.
            if (_drag != null && _dragIsTouch) Cancel();

            Vector2 mouse = UnityEngine.Input.mousePosition;
            if (_drag != null)
            {
                // A release outside the canvas may never report a button-up: a button no longer held ends it too.
                if (UnityEngine.Input.GetMouseButtonUp(0) || !UnityEngine.Input.GetMouseButton(0)) Release(mouse);
                else _drag.DragMoved(mouse, false);
                return;
            }
            if (UnityEngine.Input.GetMouseButtonDown(0)) Dispatch(mouse, false);
        }

        void ReadTouches()
        {
            for (int i = 0; i < UnityEngine.Input.touchCount; i++)
            {
                var touch = UnityEngine.Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began && _activeFingerId < 0)
                {
                    _activeFingerId = touch.fingerId;
                    Dispatch(touch.position, true);
                }
                else if (touch.fingerId == _activeFingerId)
                {
                    if (touch.phase == TouchPhase.Canceled) Cancel();
                    else if (touch.phase == TouchPhase.Ended)
                    {
                        _activeFingerId = -1;
                        if (_drag != null) Release(touch.position);
                    }
                    else if (_drag != null) _drag.DragMoved(touch.position, true);
                }
            }
        }

        void Dispatch(Vector2 screenPoint, bool touch)
        {
            _pressIsTouch = touch;
            // Copy: a handler may register or unregister others (e.g. a state change).
            foreach (var handler in _handlers.ToArray())
                if (handler(screenPoint)) return;
        }

        void Release(Vector2 screenPoint)
        {
            var drag = _drag;
            _drag = null;
            drag.DragReleased(screenPoint, _dragIsTouch);
        }

        void Cancel()
        {
            _activeFingerId = -1;
            CancelDrag();
            Canceled?.Invoke();
        }
    }
}
