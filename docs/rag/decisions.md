# Decisions log

Feeds PROJECT_NOTE.md (decisions, corrections, rejected AI outputs).

## 2026-10-03 — Phase 0 size test (G0)

Size-test scene per GDD section 8 (camera, kit tower, animated fox on top, sprite, text), built with `Scrambly/Build WebGL` settings: IL2CPP Master + OptimizeSize, managed stripping High, engine code stripping, DiskSizeLTO, Brotli + Decompression Fallback, exceptions off, Minimal template.

| Build | wasm | data | Build folder | ZIP (build + source/) |
|---|---|---|---|---|
| 1: uGUI + TMP | 4,384,391 | 1,755,621 | ~6.3 MB | — |
| 2: no uGUI/TMP, splash logo off | 2,677,169 | 675,819 | 3,533,675 | **3,724,062** |

**G0 decision:** 3.0–3.8 MB band → continue in Unity with cuts (no Plan B).

Findings:
- **uGUI 2.0 (Unity 6) pulls in UnityEngine.UIElementsModule (1.8 MB IL after stripping)** through the EventSystem/UI Toolkit interop; with TMP it adds ~1.7 MB of wasm. Rejected.
- TMP Essentials live in a `Resources` folder, so they ship (1 MB SDF atlas + 350 KB TTF + 350 KB EmojiOne) even if no scene uses them.
- `SplashScreen.show = false` alone still shipped the 2.8 MB (uncompressed) Unity logo; `showUnityLogo = false` is also required.
- Remaining wasm (~2.7 MB) is mostly the native engine; managed code left is ~1.6 MB IL (mscorlib dominates).
- Still in data, to cut next: a 524 KB built-in Cubemap (present even with skybox and reflection off), two different `colormap.png` (175 KB each), glTFast PBR shader (62 KB).
- Cube Pets import (glTFast) as rigid per-part meshes with legacy `Animation` and 8 clips: dance, eat, gesture-negative, gesture-positive, idle, run, static, walk. No skinning, no Mecanim needed.
- Cube Pets are ~2.3 units long vs 1-unit kit tiles: pets scale ~0.4 on towers.

## 2026-10-03 — Size cuts (after G0)

| Step | wasm | data | Build folder | ZIP |
|---|---|---|---|---|
| G0 baseline (build 2) | 2,677,169 | 675,819 | 3,533,675 | 3,724,062 |
| KitLit shader, 256 px colormaps, own LightingData | 2,677,169 | 543,025 | 3,400,881 | 3,603,131 |
| Physics GameObject SDK = None (PhysX stripped) | **1,832,851** | 536,723 | **2,545,557** | **2,754,552** |

- Default `LightingDataAsset` (builtin, fileID 20201) carries a 524 KB reflection cubemap even with skybox off and custom reflection null. Fix: a `LightingSettings` asset with GI off + `Lightmapping.Bake()` gives a 17 KB own LightingData, plus a 4x4 flat custom reflection cubemap.
- `Scrambly/KitLit` (half-Lambert, no reflections/shadows/fog) replaces the glTFast PBR shader (62 KB) on all kit instances (`KitMaterials` editor helper).
- Colormaps: max 256 px, no mipmaps (175 KB → 33 KB each). Pet palette has 1829 colors (faces), so no lossless block downscale.
- **PhysX was force-included** because `com.unity.modules.physics` comes in indirectly (uielements, required by the Pipeline package). Setting Project Settings > Physics > GameObject SDK = None (UI + editor restart) cut 844 KB of wasm. **Rule: game code must not use Physics** (no colliders, raycasts); taps use screen-distance as in GDD section 7.
- uGUI package removed from the project.
- Next size risk: ParticleSystem module native code when particles are first used. Measure then; fallback is pooled quads.

## 2026-10-03 — Balance pass 1 (SessionSimulator)

The board path is 15 units (GDD assumed ~20), so GDD numbers made the game too easy: one Pop Blaster cleared waves 1–2, two cleared everything, sessions ran 34–39 s. Changes in `GameConfig`: enemy health x1.5 (15/9/60), tower range x0.7 (2.3/2.2/3.2), spawn intervals 1.8/1.3/1.1 s.

`Assets/Editor/SessionSimulator.cs` ticks the real services in Play Mode with a 0.02 s step and a scripted player (`SessionSimulator.Run("pop0,up0,pop1")`). Results after the pass (deterministic, repeated):

| Plan | Session | Leaks (total) |
|---|---|---|
| none | 78 s | 27 |
| pop0 | 57 s | 12 (0 / 3 / 9) |
| pop0,pop1 | 54 s | 1 |
| pop0,up0,pop1 | 44 s | 0 |

Matches GDD intent: one tower handles wave 1, wave 3 needs 2+ towers or upgrades, active play lands in 45–60 s. Tap flow (card select/deselect, build, upgrade, shake when broke, cancel on empty) verified with simulated taps at real screen positions.

## 2026-10-03 — Redeem, end card, CTA, restart

- Taps go through a priority chain in `InputRouter` (HUD buttons first, first handler returning true consumes), so Restart never also triggers a board action.
- CTA shows "CTA clicked — demo only" on the panel and logs the same string; no `Application.OpenURL`. Verified with taps at real screen positions: toast shown, log present, scene unchanged.
- Restart reloads the scene, guarded against double taps. 10 consecutive restarts mid-wave gave identical state (1 installer, 191 transforms, same enemy count at the same wave time, 0 errors): nothing accumulates.
- Correction: a C# string got a raw newline from a Python edit; `console_status.compilationFailed` reported false while the editor kept running the old assembly, so a scene was built with the old builder. Now compile success is checked with `recompile_status` plus a type lookup.
- Known: the built-in LegacyRuntime font has no em dash ("Demo only — not real earnings" renders without it). The Fredoka font in the art pass fixes it.

