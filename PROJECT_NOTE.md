# Project note

> DRAFT prepared from the decision log (`docs/rag/decisions.md`). Rewrite it in your own words before submitting; the brief asks what *you* contributed. Lines marked [you] need your input.

## What I used

- **Unity 6000.3.19f1** (C#, WebGL, IL2CPP), with glTFast in the editor to import the Kenney `.glb` models.
- **Assets:** Kenney Tower Defense Kit and Cube Pets (CC0), recolored to the Scrambly palette; Fredoka font (OFL). UI icons, palette textures and sound effects were generated in code for this project.
- **AI:** Claude Code as a pair programmer, driving the Unity editor through the Unity CLI + Pipeline package: code, scene and prefab generation, size investigations, a session simulator for balance, and headless Chrome tests.
- **Tools:** Python (fontTools, Pillow) for the font subset and palette recolor; puppeteer-core for browser tests.

## What I contributed

- [you] The concept and GDD: a tower defense whose structure is Scrambly's own flow (Discover → Play and progress → Redeem → Explore Scrambly), the fox leading Cube Pets against purple predators, demo-coin honesty rules, the 6-hour plan and its decision gates.
- [you] Direction and review at every step: scope, architecture (state machine with one handler per state, constructor dependency injection from one composition root), what to cut, and playtesting the builds.
- [you] Process: one branch per feature, merge commits with the feature duration, `TIME_LOG.md`.

## A decision, correction and rejected output that improved the result

- **Decision: keep Unity only if it fits (Phase 0).** The 5 MB cap made Unity conditional, so the first hour was a size test with a rule (≤ 3.8 MB ZIP or switch to TypeScript + Three.js). The first build was ~6.3 MB. Measuring showed why: uGUI in Unity 6 drags the whole UI Toolkit into the build, TextMesh Pro ships a 1 MB font atlas from `Resources`, the splash logo shipped even with the splash off, the default lighting data carries a 0.5 MB cubemap, and PhysX was force-included. Cutting each one (UI with sprites and TextMesh, physics SDK set to None, own lighting data, a minimal shader) brought the production ZIP to about 3.1 MB with the full game, art and sound.
- **Correction: the path corners.** The AI generated the board and, after looking at its own screenshot, declared the corner tiles correct. They were mirrored; I caught it from the screenshot and it was fixed by correcting the corner tile's base orientation.
- **Rejected output:** [you] e.g. an early AI suggestion used the egg from the public site as the mascot; the brief provides the fox, so the art direction was corrected before starting. / DOTween and uGUI were considered and rejected for size and pause-safety.

## Time

Feature-by-feature times are in `TIME_LOG.md` (each merge commit also records its duration).
