# Credits

## Assets

| Asset | Author | License | Used for |
|---|---|---|---|
| [Tower Defense Kit](https://kenney.nl/assets/tower-defense-kit) (v2.1) | Kenney | CC0 1.0 | Board tiles, path, tower bases, slot markers, vault, trees/rocks/crystals decoration |
| [Cube Pets](https://kenney.nl/assets/cube-pets) | Kenney | CC0 1.0 | Fox, dog and cat (defenders); lion, tiger and polar bear (predators) |
| [Fredoka](https://fonts.google.com/specimen/Fredoka) | The Fredoka Project Authors | SIL OFL 1.1 (`source/Assets/Art/Fonts/OFL.txt`) | All in-game text (weight 600 instance, subset) |

The palette recolors, UI sprites, sound effects and music loop are made for this project (generated in code, the audio synthesized at runtime), no third-party files.

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
