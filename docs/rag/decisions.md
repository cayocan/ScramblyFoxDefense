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

## 2026-10-03 — UI approach

**In-Unity UI with camera-attached SpriteRenderers + legacy TextMesh** (subset Fredoka TTF), taps resolved by screen-distance input as in GDD section 7. Rejected: uGUI/TMP (size), HTML overlay (logic split across JS/C#, more to test), IMGUI (hard to polish).

## 2026-10-03 — Unity MCP / CLI

Unity CLI + `com.unity.pipeline` (editor-only) drive the open editor (`unity command ...`) and expose `unity mcp` to Claude Code. The AI Assistant package was not installed: its in-editor MCP server is deprecated in favor of the CLI.

## 2026-10-03 — Architecture

State machine with one handler per state + manual constructor DI from a single composition root. See `architecture.md`.
