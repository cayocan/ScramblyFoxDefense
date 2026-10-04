# Scrambly Fox Defense

A one-handed tower defense playable for Scrambly (Simula take-home). The fox guards the Reward Vault while her squad of Cube Pets stops clumsy predators (lions, tigers, polar bears). The session follows Scrambly's own steps: **Discover** (pick a game card) → **Play and progress** (five waves, five reward locks) → **Redeem** (coins fly into the vault) → **Explore Scrambly** (simulated CTA).

All balances are demo coins. Nothing is real money, coins you earn are never taken away, and the CTA never navigates.

## Run it

The build is a static site. Serve the folder that contains `index.html` with any plain HTTP server. No special headers are needed: the build uses Brotli with Decompression Fallback.

```sh
python -m http.server 8080
# open http://localhost:8080
```

Opening `index.html` straight from disk (`file://`) does not work: browsers block the loader there.

## How to play

1. **Tutorial (required):** the hand points at the Pop Blaster card, then at the first slot. Until that first tower is built nothing else responds (other cards, slots and Restart are greyed out) and the game waits. The first build starts wave 1.
2. Tap a card at the bottom (Pop Blaster 30, Puzzle Pulse 40, Racer Zap 55), then a pulsing slot to build. Each pet plays differently: Pop Blaster fires fast single shots, Puzzle Pulse hits an area, Racer Zap snipes the strongest predator.
3. Tap a tower to upgrade it (up to level 3). When you can afford it, the tower shows a pulsing ring, an arrow and an orange "UP" cost badge. The upgrade hint appears from wave 3; the first waves are free play.
4. Predators that reach the vault hurt the fox (10 HP: lion and tiger 1, polar bear 3; health bar over the fox). Every predator has a red health bar. If the fox falls, the session ends early on a "So close!" card. Clear a wave with no predator reaching the vault for a gold lock. Each wave pays a bonus, and the breather tells you what comes next.
5. After wave 5 the coins fly into the vault. Then tap **Explore Scrambly** or **Play again**. **Restart** (top left) works at any time after the tutorial; the speaker button next to it mutes sound and music.

A session lasts about 40 s (fox falls early) to about 110 s (all five waves).

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
| Gameplay | `Gameplay/` (Economy, FoxHealth, PathRoute, EnemySystem, WaveSpawner, TowerSystem, SlotManager, PlayerActions, TutorialGate) |
| Input | `Input/InputRouter.cs`: one tap primitive, first pointer only, cancel on focus loss, priority chain (HUD buttons first) |
| Page pause | `Core/PagePause.cs`, `Assets/Plugins/WebGL/PageState.jslib`, template `index.html` |
| Restart | `Core/RestartController.cs` (scene reload, guarded) |
| Audio | `Audio/` (sound ids, cues, mute) → `Assets/Plugins/WebGL/Sfx.jslib` → Web Audio synth in the template; `Audio/EditorSynth.cs` is the editor-only port |
| HUD without uGUI | `Presentation/` (HudLayout, HudView, cards, locks, badges, tutorial hand and focus, fox and enemy health bars, end card, redeem sequence) |
| Effects | `Presentation/` (FeedbackFx, SparkFx particles, CoinPopFx), `BloomEffect.cs` + `Assets/Art/Shaders/Bloom.shader` |
| Editor tools | `Assets/Editor/` (MainSceneBuilder, BuildTools, KitMaterials, ArtImports, SessionSimulator) |

## Size

| Item | Bytes |
|---|---|
| Build folder (`index.html` + `Build/`) | 2,779,524 |
| Production ZIP (build + source + docs) | 3,326,131 |
| Brief limit | 5,000,000 |

Measured with `tools/measure-zip.ps1 -Out Builds\ScramblyFoxDefense.zip` (editing the docs shifts the ZIP by a few hundred bytes). How the size was cut is logged in `docs/rag/decisions.md`: no uGUI/TMP, physics SDK set to None, own lighting data, minimal shaders, own particles and bloom instead of the ParticleSystem and Post Processing packages, synthesized audio instead of audio files.

## Tests

