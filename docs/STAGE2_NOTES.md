# Stage 2 notes

## Task 1: shorter run and economy

All numbers come from `SessionSimulator` (Play Mode, fixed 0.02 s step, orders executed as soon as they are affordable). Times start at the first build, so the tutorial taps (a few seconds for a person) come on top.
All values live in `MainSceneBuilder.ApplyShortRunPass`, which runs last on every scene rebuild and is applied to `Assets/Config/GameConfig.asset`.

### Before (Stage 1, 5 waves)

| Wave | Content | Interval | Wave time (clean plan) | Clear bonus |
|---|---|---|---|---|
| 1 | 6 lions | 1.8 s | 15.4 s | 15 |
| 2 | 5 lions + 5 tigers | 1.3 s | 14.1 s | 20 |
| 3 | 6 lions + 4 tigers + 3 bears | 1.1 s | 18.9 s | 25 |
| 4 | 8 lions + 6 tigers + 4 bears | 0.9 s | 18.5 s | 30 |
| 5 | 10 lions + 8 tigers + 6 bears | 0.75 s | 20.0 s | 0 |

Breather 4 s (x4), starting coins 70 (two towers at once), predators 24 / 14 / 94 HP, rewards 4 / 5 / 10.

| Plan | Result |
|---|---|
| `pop0` (one tower) | fox falls in wave 2, 40.8 s |
| `pop0,puz2` | fox falls in wave 3, 65.5 s |
| `pop0,puz2,zap1` | fox falls in wave 5, 115.9 s |
| three towers + all upgrades | perfect, 10/10, **105.6 s** |

### After (Stage 2, 3 waves)

| Wave | Content | Interval | Wave time (clean plan) | Clear bonus |
|---|---|---|---|---|
| 1 | 7 lions | 2.4 s | 17.2 s | 10 |
| 2 | 6 lions + 6 tigers | 1.7 s | 20.9 s | 30 |
| 3 | 12 lions + 10 tigers + 5 bears | 1.1 s | 32.0 s | 0 |

Breather 2 s (x2), redeem 2.6 s, starting coins 50, predators 14 / 10 / 70 HP, rewards 2 / 2 / 6 coins. Fox health rules unchanged (10 HP; lion and tiger 1, bear 3).

| Plan | Result |
|---|---|
| `pop0` (tutorial tower only) | wave 1 perfect, fox falls in wave 3, 66.6 s |
| `pop0,pop1` (second tower, nothing else) | fox falls in wave 3, 69.1 s |
| `pop0,up0` (first upgrade, nothing else) | fox falls in wave 3, 64.8 s |
| `pop0,pop1,up0` | wins, fox 2/10, 78.3 s |
| `pop0,pop1,up0,up1` (tower first) | perfect, fox 10/10, **76.9 s** |
| `pop0,up0,pop1,up1` (upgrade first) | perfect, fox 10/10, **75.9 s** |
| `pop0,puz1,up0,up1` (Puzzle Pulse as second tower) | perfect, fox 10/10, 76.5 s |

A clean run is 76–77 s in the simulator, about 80 s with the tutorial taps. The failure path ("So close!") is reached by a player who stops spending after one choice; a player who keeps spending wins either way.

### The decision, in numbers

- Start: 50 coins. Pop Blaster costs 30, so the tutorial tower leaves 20 and two towers (60) are impossible. Only one tower at the start.
- Wave 1: 7 lions x 2 + clear bonus 10 = +24, so the first breather opens with **44 coins**. That buys a second Pop Blaster (30), a Puzzle Pulse (40) or the Pop Blaster upgrade (40), but not two of them: the cheapest pair is 30 + 40 = 70.
- Wave 2 kills pay at most 12 x 2 = 24, so the wallet can reach **at most 68 before wave 2 ends**, still under 70. Simulator: tower first leaves 14 + 24 = 38 (< 40 for the upgrade); upgrade first leaves 4 + 24 = 28 (< 30 for a tower).
- The wave 2 clear bonus (+30) pays for the other option before wave 3, which needs both: two level-1 towers, or one upgraded tower, lose in wave 3.

### Changes outside the config

- Lock bar: the scene rebuild creates one lock per wave (3).
- Tutorial upgrade hint moved from wave 3 to wave 2 (`TutorialHand.UpgradeHintFromWave = 1`).
- End card and redeem read the wave count from the config; no change needed.
- README text and the browser test wait (one tower now falls at ~67 s, so the wait went from 70 to 80 s).

## Notes for later tasks

- `tools/finish-feature.ps1` writes part of the merge message in Portuguese ("Duracao da feature (inicio ate o fim)"), against the English-only rule. Not blocking; left as is so the Stage 1 history stays consistent.
- The README test table still lists the Stage 1 browser runs (e.g. "1/5 waves"). They will be re-run on the Stage 2 build in the delivery task.

## Task 2: free placement by dragging

