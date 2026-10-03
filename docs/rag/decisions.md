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

## 2026-10-03 — UI approach

**In-Unity UI with camera-attached SpriteRenderers + legacy TextMesh** (subset Fredoka TTF), taps resolved by screen-distance input as in GDD section 7. Rejected: uGUI/TMP (size), HTML overlay (logic split across JS/C#, more to test), IMGUI (hard to polish).

## 2026-10-03 — Unity MCP / CLI

Unity CLI + `com.unity.pipeline` (editor-only) drive the open editor (`unity command ...`) and expose `unity mcp` to Claude Code. The AI Assistant package was not installed: its in-editor MCP server is deprecated in favor of the CLI.

## 2026-10-03 — Architecture

State machine with one handler per state + manual constructor DI from a single composition root. See `architecture.md`.
