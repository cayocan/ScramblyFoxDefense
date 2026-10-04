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
2. Tap a pulsing slot to build. The hand guides the first build (nothing else responds until then); it starts wave 1.
3. Tap a tower to upgrade it (up to level 3). The badge above it shows the cost. Each pet plays differently: Pop Blaster fires fast single shots, Puzzle Pulse hits an area, Racer Zap snipes the strongest predator.
4. Predators that reach the vault hurt the fox (health bar over it); coins you earned are never lost. If the fox falls, the session ends early. Clear a wave with no predator reaching the vault for a gold lock. Each wave pays a bonus, and the breather tells you what comes next. There are 5 waves.
5. After wave 3 the coins fly into the vault. Then tap **Explore Scrambly** or **Play again**. **Restart** (top left) works at any time; the speaker button next to it mutes the sound.

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
| Build folder (`index.html` + `Build/`) | 2,779,524 |
| Production ZIP (build + source + docs) | 3,326,098 |
| Brief limit | 5,000,000 |

Measured with `tools/measure-zip.ps1 -Out Builds\ScramblyFoxDefense.zip` (the docs in the ZIP can shift it by a few hundred bytes). How the size was cut is logged in `docs/rag/decisions.md`: no uGUI/TMP, physics SDK set to None, own lighting data, minimal shader.

## Tests

Environment: Windows 10, Unity 6000.3.19f1, Chrome (headless, SwiftShader) driven by `tools/browser-test` (puppeteer-core), served by `python -m http.server`. Date: 2026-10-03. Run it with `npm install` then `node test.js` in `tools/browser-test` (`TEST_URL=... ` for another server, `FULL=1` for the full session).

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
| Audio | First tap, hide/show, mute, restart | Chrome headless, `scramblySfx.state()` | locked before tap → running → suspended when hidden → running → muted (suspended) → still muted after restart |
| Music | Loop after first tap, frozen while hidden | Chrome headless, `scramblySfx.musicStep()` | Step advances while audible (10 → 14), stays at 14 while the tab is hidden |
| Tutorial | First card, then slot | Chrome headless screenshots | Hand points at the first card, then at the slot nearest the path start |
| Package | Production ZIP unpacked into an empty folder, served with `python -m http.server` | Chrome headless (`TEST_URL`) | All checks above pass from the unpacked ZIP; `index.html` at the root |
| Full flow | Wait 10 s, an ignored tap, the two tutorial taps, then the session with one tower, **Explore Scrambly** and **Play again** | Chrome headless, real touch taps (`FULL=1`) | Tutorial holds the session; the fox falls in wave 2 and the "So close!" end card opens (coins kept, 1/5 waves); "CTA clicked — demo only" on screen and in the console; URL unchanged; Play again back to Discover |
| Tutorial | Wait 20 s, tap Restart, other cards and slots, then card 0 and slot 0 | Editor, simulated taps at real screen positions | Stays on the intro and ignores every non-tutorial tap; card 0 then slot 0 build the tower and wave 1 starts |
| Flow | Balance plans, 5 waves, fox 10 HP | Editor, SessionSimulator | One tower: fox falls in wave 2; two towers: falls in wave 5; three towers: survives (6/10); three towers + upgrades: perfect, ~100 s (details in `docs/rag/decisions.md`) |
| CTA | Tap **Explore Scrambly** | Editor, simulated tap | "CTA clicked — demo only" on screen and in the console; no navigation |
| Restart | 10 restarts in a row mid-wave | Editor | Identical state each time (1 installer, same object count), 0 errors |

### Not tested yet

- A real phone (Android or iPhone) and a second browser (Firefox or Safari).
- Hiding the page with a real tab switch: the test fakes `document.hidden` and fires `visibilitychange`.

## Known limitations

- Sound is synthesized in the browser (Web Audio); the editor plays nothing. It starts after the first tap, has a mute button (top left) and is silent while the page is hidden or the phone is in landscape.
- Reward icons and the tutorial hand are simple shapes drawn in code; the 3D art is the Kenney kits recolored to the palette (autumn trees and purple crystals as board decoration).
