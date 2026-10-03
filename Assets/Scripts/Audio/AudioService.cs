using System.Runtime.InteropServices;

namespace ScramblyFoxDefense.Audio
{
    /// <summary>Sound ids; must match the synth table in the WebGL template (window.scramblySfx).</summary>
    public enum Sound
    {
        Select = 0,
        Build = 1,
        Upgrade = 2,
        Shoot = 3,
        Defeat = 4,
        Leak = 5,
        Wave = 6,
        Unlock = 7,
        Coin = 8,
        Cta = 9,
        Deny = 10,
        Fanfare = 11
    }

    public interface IAudioService
    {
        bool Muted { get; set; }
        void Play(Sound sound);
    }

    /// <summary>
    /// Web Audio synth in the page (no clips, no Unity audio module in the build). The page unlocks audio on
    /// the first gesture and keeps it suspended while hidden, so calls before that are simply dropped.
    /// </summary>
    public sealed class WebAudioService : IAudioService
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void Sfx_Play(int id);
        [DllImport("__Internal")] static extern void Sfx_SetMuted(int muted);
        [DllImport("__Internal")] static extern int Sfx_IsMuted();

        public bool Muted
        {
            get => Sfx_IsMuted() != 0;
            set => Sfx_SetMuted(value ? 1 : 0);
        }

        public void Play(Sound sound) => Sfx_Play((int)sound);
#else
        // Editor and non-web builds: silent, but the mute toggle still works for testing the UI.
        public bool Muted { get; set; }

        public void Play(Sound sound) { }
#endif
    }
}
