// Bridge to the Web Audio synth in the WebGL template (window.scramblySfx).
mergeInto(LibraryManager.library, {
  Sfx_Play: function (id) {
    if (window.scramblySfx) window.scramblySfx.play(id);
  },

  // 0 at the intro, then the wave number: the music speeds up and adds layers.
  Sfx_SetMusicLevel: function (level) {
    if (window.scramblySfx && window.scramblySfx.setMusicLevel) window.scramblySfx.setMusicLevel(level);
  },

  Sfx_SetMuted: function (muted) {
    if (window.scramblySfx) window.scramblySfx.setMuted(muted !== 0);
  },

  // The page outlives in-game restarts, so the mute state lives here and the game reads it back.
  Sfx_IsMuted: function () {
    return window.scramblySfx && window.scramblySfx.isMuted() ? 1 : 0;
  }
});
