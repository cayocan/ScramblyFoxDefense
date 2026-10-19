# Time log

Total limit: 6 h. Each feature records start, end and duration here (the same duration goes in its merge commit).

| Feature | Start | End | Duration | Notes |
|---|---|---|---|---|
| Project setup | 2026-10-03 | 2026-10-03 | — | Repo, scripts, Kenney assets imported |
| phase0-build-baseline | 2026-10-03 10:50 | 2026-10-03 11:30 | 0h 40min | Unity CLI/MCP, package trim, size-test builds, G0 = continue in Unity (ZIP 3,724,062 bytes), UI without uGUI/TMP |
| size-cuts | 2026-10-03 11:30 | 2026-10-03 11:54 | 0h 23min | KitLit shader, colormaps 256 px, own LightingData, PhysX off: ZIP 2,754,552 bytes |
| core-greybox | 2026-10-03 11:58 | 2026-10-03 12:21 | 0h 23min | State machine + DI, S-path board, 3 waves, towers/projectiles/coins, simulated full session (with and without towers), ZIP 2,903,870 bytes |
| fix-corner-tiles | 2026-10-03 12:24 | 2026-10-03 12:26 | 0h 01min | Corner path tiles were mirrored (reported by user from screenshot) |
| cards-upgrades-locks | 2026-10-03 12:27 | 2026-10-03 12:44 | 0h 16min | 3 tower cards, tap-to-upgrade with stacked pieces + cost badge, 3 reward locks, camera fit between HUD bands, balance pass 1 (SessionSimulator), ZIP 2,930,986 bytes |
| redeem-endcard-restart | 2026-10-03 12:47 | 2026-10-03 13:11 | 0h 23min | Coins fly to vault, end card (title, collected, rewards, CTA demo-only, Play again, disclaimer), restart x10 verified, ZIP 2,958,015 bytes |
| robustness | 2026-10-03 13:28 | 2026-10-03 13:45 | 0h 17min | Template, PagePause + jslib, rotate prompt, headless Chrome tests (network, touch, pause, layouts), README/CREDITS, ZIP 2,959,429 bytes |
| unity-remote-tools | 2026-10-03 14:05 | 2026-10-03 14:12 | 0h 06min | Win32 tool to read/click Unity dialogs, focus and capture the editor; watchdog auto-answers scene-reload dialogs, keeps PC awake |
| art-pass | 2026-10-03 14:15 | 2026-10-03 14:51 | 0h 36min | Fredoka, palette recolor, rounded UI and icons, hit flash/poof/cheer, tutorial hand, Web Audio sound + mute, ZIP 3,103,873 bytes |
| release-stage1 | 2026-10-03 15:21 | 2026-10-03 15:26 | 0h 05min | PROJECT_NOTE draft, production ZIP verified from a clean folder (full flow + CTA in Chrome), README final numbers, ZIP 3,107,328 bytes |
| synth-music | 2026-10-04 14:32 | 2026-10-04 15:02 | 0h 30min | Synth music loop (Web Audio), editor-only C# synth for Play Mode, PROJECT_NOTE final draft, rebuilt deleted build folder |
| progression | 2026-10-04 15:03 | 2026-10-04 15:21 | 0h 17min | Wave previews + clear bonus, perfect-wave stars and gold locks, tower targeting/shot identity, balance pass 2 (all towers viable, 48-62 s sessions) |
| art-polish | 2026-10-04 15:25 | 2026-10-04 15:38 | 0h 12min | Board decoration (kit trees/rocks/crystals, deco palette), HUD band/shadows/banner pill, build burst, leak shake, ZIP 3,187,058 bytes |
| tutorial-feedback-particles | 2026-10-04 16:13 | 2026-10-04 16:31 | 0h 17min | Tutorial gate (blocks progress and other taps), upgrade-ready ring/arrow/pulse, SparkFx particles on shots and hits, louder audio + limiter, ZIP 3,194,083 bytes |
| fox-health-world | 2026-10-04 16:37 | 2026-10-04 16:55 | 0h 18min | Fox health (coins never lost) + defeat end card, 4 waves, upgrade hint from wave 3, tutorial greys blocked items, per-tower shot SFX, fox on vault fix, forest ring + path entrance, ZIP 3,265,251 bytes |
| final-wave-bloom | 2026-10-04 17:00 | 2026-10-04 17:36 | 0h 35min | 5th wave, harder balance (+25% HP, lower rewards), green grass, ground slab + wider forest (no background visible), light bloom, ZIP 3,289,087 bytes |
| final-hud-enemies | 2026-10-04 17:47 | 2026-10-04 18:04 | 0h 17min | Kenney UI Pack HUD, card borders, enemy colours +25% HP, red enemy health bars, fox repositioned, end tile gap fix |
| release-final | 2026-10-04 18:05 | 2026-10-04 18:05 | 0h 00min | Final sizes, production ZIP, tag stage1-final |
| readme-fix | 2026-10-04 18:11 | 2026-10-04 18:14 | 0h 03min | README rewritten for the final game; balance re-simulated with final config |
| project-note-language | 2026-10-04 18:25 | 2026-10-04 18:26 | 0h 00min | PROJECT_NOTE note on the Portuguese setup commits and merge duration line |
| docs-english | 2026-10-04 19:48 | 2026-10-04 19:51 | 0h 02min | GDD and CLAUDE.md translated to English |
| fix-tutorial-hand | 2026-10-04 19:54 | 2026-10-04 19:57 | 0h 03min | Tutorial hand redrawn as a pointing hand (old sprite read as a middle finger) |
| stage2-short-run | 2026-10-18 18:23 | 2026-10-18 18:57 | 0h 34min | Stage 2: 3 waves (~77 s clean in SessionSimulator, was 105.6 s), 2 s breather, 50 starting coins, second tower or first upgrade before wave 2 ends (not both), 3 locks, upgrade hint from wave 2 |
| stage2-drag-placement | 2026-10-18 19:15 | 2026-10-18 20:03 | 0h 48min | Drag a ghost tower from the cards to any valid point (path, tower, decoration and arena rules in GameConfig, max 3 towers), slots removed, cancel on hide/focus loss/touch cancel, stuck-finger-after-hide fix, drag browser checks at 390x844 and 320x568 (touch + mouse), ZIP 3,337,591 bytes |
| stage2-grab-tutorial | 2026-10-18 20:12 | 2026-10-18 20:19 | 0h 07min | Grabbing-hand tutorial: open/grab sprites drawn by tools/draw-hands.py, demo loop carries a ghost to GameConfig.tutorialSpot, hides on player drag; verified with a real drag at 390x844 and 320x568 |
| stage2-tower-unlocks | 2026-10-18 20:19 | 2026-10-18 20:35 | 0h 16min | Cards 2 and 3 unlock after waves 1 and 2 (silhouette + padlock when locked), breather celebration (banner, slide-in, sparks, screen flash, cue), pulse until used; economy re-simulated unchanged; restart and hidden-tab checks in Chrome |
| stage2-release | 2026-10-18 20:34 | 2026-10-18 20:39 | 0h 5min | Stage 2 docs (README, CREDITS, PROJECT_NOTE Stage 2 section, CHANGELOG_STAGE2.md), ScramblyFoxDefense-stage2.zip 3,370,368 bytes, full run + browser checks from the unpacked ZIP; Tasks 5-7 cut for the two-hour limit |
| stage2-readme-sizes | 2026-10-18 20:40 | 2026-10-18 20:41 | 0h 01min | README size table updated to the Stage 2 build and ZIP |
| stage2-audio-pass | 2026-10-18 20:48 | 2026-10-18 20:57 | 0h 09min | 10 Kenney CC0 one-shots and jingles (94 KB) decoded after the first gesture with synth fallback, music speeds up and layers per wave and ducks under jingles, WaveClear bell; audio, network and layout checks in Chrome; ZIP 3,551,344 bytes |
| stage2-opening-hook | 2026-10-18 20:58 | 2026-10-18 21:07 | 0h 09min | Opening hook in IntroState: "Defend the Vault!", roar shake + growl, pulsing "Tap to start" (tap unlocks audio, 8 s auto-start); tests tap to start |
| stage2-final-sizes | 2026-10-18 21:08 | 2026-10-18 21:08 | 0h 00min | README ZIP size after the opening hook |
