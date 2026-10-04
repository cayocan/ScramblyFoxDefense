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
#elif UNITY_EDITOR
        // Play Mode: C# port of the page synth. Mute is kept in SessionState so it survives restarts,
        // like the page keeps it across scene reloads.
        const string MutedKey = "Scrambly.EditorSynth.Muted";
        readonly EditorSynth _synth = EditorSynth.Create();

        public WebAudioService() => _synth.Muted = Muted;

        public bool Muted
        {
            get => UnityEditor.SessionState.GetBool(MutedKey, false);
            set
            {
                UnityEditor.SessionState.SetBool(MutedKey, value);
                _synth.Muted = value;
            }
        }

        public void Play(Sound sound) => _synth.Play(sound);
#else
        // Non-web players are not a target: silent.
        public bool Muted { get; set; }

        public void Play(Sound sound) { }
#endif
    }
}
