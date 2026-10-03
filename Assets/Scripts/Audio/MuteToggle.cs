using ScramblyFoxDefense.Presentation;
using UnityEngine;

namespace ScramblyFoxDefense.Audio
{
    /// <summary>Sound on/off button in the top band (required by the brief once the game has audio).</summary>
    public sealed class MuteToggle
    {
        readonly IAudioService _audio;
        readonly HudButton _button;
        readonly SpriteRenderer _icon;
        readonly Sprite _onSprite;
        readonly Sprite _offSprite;
        readonly Camera _camera;

        public MuteToggle(IAudioService audio, HudButton button, SpriteRenderer icon, Sprite onSprite, Sprite offSprite, Camera camera)
        {
            _audio = audio;
            _button = button;
            _icon = icon;
            _onSprite = onSprite;
            _offSprite = offSprite;
            _camera = camera;
            Refresh();
        }

        public bool HandleTap(Vector2 screenPoint)
        {
            if (!_button.HitTest(_camera, screenPoint)) return false;
            _audio.Muted = !_audio.Muted;
            if (!_audio.Muted) _audio.Play(Sound.Select);
            Refresh();
            return true;
        }

        void Refresh() => _icon.sprite = _audio.Muted ? _offSprite : _onSprite;
    }
}
