// Reads the page state kept by the WebGL template (Assets/WebGLTemplates/Scrambly/index.html).
mergeInto(LibraryManager.library, {
  // 1 while the page is hidden or a phone is held in landscape.
  PageState_IsPaused: function () {
    return window.scramblyPaused ? 1 : 0;
  },

  // 1 once after the page comes back, so the game can drop that frame's delta (no time jump).
  PageState_ConsumeResumed: function () {
    if (!window.scramblyResumed) return 0;
    window.scramblyResumed = false;
    return 1;
  }
});
