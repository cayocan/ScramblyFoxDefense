# Credits

## Assets

| Asset | Author | License | Used for |
|---|---|---|---|
| [Tower Defense Kit](https://kenney.nl/assets/tower-defense-kit) (v2.1) | Kenney | CC0 1.0 | Board tiles, path, tower bases, vault, trees/rocks/crystals decoration |
| [Cube Pets](https://kenney.nl/assets/cube-pets) | Kenney | CC0 1.0 | Fox, dog and cat (defenders); lion, tiger and polar bear (predators) |
| [UI Pack](https://kenney.nl/assets/ui-pack) (v2.0) | Kenney | CC0 1.0 (`source/Assets/Art/UI/Kenney-UI-Pack-License.txt`) | HUD buttons, cards, panels (4 grey sprites tinted to the palette) |
| [Interface Sounds](https://kenney.nl/assets/interface-sounds), [Impact Sounds](https://kenney.nl/assets/impact-sounds), [Music Jingles](https://kenney.nl/assets/music-jingles) | Kenney | CC0 1.0 (`source/Assets/WebGLTemplates/Scrambly/sfx/License-*.txt`, also next to `index.html` in `sfx/`) | 10 one-shots and jingles (Stage 2): UI click, place, upgrade, hit, wave clear, coin, CTA, error, unlock and end-card jingles |
| [Children's March Theme](https://opengameart.org/content/childrens-march-theme) | CleytonKauffman | CC0 1.0 | Background music loop (Stage 2), re-encoded mono 32 kHz OGG (`sfx/music.ogg`, 345 KB) |
| [Fredoka](https://fonts.google.com/specimen/Fredoka) | The Fredoka Project Authors | SIL OFL 1.1 (`source/Assets/Art/Fonts/OFL.txt`) | All in-game text (weight 600 instance, subset) |

The palette recolors, the other UI sprites (icons, padlocks, the pointing, open and grabbing hands; `tools/draw-hands.py`), the shot sounds and the music loop are made for this project (generated in code, the audio synthesized at runtime).

`docs/stage2-audio-candidates` holds the Kenney CC0 short list the Stage 2 sounds were picked from; only the 10 files in `sfx/` ship.

The license files ship next to the models in `source/Assets/Art/*/License.txt`. Only the models that are used are included.

## Engine, packages and tools

| Item | License | Notes |
|---|---|---|
| Unity 6000.3.19f1 | Unity terms | Engine, WebGL build |
| Unity glTFast (`com.unity.cloud.gltfast`) | Apache 2.0 | Editor import of `.glb` models; its runtime code is stripped from the build |
| Unity Pipeline package (`com.unity.pipeline`) + Unity CLI | Unity terms | Editor automation only, not part of the build |
| puppeteer-core | Apache 2.0 | Dev-only browser test (`tools/browser-test`), not shipped |
| fontTools, Pillow (Python) | MIT / HPND | Dev-only: font subset, palette recolor and sprite generation, not shipped |

No third-party code is reused in the game itself.

## AI use

Built with Claude Code (Anthropic) as a pair programmer: code, editor automation through the Unity CLI and Pipeline, size investigations, simulations and browser tests. Decisions, corrections and rejected AI outputs are logged in `docs/rag/decisions.md` and summarized in `PROJECT_NOTE.md`.
