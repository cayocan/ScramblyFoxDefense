# Credits

## Assets

| Asset | Author | License | Used for |
|---|---|---|---|
| [Tower Defense Kit](https://kenney.nl/assets/tower-defense-kit) | Kenney | CC0 1.0 | Board tiles, path, tower bases, slot markers, vault |
| [Cube Pets](https://kenney.nl/assets/cube-pets) | Kenney | CC0 1.0 | Fox, dog and cat (defenders); lion, tiger and polar bear (predators) |

The license files ship next to the models in `source/Assets/Art/*/License.txt`. Only the models that are used are included.

## Engine, packages and tools

| Item | License | Notes |
|---|---|---|
| Unity 6000.3.19f1 | Unity terms | Engine, WebGL build |
| Unity glTFast (`com.unity.cloud.gltfast`) | Apache 2.0 | Editor import of `.glb` models; its runtime code is stripped from the build |
| Unity Pipeline package (`com.unity.pipeline`) + Unity CLI | Unity terms | Editor automation only, not part of the build |
| puppeteer-core | Apache 2.0 | Dev-only browser test (`tools/browser-test`), not shipped |

No third-party code is reused in the game itself.

## AI use

Built with Claude Code (Anthropic) as a pair programmer: code, editor automation through the Unity CLI and Pipeline, size investigations, simulations and browser tests. Decisions, corrections and rejected AI outputs are logged in `docs/rag/decisions.md` and summarized in `PROJECT_NOTE.md`.
