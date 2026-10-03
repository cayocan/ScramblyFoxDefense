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
