# Scrambly Fox Defense

A 45–60 second, one-handed tower defense playable for Scrambly (Simula take-home). The fox leads a squad of Cube Pets that guard the Reward Vault from clumsy predators trying to steal demo coins. The session follows Scrambly's own steps: **Discover** (pick a game card) → **Play and progress** (three waves, three reward locks) → **Redeem** (coins fly into the vault) → **Explore Scrambly** (simulated CTA).

All balances are demo coins. Nothing is real money, and the CTA never navigates.

## Run it

The build is a static site. Serve the folder that contains `index.html` with any plain HTTP server. No special headers are needed: the build uses Brotli with Decompression Fallback.

```sh
python -m http.server 8080
# open http://localhost:8080
```

Opening `index.html` straight from disk (`file://`) does not work: browsers block the loader there.

## How to play

1. Tap a card at the bottom (Pop Blaster, Puzzle Pulse, Racer Zap). The free slots pulse.
2. Tap a pulsing slot to build. The first build starts wave 1 (it also starts on its own after 8 s).
3. Tap a tower to upgrade it (up to level 3). The badge above it shows the cost.
4. After wave 3 the coins fly into the vault. Then tap **Explore Scrambly** or **Play again**. **Restart** (top left) works at any time.

## Package contents

| Path | What |
|---|---|
| `index.html`, `Build/` | WebGL build (custom template from `source/Assets/WebGLTemplates/Scrambly`) |
| `source/` | Readable source: `Assets/` (scripts, scenes, prefabs, config, art), `Packages/`, `ProjectSettings/`, `tools/` |
| `README.md`, `CREDITS.md`, `PROJECT_NOTE.md`, `TIME_LOG.md` | Docs |

Open `source/` with **Unity 6000.3.19f1**. `Scrambly > Build Main Scene` rebuilds the scene, prefabs and config from code. `Scrambly > Build WebGL` applies the size settings and builds.

## Code map

The architecture is a state machine with one handler per state plus constructor dependency injection from a single composition root (`docs/rag/architecture.md`).

| Area | Files |
|---|---|
| Composition root, the only `Update` | `Assets/Scripts/Core/GameInstaller.cs` |
| Session flow | `Core/GameStateMachine.cs`, `States/SessionStates.cs` (Intro, Wave, Breather, Redeem, EndCard) |
| Tuning (all numbers) | `Config/GameConfig.cs`, `Assets/Config/GameConfig.asset` |
| Gameplay | `Gameplay/` (Economy, PathRoute, EnemySystem, WaveSpawner, TowerSystem, SlotManager, PlayerActions) |
| Input | `Input/InputRouter.cs`: one tap primitive, first pointer only, cancel on focus loss |
| Page pause | `Core/PagePause.cs`, `Assets/Plugins/WebGL/PageState.jslib`, template `index.html` |
| Restart | `Core/RestartController.cs` (scene reload, guarded) |
| HUD without uGUI | `Presentation/` (HudLayout, cards, locks, badges, end card, redeem sequence) |
| Editor tools | `Assets/Editor/` (MainSceneBuilder, BuildTools, KitMaterials, SessionSimulator) |

## Size

| Item | Bytes |
|---|---|
| Build folder (`index.html` + `Build/`) | 2,662,169 |
| Production ZIP (build + source + docs) | 2,959,429 |
| Brief limit | 5,000,000 |

These are the last measured values (`tools/measure-zip.ps1`). The final ZIP is re-measured before submission. How the size was cut is logged in `docs/rag/decisions.md`: no uGUI/TMP, physics SDK set to None, own lighting data, minimal shader.

## Tests

Environment: Windows 10, Unity 6000.3.19f1, Chrome (headless, SwiftShader) driven by `tools/browser-test` (puppeteer-core), served by `python -m http.server`. Date: 2026-10-03.

| Area | Case | How | Result |
|---|---|---|---|
| Network | Only same-origin requests, no errors | Chrome headless, request log | 5 local files + 2 same-origin `blob:` URLs (decompression); no 4xx, no page errors |
| Layout | 390×844 portrait | Emulated (touch, DPR 2) | Board, HUD and cards visible, nothing cut |
| Layout | 320×568 portrait | Emulated (touch, DPR 2) | Everything visible, cards tappable |
| Layout | Desktop 1280×720 | Emulated (mouse) | Centred portrait column, Deep ink sides, no page scroll |
| Layout | Phone in landscape | Emulated 844×390 touch | "Rotate your phone" overlay, game paused; cleared when rotated back |
| Input | Touch: card → slot → tower | Emulated touch taps | Card selected, tower built (70 → 40), upgraded (40 → 0), wave started |
| Input | Card without coins, tap empty area, re-tap card | Editor, simulated taps at real screen positions | Card shakes and nothing is built; selection cleared |
| Visibility | Hide the tab for 3 s mid-session | Emulated `visibilitychange` | Paused at t=3.76, resumed at t=3.76 (game time does not jump) |
| Flow | No towers at all | Editor, SessionSimulator | Reaches the end card in 78 s, 27 leaks, wallet never below 0 |
| Flow | Balance plans (1 tower, 2 towers, upgrades) | Editor, SessionSimulator | 57 s / 12 leaks; 54 s / 1 leak; 44 s / 0 leaks |
| CTA | Tap **Explore Scrambly** | Editor, simulated tap | "CTA clicked — demo only" on screen and in the console; no navigation |
| Restart | 10 restarts in a row mid-wave | Editor | Identical state each time (1 installer, same object count), 0 errors |

### Not tested yet

- A real phone (Android or iPhone) and a second browser (Firefox or Safari).
- A full session with real input in the browser (input is covered by emulated taps, the full flow by the simulator).
- Hiding the page with a real tab switch: the test fakes `document.hidden` and fires `visibilitychange`.

## Known limitations

- No audio, by design. Because of that there is no mute button.
- The built-in font has no em dash, so "Demo only — not real earnings" renders without it. The Fredoka font in the art pass will fix this.
- Placeholder art: mint ground, flat reward icons. The art pass is next.