- Input: `InputRouter` keeps the tap primitive (HUD buttons, tower upgrade taps) and adds a drag: a tap handler calls `Capture` and the router then reports move, release and cancel for the first pointer only. Canceled on focus loss, touch cancel, page hidden or phone rotated (`Interrupt`, called every paused frame) and when actions are disabled (Redeem, EndCard). Restart reloads the scene, so the ghost goes with it.
- Rules (`PlacementValidator`, plain C#): inside the arena minus `arenaEdgeMargin` (0.35), at least `pathClearance` (0.8) from the path polyline, `towerSpacing` (0.9) from every tower, `decorationSpacing` (0.6) from board trees, rocks and crystals (added beyond the brief: otherwise towers could stand inside a tree), and fewer than `maxTowers` (3). All in `GameConfig`.
- Ghost (`TowerGhost`): one translucent copy per tower type plus a range ring, built once and only shown or hidden. Green or red via `Tint`. It uses a `KitLit` material with alpha blending (blend and ZWrite are now material properties, opaque by default), so no new shader.
- Release: valid builds (existing build feedback); invalid shakes the card with the deny cue; back over the cards or off the canvas is a silent cancel. Coins are only spent by `TowerSystem.TryBuild`.
- Slots are gone: `SlotManager`, `Slot` and `SlotHighlighter` deleted; the builder no longer places markers. The 4 former slot cells stay in `MainSceneBuilder.ReferenceSpots` (decoration keeps clear of them, the tutorial suggests the first, `SessionSimulator` builds on them through the validator). Balance re-simulated: identical to Task 1.
- Ambiguity resolved: "cancel on state change" is applied where actions are disabled (Redeem, EndCard); Wave and Breather changes keep the drag so a player is not interrupted between waves.
- Bug found and fixed while testing: a touch that ended while the page was hidden left the router thinking that finger was still down, so every later touch was ignored as a second pointer. `Interrupt` now forgets the active finger.
- Test notes: headless SwiftShader frames are slow, so the mouse test holds 250 ms after pressing (Unity samples the cursor once per frame); the FULL flow now waits 120 s for the one-tower defeat (~67 s of game time).
- Size: build folder 2,775,382 bytes, ZIP 3,337,591 bytes.

## Task 3: grabbing-hand tutorial

- `tools/draw-hands.py` draws `hand-open.png` and `hand-grab.png` (64 px, palette colours, plain fist with no raised finger), checked at 128 px and 44 px.
- `TutorialHand` loops a 2.8 s demo while no tower stands: open over the Pop Blaster card, grab, carry a ghost along an arc to `GameConfig.tutorialSpot`, release, fade. The ghost turns red while it crosses the path and green at the spot, which shows the rule. The hand hides as soon as the player starts a drag; the player's drag then owns the ghost. The spot is checked against `PlacementValidator` at start (warning if it breaks the rules); none was logged.
- After the first tower the old pointing hand is still used for the upgrade and idle hints.
- Verified in headless Chrome (`node tutorial.js`): 12 frames of the loop at 390x844 (`out/t-hand-sheet.png`), and the tutorial completed with a real touch drag at 390x844 and 320x568 (built at the spot, wallet 50 -> 20).

## Task 4: milestones unlock towers

- `TowerUnlocks` (plain C#): only the Pop Blaster card is open at the start; the breather after wave 1 unlocks Puzzle Pulse, the breather after wave 2 unlocks Racer Zap. Locked cards are a dark silhouette with a padlock and are refused (shake + deny cue) like an unaffordable card. No new towers.
- Celebration inside the breather (scaled time, owned by `BreatherState`, rebuilt with the scene): the clear banner is cut to 0.8 s; then "New friend: <name>!" for 1.2 s, the card slides up with a scale overshoot, a spark burst over it (`SparkFx.Celebrate`, sized in HUD pixels), a warm-white screen flash (`FeedbackFx.ScreenFlash`) and the `Unlock` cue (placeholder; the audio pass was cut, see below). The lock bar still pops on the wave clear. The new card pulses until it is first dragged. Then the next-wave preview for 0.6 s. Breather 2 s -> 2.6 s.
- Economy unchanged (SessionSimulator): one tower falls in wave 3 at 67.8 s; tower first then upgrades: perfect at 78.1 s; upgrade first: 77.1 s; Puzzle Pulse as second tower: 77.7 s. The first breather still opens with 44 coins and the cap before wave 2 ends is still 68 < 70. The simulator builds through `TowerSystem` and does not enforce unlocks; no plan uses a card before its unlock.
- Verified in headless Chrome (`node unlock.js`, 390x844 touch): a locked card is refused at the start; both unlocks fire ("[Unlock] card=1 Puzzle Pulse at wave 2", "card=2 Racer Zap at wave 3") and the new card builds right away; Restart during the celebration returns to the locked state (card 1 refused again); hiding the tab during the celebration freezes it (paused and resumed at the same game time, 22.54 s) and it completes after. The frozen frame `out/u-hidden-mid-celebration.png` shows banner, flash, slide and sparks together.
- Known issue: the padlock sits a little low on the locked card and covers part of the card name (perspective shift from drawing it in front of the 3D face). Readable as locked; not fixed for time.

## Cut for time (two-hour Stage 2 limit)

Task 7 (audio), Task 5 (opening hook) and a trimmed Task 6 (top HUD: coin pill, wave pips, round buttons; no fox health in the HUD) were done afterwards in the time left once WebGL build time (17 min, measured in the Unity log) is not counted.

## Task 7: audio pass

- 10 Kenney CC0 files copied to `Assets/WebGLTemplates/Scrambly/sfx/` (Unity copies them next to `index.html`), 94,470 bytes: Select = ui_click_003, Build = ui_confirmation_001, Upgrade = game_impactPlate_light_001, kill = game_impactSoft_medium_001, new WaveClear = game_impactBell_heavy_001, Unlock = jingles_PIZZI00, Coin = ui_pluck_001, CTA = ui_confirmation_003, Deny = ui_error_004, end-card Fanfare = jingles_STEEL00. Shots, leak and wave start stay synthesized.
- "Audition": no listening was possible in this session. The files were picked by name from the Stage 2 short list and checked for decode success in Chrome; the levels are set per sound in `SAMPLE_GAIN`. A human listen is still due.
- The template fetches and decodes them after the first gesture (same origin, relative URLs); a missing or undecodable file keeps the synth for that id (tested by blocking the OGGs).
- Music: `SetMusicLevel` (jslib + `IAudioService`) is 0 at the intro and the wave number during play: about 6% faster per level, hi-hat ticks from level 1, a melody octave at level 3. The page outlives restarts, so `IntroState` resets it to 0. The music ducks under the two jingles.
- Rules unchanged: locked until the first gesture, mute suspends the context, hidden page or landscape suspends it; samples are buffer sources on the same context, so they stop with it.
- Verified (`node audio.js`, `node test.js`): see the README test table. Size: build folder 2,890,674 bytes.

## Task 5: opening hook

- `IntroState` starts with a hook phase (no new screen or menu): banner "Defend the Vault!", the fox idles on the vault, at 0.7 s a distant roar (new `FeedbackFx.Shake` plus the synth growl, audible only once audio is unlocked), and the phase line pulses "Tap to start". Player actions are disabled during the hook, so the tutorial hand waits.
- A tap anywhere ends it (the same tap unlocks the page audio, and a select click plays); otherwise it ends after `introMaxSeconds` (8 s). The tutorial starts right after. Logged as `[Intro] tutorial starts after N s`.
- Verified: `node tutorial.js` (tap at 390x844 after 1.0 s and at 320x568 after 1.2 s, tutorial then completed by drag), `node audio.js` (locked before the tap, running after; 5 s of frames in `out/h-hook-*.png`, which also showed the 8 s auto-start), `node test.js` (no regressions). The browser tests now tap once to start.

## Task 6: top HUD (trimmed)

- Kept to the cut order's floor plus the cheap part: a purple coin pill (`Coin Pill`, sized by `HudView` to hug the coin icon and balance), a count that rolls up toward the wallet with a pop on gains (spending snaps down), the three padlocks as wave pips (`LockBarView.SetCurrent`: the wave being played pulses orange until its lock opens), and round buttons from the 44 px rounded sprite (Restart 88 x 44 capsule, Mute 44 x 44 circle) in the palette purple.
- Not done: moving the fox health into the HUD with a fox icon (it stays as the bar over the fox).
- Touch targets: 44 reference px; the whole HUD scales by min(width / 390, height / 844), so they are 44 CSS px at 390x844 but about 30 at 320x568. Noted as a known issue.
- Verified (`node hud.js`): screenshots idle and mid-wave at both sizes (`out/hud-*.png`), no overlap; mute, unmute and restart work at both sizes; 0 errors.

## After the report: no tower cap, CC0 music loop (user request)

- `maxTowers` is now 0 = no limit (set in `ApplyShortRunPass`). SessionSimulator: four Pop Blasters now build (`pop3` was refused before) and win; the economy decision is unchanged.
- Music: Kenney's "Music Loops" pack only ships in the paid All-in-1 bundle, so a CC0 loop from another author was used: "Children's March Theme" by CleytonKauffman (OpenGameArt, license checked on the page), 64 s, re-encoded with ffmpeg to mono 32 kHz Vorbis (853 KB -> 345 KB) so the ZIP, which carries it twice (build and source), stays under 5 MB. It loops on the music bus (ducking, mute and hidden-page rules unchanged), and the synth loop is the fallback. Verified with `node audio.js`: music=file after the tap, suspended when muted or hidden, synth when the OGGs are blocked, no errors.

- Later change (user request): the per-wave speed-up of the music file was removed, because playbackRate also raised the pitch. The file now always plays at its recorded speed; only the synth fallback still speeds up per wave.