## 2026-10-03 — Art pass and sound

- Font: Fredoka (OFL) instanced at weight 600 and subset to ASCII + em dash with fontTools: 21 KB, and the em dash now renders.
- Palette recolor without new models: `colormap-scrambly.png` remaps the kit colormap by hue (grass -> cream/sand, dirt path -> light orange, tower reds and vault purples -> orange); `colormap-predators.png` turns the pet palette purple (blacks and eye whites kept, polar bear white -> lavender). Both generated with PIL, 19 KB and 5 KB.
- Rounded UI (brief: warm, rounded): 9-sliced sprite for cards, buttons and panel; padlock closed/open, coin, trophy, medal, basket, speaker and hand sprites drawn in code (all under 3 KB each).
- Feedback: white hit flash (KitLit `_Flash`), a short poof on defeat, pets play `gesture-positive` when built or upgraded. No ParticleSystem (native module size).
- Tutorial hand (GDD section 4): card -> slot -> first affordable upgrade, then idle hint after 5 s between waves.
- DOTween offered by the user; not added: the tweens needed are a few lines on scaled time (pause-safe), and it would add code and another time source to pause.
- Sound: Web Audio synth in the page (`window.scramblySfx`, 12 cues), bridged by `Sfx.jslib`; no audio files and no Unity audio module. AudioContext is created on the first gesture, suspended while hidden/landscape, master gain 0 when muted; mute lives in the page so it survives in-game restarts. Browser test: locked -> running after tap -> suspended when hidden -> running -> muted survives restart.
- Correction: a "Build Main Scene" call during a domain reload reported success but saved nothing (scene file timestamp unchanged), which shipped a build with an unassigned field and an abort at startup (exceptions are off). Now the scene file timestamp is checked after building it.
- Correction: with the watchdog keeping Unity in front, Play Mode advances in real time, which skewed SessionSimulator runs; the editor is now paused right after entering Play Mode.
- Size after the pass: ZIP 3,103,873 bytes (build 2,714,300).

## 2026-10-04 — Synth music and editor audio

- User asked for synthesized audio only, with music. Added a soft 4-bar loop (C pentatonic melody over I-vi-IV-V, 100 BPM) to the page synth, scheduled on the AudioContext clock with a lookahead timer, so it freezes in place whenever the context is suspended (hidden, landscape, muted). Browser test: music step advances after the first tap and stays frozen while hidden.
- The user heard nothing because they tested in the editor, where the page synth does not exist. Added `EditorSynth` (C# port of the same cues and loop, `OnAudioFilterRead`), compiled only under `UNITY_EDITOR`; `com.unity.modules.audio` is in the manifest for the editor, and the linker report confirms no Audio module in the WebGL build. The AudioListener is added at runtime in the editor only (a scene listener would pull audio into the build). The two synth tables must be kept in sync.
- The interrupted batchmode build had already deleted `Builds/WebGL`; builds now go through the Pipeline while the editor is open. Stray `Assets/Resources/PerformanceTestRun*` files (performance test framework, generated during builds) are gitignored so they never ship.

## 2026-10-04 — Progression and balance pass 2

User feedback: no logical progression, no balance. Changes (all data in `GameConfig`):

- **Escalating waves with a preview:** each wave has an `intro` shown in the breather before it ("Next: Tigers — fast!", "Next: Polar bears — tough!") and a `clearBonus` (+15, +20). Breather is 4 s so the preview can be read and acted on.
- **Rewards that grow with skill:** a wave with no leaks is a perfect wave (gold lock, "Perfect wave! +15" banner, stronger cue). End card title depends on perfect waves (Perfect / Great / Nice defense) and shows "Perfect waves: n/3".
- **Towers that feel different:** per-tower targeting (Racer Zap aims at the strongest predator), projectile colour/size/speed (orange quick shots, big slow purple orbs with a splash ring, thin fast gold bolts); shots and pets grow with each upgrade.
- **Balance (SessionSimulator, deterministic):** Racer Zap could not one-shot a lion (14 dmg vs 15 hp) and Puzzle Pulse was weak against the spread-out first wave, so Pop Blaster dominated. Racer Zap 16 dmg / 0.7 per s / range 3.6; Puzzle Pulse cost 40, 7 dmg, 1 per s, radius 1.5. Waves 2 and 3 got one more tiger and one more bear to keep the challenge.

| Plan | Session | Leaks |
|---|---|---|
| none | 83 s | 29 |
| Pop Blaster only | 62 s | 15 |
| Puzzle Pulse only | 70 s | 10 |
| Racer Zap only | 58 s | 9 (waves 1–2 perfect) |
| two towers | 54–61 s | 1–4 |
| three towers, or two + upgrade | 48–51 s | 0 |

- Fix: world sprites (poof, splash ring) drew over the end card because sprites sort by order before distance; world effects now use sorting order -10. The wave banner is 26 px (the longest one fits 390 px) and hides when the end card opens.

## 2026-10-03 — UI approach

**In-Unity UI with camera-attached SpriteRenderers + legacy TextMesh** (subset Fredoka TTF), taps resolved by screen-distance input as in GDD section 7. Rejected: uGUI/TMP (size), HTML overlay (logic split across JS/C#, more to test), IMGUI (hard to polish).

## 2026-10-03 — Unity MCP / CLI

Unity CLI + `com.unity.pipeline` (editor-only) drive the open editor (`unity command ...`) and expose `unity mcp` to Claude Code. The AI Assistant package was not installed: its in-editor MCP server is deprecated in favor of the CLI.

## 2026-10-03 — Architecture

State machine with one handler per state + manual constructor DI from a single composition root. See `architecture.md`.
