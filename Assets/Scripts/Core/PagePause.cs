using System.Runtime.InteropServices;
using UnityEngine;

namespace ScramblyFoxDefense.Core
{
    /// <summary>Browser page state: hidden tab or phone held in landscape.</summary>
    public interface IPageState
    {
        bool IsPaused { get; }
        /// <summary>True once right after the page comes back from a pause.</summary>
        bool ConsumeResumed();
    }

    /// <summary>Reads the flags kept by the WebGL template through PageState.jslib.</summary>
    public sealed class WebPageState : IPageState
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern int PageState_IsPaused();
        [DllImport("__Internal")] static extern int PageState_ConsumeResumed();

        public bool IsPaused => PageState_IsPaused() != 0;
        public bool ConsumeResumed() => PageState_ConsumeResumed() != 0;
#else
        public bool IsPaused => false;
        public bool ConsumeResumed() => false;
#endif
    }

    /// <summary>
    /// Pauses every clock while the page is hidden (brief: no time jump on return). All gameplay uses
    /// scaled time, so timeScale 0 freezes spawns, movement, towers, tweens and animations at once.
    /// The first frame after a resume contributes no time, on top of Time.maximumDeltaTime.
    /// </summary>
    public sealed class PagePause
    {
        readonly IPageState _page;

        public bool IsPaused { get; private set; }

        public PagePause(IPageState page)
        {
            _page = page;
        }

        /// <summary>Returns the delta the game should advance this frame.</summary>
        public float Filter(float deltaTime)
        {
            bool paused = _page.IsPaused;
            bool resumed = _page.ConsumeResumed();
            if (paused != IsPaused) Debug.Log(paused ? $"[PagePause] paused at t={Time.time:F2}" : $"[PagePause] resumed at t={Time.time:F2}");
            IsPaused = paused;
            Time.timeScale = IsPaused ? 0f : 1f;
            return IsPaused || resumed ? 0f : deltaTime;
        }
    }
}
