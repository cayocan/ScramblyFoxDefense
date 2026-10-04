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
        Gameplay.TutorialGate _tutorial;
        bool _restarting;

        public RestartController(HudButton button, Camera camera)
        {
            _button = button;
            _camera = camera;
        }

        /// <summary>Set once the tower system exists (the restart button is registered first in the tap chain).</summary>
        public void BlockDuring(Gameplay.TutorialGate tutorial) => _tutorial = tutorial;

        public bool HandleTap(Vector2 screenPoint)
        {
            if (!_button.HitTest(_camera, screenPoint)) return false;
            if (_tutorial != null && _tutorial.Active) return true; // swallowed: finish the tutorial first
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
