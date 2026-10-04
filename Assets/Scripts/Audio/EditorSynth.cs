#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScramblyFoxDefense.Audio
{
    /// <summary>
    /// Editor-only port of the page synth (Assets/WebGLTemplates/Scrambly/index.html, window.scramblySfx), so
    /// Play Mode is not silent. Same 12 cues and the same music loop. Compiled out of player builds: the web
    /// build keeps using Web Audio and ships no Unity audio code. Keep both tables in sync when tuning.
    /// </summary>
    [AddComponentMenu("")]
    public sealed class EditorSynth : MonoBehaviour
    {
        enum Wave { Sine, Triangle, Square, Saw }

        struct Voice
        {
            public Wave Wave;
            public double Start, Duration;   // seconds on the synth clock
            public double Freq, SlideTo;     // SlideTo 0 = no slide
            public float Gain;
            public double Phase;
            public bool Music;
        }

        const float MasterGain = 0.9f;
        const float MusicGain = 0.32f;
        const double Step = 60.0 / 100.0 / 2.0; // eighth notes at 100 BPM

        static readonly int[] Melody =
        {
            72, 0, 76, 0, 79, 0, 76, 74,   72, 0, 69, 0, 72, 0, 0, 0,
            69, 0, 72, 0, 77, 0, 76, 74,   74, 0, 79, 0, 74, 0, 0, 0
        };
        static readonly int[] Bass = { 48, 45, 41, 43 };

        readonly List<Voice> _voices = new List<Voice>();
        readonly List<Voice> _pending = new List<Voice>();
        readonly object _lock = new object();
        double _clock;          // advanced by the audio thread
        double _nextStep = 0.1;
        int _step;
        int _sampleRate;
        volatile bool _muted;
        volatile bool _paused;
        double _lastShot = -1;

        public bool Muted { get => _muted; set => _muted = value; }

        public static EditorSynth Create()
        {
            // Lives in the scene: a restart (scene reload) destroys it, so synths never stack up.
            var go = new GameObject("Editor Synth") { hideFlags = HideFlags.HideInHierarchy };
            go.AddComponent<AudioSource>().playOnAwake = false; // drives OnAudioFilterRead
            // The scene has no AudioListener on purpose (it would pull the audio module into the web build).
            if (FindFirstObjectByType<AudioListener>() == null && Camera.main != null) Camera.main.gameObject.AddComponent<AudioListener>();
            var synth = go.AddComponent<EditorSynth>();
            go.GetComponent<AudioSource>().Play();
            return synth;
        }

        void Awake() => _sampleRate = AudioSettings.outputSampleRate;

        // Silent while the game is paused (same rule as the page: no sound with the clocks stopped).
        void Update() => _paused = Time.timeScale == 0f;

        public void Play(Sound sound)
        {
            double now = _clock;
            if (sound == Sound.Shoot)
            {
                if (now - _lastShot < 0.07) return; // cap shot spam, like the page
                _lastShot = now;
            }
            lock (_lock)
            {
                switch (sound)
                {
                    case Sound.Select: Tone(now, 880, 0, 0.06, Wave.Triangle, 0.25f); break;
                    case Sound.Build: Tone(now, 330, 0, 0.12, Wave.Triangle, 0.35f, 660); Tone(now, 990, 0.08, 0.1, Wave.Sine, 0.2f); break;
                    case Sound.Upgrade: Arp(now, new[] { 523, 659, 784, 1047 }, 0.06, 0.12, 0.28f); break;
                    case Sound.Shoot: Tone(now, 1200, 0, 0.05, Wave.Square, 0.05f, 600); break;
                    case Sound.Defeat: Tone(now, 600, 0, 0.09, Wave.Sine, 0.3f, 300); Tone(now, 1568, 0.05, 0.12, Wave.Triangle, 0.18f); break;
                    case Sound.Leak: Tone(now, 220, 0, 0.25, Wave.Saw, 0.12f, 110); break;
                    case Sound.Wave: Tone(now, 392, 0, 0.14, Wave.Triangle, 0.3f); Tone(now, 587, 0.14, 0.2, Wave.Triangle, 0.3f); break;
                    case Sound.Unlock: Tone(now, 784, 0, 0.1, Wave.Sine, 0.3f); Tone(now, 1175, 0.09, 0.25, Wave.Sine, 0.3f); break;
                    case Sound.Coin: Tone(now, 1319, 0, 0.07, Wave.Square, 0.08f); Tone(now, 1976, 0.05, 0.12, Wave.Square, 0.08f); break;
                    case Sound.Cta: Arp(now, new[] { 523, 784, 1047 }, 0.08, 0.14, 0.3f); break;
                    case Sound.Deny: Tone(now, 180, 0, 0.12, Wave.Square, 0.12f); Tone(now, 150, 0.1, 0.14, Wave.Square, 0.12f); break;
                    case Sound.Fanfare: Arp(now, new[] { 523, 659, 784, 1047, 784, 1047 }, 0.1, 0.18, 0.26f); break;
                }
            }
        }

        void Arp(double now, int[] freqs, double gap, double duration, float gain)
        {
            for (int i = 0; i < freqs.Length; i++) Tone(now, freqs[i], i * gap, duration, Wave.Triangle, gain);
        }

        void Tone(double now, double freq, double delay, double duration, Wave wave, float gain, double slideTo = 0, bool music = false)
        {
            _pending.Add(new Voice { Wave = wave, Start = now + delay, Duration = duration, Freq = freq, SlideTo = slideTo, Gain = gain, Music = music });
        }

        static double Midi(int note) => 440.0 * Math.Pow(2.0, (note - 69) / 12.0);

        void QueueMusic(double until)
        {
            while (_nextStep < until)
            {
                int bar = _step / 8, beat = _step % 8;
                if (beat == 0 || beat == 4) Tone(0, Midi(Bass[bar] - (beat == 4 ? 0 : 12)), _nextStep, Step * 3.5, Wave.Triangle, 0.5f, 0, true);
                if (beat % 2 == 0) Tone(0, Midi(Bass[bar] + 16), _nextStep, Step * 1.6, Wave.Sine, 0.12f, 0, true);
                if (Melody[_step] != 0) Tone(0, Midi(Melody[_step]), _nextStep, Step * 1.8, Wave.Triangle, 0.32f, 0, true);
                _nextStep += Step;
                _step = (_step + 1) % Melody.Length;
            }
        }

        // Audio thread. The synth clock only advances while audible, so pause/mute freezes the music in place.
        void OnAudioFilterRead(float[] data, int channels)
        {
            if (_paused || _muted || _sampleRate == 0)
            {
                Array.Clear(data, 0, data.Length);
                return;
            }

            lock (_lock)
            {
                QueueMusic(_clock + 0.15);
                _voices.AddRange(_pending);
                _pending.Clear();
            }

            double dt = 1.0 / _sampleRate;
            int frames = data.Length / channels;
            for (int f = 0; f < frames; f++)
            {
                double t = _clock + f * dt;
                float sum = 0f;
                for (int i = 0; i < _voices.Count; i++)
                {
                    var v = _voices[i];
                    double local = t - v.Start;
                    if (local < 0 || local > v.Duration) continue;
                    double freq = v.SlideTo > 0 ? v.Freq * Math.Pow(v.SlideTo / v.Freq, local / v.Duration) : v.Freq;
                    v.Phase += freq * dt;
                    double p = v.Phase - Math.Floor(v.Phase);
                    float sample = v.Wave switch
                    {
                        Wave.Sine => (float)Math.Sin(p * 2 * Math.PI),
                        Wave.Triangle => (float)(4 * Math.Abs(p - 0.5) - 1),
                        Wave.Square => p < 0.5 ? 1f : -1f,
                        _ => (float)(2 * p - 1)
                    };
                    // Same envelope as the page: 12 ms attack, exponential-ish decay to silence.
                    float attack = (float)Math.Min(1.0, local / 0.012);
                    float decay = (float)Math.Pow(0.0001, local / v.Duration);
                    sum += sample * v.Gain * attack * decay * (v.Music ? MusicGain : 1f);
                    _voices[i] = v;
                }
                // Soft limiter (tanh) so the louder mix never clips, like the page's compressor.
                sum = (float)Math.Tanh(sum * MasterGain * 1.2f);
                for (int c = 0; c < channels; c++) data[f * channels + c] = sum;
            }
            _clock += frames * dt;
            _voices.RemoveAll(v => _clock - v.Start > v.Duration);
        }

        void OnDestroy() => _voices.Clear();
    }
}
#endif