Environment: Windows 10, Unity 6000.3.19f1, Chrome (headless, SwiftShader) driven by `tools/browser-test` (puppeteer-core), served by `python -m http.server`. Dates: 2026-10-03 and 2026-10-04 (last run on the final build). Run it with `npm install` then `node test.js` in `tools/browser-test` (`TEST_URL=...` for another server, `FULL=1` for the full session).

| Area | Case | How | Result |
|---|---|---|---|
| Network | Only same-origin requests, no errors | Chrome headless, request log | 5 local files + 2 same-origin `blob:` URLs (decompression); no 4xx, no page errors |
| Layout | 390×844 portrait | Emulated (touch, DPR 2) | Board, HUD and cards visible, nothing cut, no camera background showing |
| Layout | 320×568 portrait | Emulated (touch, DPR 2) | Everything visible, cards tappable |
| Layout | Desktop 1280×720 | Emulated (mouse) | Centred portrait column, Deep ink sides, no page scroll |
| Layout | Phone in landscape | Emulated 844×390 touch | "Rotate your phone" overlay, game paused; cleared when rotated back |
| Input | Touch: tutorial card → slot → tower | Emulated touch taps | Tower built, then upgraded; wave 1 started |
| Input | Card without coins, tap empty area, re-tap card | Editor, simulated taps at real screen positions | Card shakes and nothing is built; selection cleared |
| Tutorial | Wait 20 s, tap Restart, other cards and slots, then card 0 and slot 0 | Editor, simulated taps at real screen positions | Stays on the intro and ignores every non-tutorial tap; card 0 then slot 0 build the tower and wave 1 starts |
| Visibility | Hide the tab mid-session, then show it | Emulated `visibilitychange` | Paused and resumed at the same game time (no time jump) |
| Audio | First tap, hide/show, mute, restart | Chrome headless, `scramblySfx.state()` | locked before tap → running → suspended when hidden → running → muted (suspended) → still muted after restart |
| Music | Loop after first tap, frozen while hidden | Chrome headless, `scramblySfx.musicStep()` | Step advances while audible, stays frozen while the tab is hidden |
| Package | Production ZIP unpacked into an empty folder, served with `python -m http.server` | Chrome headless (`TEST_URL`) | All browser checks pass from the unpacked ZIP; `index.html` at the root |
| Full flow | Wait 10 s, an ignored tap, the two tutorial taps, then the session with one tower, **Explore Scrambly** and **Play again** | Chrome headless, real touch taps (`FULL=1`) | Tutorial holds the session; the fox falls in wave 2 and "So close!" opens (coins kept, 1/5 waves); "CTA clicked — demo only" on screen and in the console; URL unchanged; Play again back to Discover |
| Balance | Scripted plans, 5 waves, fox 10 HP, final config | Editor, SessionSimulator | One tower: fox falls in wave 2 (~41 s); two towers: wave 3 (~61 s); three towers without upgrades: wave 5 (~116 s); three towers + upgrades: perfect, 10/10 (~111 s) |
| CTA | Tap **Explore Scrambly** | Editor, simulated tap | "CTA clicked — demo only" on screen and in the console; no navigation |
| Restart | 10 restarts in a row mid-wave | Editor (tested 2026-10-03, before the later features) | Identical state each time (1 installer, same object count), 0 errors |

### Not tested yet

- A real phone (Android or iPhone) and a second browser (Firefox or Safari).
- Hiding the page with a real tab switch: the test fakes `document.hidden` and fires `visibilitychange`.
- The 10-restart test was not repeated after the fox health, tutorial and effects features (restart itself is exercised by the browser test).
- How the sound actually sounds (volume, timbre): checked by state only, not by ear in the automated tests.

## Known limitations

- Sessions run about 40–110 s, longer than the brief's 45–60 s target: five waves and a fox health bar were chosen for depth during playtesting.
- Sound and music are synthesized (Web Audio in the browser, a C# port in the editor); there are no recorded audio files. They start after the first tap, have a mute button and are silent while the page is hidden or the phone is in landscape.
- Reward icons, padlocks and the tutorial hand are simple shapes drawn in code; HUD buttons and panels use the Kenney UI Pack; the 3D art is the Kenney kits recolored to the palette.
