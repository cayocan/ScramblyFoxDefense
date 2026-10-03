using ScramblyFoxDefense.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScramblyFoxDefense.Core
{
    /// <summary>
    /// Restart = reload the scene, which rebuilds the whole object graph (no static state survives).
    /// Guarded so repeated taps during the reload never queue a second load.
    /// </summary>
    public sealed class RestartController
    {
        readonly HudButton _button;
        readonly Camera _camera;
        bool _restarting;

        public RestartController(HudButton button, Camera camera)
        {
            _button = button;
            _camera = camera;
        }

        public bool HandleTap(Vector2 screenPoint)
        {
            if (!_button.HitTest(_camera, screenPoint)) return false;
            Restart();
            return true;
        }

        public void Restart()
        {
            if (_restarting) return;
            _restarting = true;
            _button.SetVisible(false);
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
