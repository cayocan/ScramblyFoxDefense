# Project note

## What I used

- **Unity 6000.3.19f1** (C#, WebGL, IL2CPP), with glTFast in the editor to import the Kenney `.glb` models.
- **Assets:** Kenney Tower Defense Kit and Cube Pets (CC0), recolored to the Scrambly palette; Fredoka font (OFL). UI icons, palette textures, sound effects and the music loop are generated in code for this project (the audio is synthesized in the browser with Web Audio, no audio files).
- **AI:** Claude Code as a pair programmer, driving the Unity editor through the Unity CLI and Pipeline package: code, scene and prefab generation, size investigations, a session simulator for balance, and headless Chrome tests.
- **Tools:** Python (fontTools, Pillow) for the font subset and palette recolor; puppeteer-core for the browser tests.

## What I contributed

- **The concept and the GDD.** I designed a tower defense whose structure is Scrambly's own flow: Discover (pick a game card), Play and progress (three waves, three reward locks), Redeem (coins fly into the vault), then the Explore Scrambly invitation. The fox leads the Cube Pets against purple predators, every balance is labelled demo coins, and the 6-hour plan has decision gates (G0 size test, G1 playable loop) and a cut list.
- **The architecture.** I chose a state machine with one handler per session state and constructor dependency injection from a single composition root, so restart is a clean scene reload with no static state.
- **Review and playtests.** I reviewed every build and screenshot and steered the result: I caught the mirrored corner tiles the AI had declared correct, asked for synthesized audio (effects and music), approved the size cuts and the UI approach, and decided what to keep or cut.

## A decision, correction and rejected output that improved the result

- **Rejected AI output: the egg mascot.** An early AI suggestion used the egg from Scrambly's public site as the mascot. The brief provides a fox, so I rejected it and corrected the art direction before any work started: the fox leads the team and sits on the Reward Vault.
- **Decision: keep Unity only if it fits.** The 5 MB cap made Unity conditional, so the first hour was a size test with a rule (production ZIP up to 3.8 MB, otherwise switch to TypeScript + Three.js). The first build was about 6.3 MB. Measuring showed why: uGUI in Unity 6 pulls in the whole UI Toolkit, TextMesh Pro ships a 1 MB font atlas, the splash logo shipped even with the splash off, the default lighting data carries a 0.5 MB cubemap, and PhysX was force-included. Cutting each one brought the production ZIP to about 3.1 MB with the full game, art and sound.
- **Correction: the path corners.** The board was generated from code and the AI judged its own screenshot as correct; the corner tiles were mirrored. I spotted it and it was fixed by correcting the corner tile's base orientation.

## Time

Feature-by-feature times are in `TIME_LOG.md`; each merge commit also records its duration.
