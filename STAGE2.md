# Stage 2: revision report

Scrambly Fox Defense, second round, driven by reviewer feedback. Work happened on 2026-10-18 under a two-hour limit for the whole of Stage 2. Stage 1 is preserved unchanged at tag `stage1-final`. The revision is tag `stage2`, delivered as `ScramblyFoxDefense-stage2.zip`.

Related documents: `CHANGELOG_STAGE2.md` (what changed, known issues), `docs/STAGE2_NOTES.md` (numbers and per-task detail), `TIME_LOG.md` (one line per branch), `docs/STAGE2_PROMPTS.md` (the task prompts given to the AI).

## 1. Feedback and what was done

| Reviewer feedback | What was done | Task |
|---|---|---|
| A full run takes about 3 minutes; tighten it and keep a real decision | 3 waves instead of 5, 2.6 s breathers, retuned predators and rewards. A clean run takes about 77–78 s of game time, about 80 s with the tutorial; Stage 1 took 105.6 s. Before wave 2 ends the player can afford a second tower or the first upgrade, never both | 1 |
| Drag towers anywhere instead of fixed spots | Fixed slots removed. A ghost tower with its range ring follows the pointer: green where it can stand, red on the path, next to a tower or a tree, or off the arena. Up to 3 towers | 2 |
| Replace the tapping hand with a grabbing hand that drags a ghost | A hand opens over the first card, grabs it, carries a ghost to a suggested spot and lets go, in a loop. It hides as soon as the player drags | 3 |
| Winning a level should feel like an unlock | The game starts with only the first tower card. Wave 1 unlocks Puzzle Pulse, wave 2 unlocks Racer Zap. Each unlock gets a banner, a card slide-in, sparks, a screen flash and a jingle, and the new card pulses until it is used | 4 |
| A hook that makes players want to begin | "Defend the Vault!", a distant roar (camera shake and a growl) and a pulsing "Tap to start". The tap also turns on the sound | 5 |
| The top UI should look better | Purple coin pill with a rolling count, the padlocks double as wave pips with the current wave pulsing, round Restart and Mute buttons. Trimmed: the fox health stays above the fox | 6 |
| Sound and music should be more satisfying | 10 Kenney CC0 one-shots and jingles (94 KB) with the synth as fallback; the music gets faster and fuller each wave and ducks under jingles | 7 |
| Keep one real decision, no new towers, enemies or systems | Only existing content was used. The decision is proved with numbers in section 4 | 1, 4 |
| Deliver a production ZIP | `ScramblyFoxDefense-stage2.zip`, 3,554,410 bytes, with `index.html` at the root | 8 |

## 2. Timeline (2026-10-18)

Every task ran on its own `feature/<name>` branch. Each branch was started with `tools/start-feature.ps1` and ended with a `--no-ff` merge whose message records the duration. The times below come from those merge commits; build times come from the Unity editor log (`PlayerBuildInfo` step durations).

| Start | Merged | Branch | Task | Duration | WebGL builds (measured) | Without builds |
|---|---|---|---|---|---|---|
| 18:23 | 18:56 | `stage2-short-run` | 1 Shorter run, economy decision | 33 min | none (simulator only) | 33.0 min |
| 19:15 | 20:02 | `stage2-drag-placement` | 2 Drag placement | 47 min | 19:31 236 s, 19:36 182 s, 19:46 175 s (one more failed at once, folder locked) | 37.1 min |
| 20:12 | 20:19 | `stage2-grab-tutorial` | 3 Grabbing-hand tutorial | 6 min | 20:17 140 s | 3.7 min |
| 20:19 | 20:34 | `stage2-tower-unlocks` | 4 Milestone unlocks | 14 min | 20:26 147 s, 20:33 140 s | 9.2 min |
| 20:34 | 20:39 | `stage2-release` | 8 Release docs and ZIP | 5 min | none (packaged the 20:33 build) | 5.0 min |
| 20:39 | 20:39 | `stage2-readme-sizes` | 8 README sizes | 0 min | none | 0.0 min |
| 20:48 | 20:56 | `stage2-audio-pass` | 7 Sound and music | 8 min | 20:54 136 s | 5.7 min |
| 20:58 | 21:06 | `stage2-opening-hook` | 5 Opening hook | 7 min | 21:02 131 s | 4.8 min |
| 21:06 | 21:06 | `stage2-final-sizes` | 8 README sizes | 0 min | none | 0.0 min |
| 21:08 | 21:14 | `stage2-top-hud` | 6 Top HUD (trimmed) | 5 min | 21:12 134 s | 2.8 min |
| 21:16 | – | `stage2-docs` | This report | see `TIME_LOG.md` | none | – |

Before the clock started, the plan and the prompts were prepared (merged 17:36 and 18:18). The repository records that preparation, but it is not counted as Stage 2 work.

## 3. Time spent

| Measure | Time | How it was measured |
|---|---|---|
| Branch time, Tasks 1–8 | **2h05** | Sum of the durations in the merge commits (start of branch to merge) |
| WebGL build time inside those branches | **23.7 min** | 9 builds, 1,422 s, from the Unity editor log |
| Branch time without builds | **~1h41** | 2h05 minus 23.7 min |
| Wall clock, first branch to last merge | **2h51** (18:23 → 21:14) | Includes the gaps below |

Gaps between branches, which no branch covers:
- 18:56 → 19:15, 19 min: reading and planning Task 2 before its branch was started. From Task 3 on, branches were started before reading.
- 20:02 → 20:12, 10 min: reporting Task 2 and replanning the remaining time (the two hours were confirmed to cover all of Stage 2).
- 20:39 → 20:48, 9 min: deciding with the user whether to split the delivery and how to count time.
- 20:56 → 20:58 and 21:06 → 21:08: reporting between tasks.

**Order of work and the two-hour limit.** Tasks 1–4 and 8 (the ones the prompts mark as never to cut) were finished first, in 1h45 of branch time. At that point Tasks 5–7 were set aside following the documented cut order. After measuring build time from the editor log, the user decided to count time without WebGL builds, which left about 30 minutes. Task 7, then Task 5, then a trimmed Task 6 were added in that order. Counted with builds, Stage 2 took 2h05 and runs 5 min over the limit. Counted without builds, it took about 1h41. By the wall clock it took 2h51, including the gaps above.

## 4. Key decisions, with evidence

- **The decision (Task 1).** The run starts with 50 coins; the tutorial Pop Blaster costs 30. The first breather opens with 44 coins. Wave 2 kills pay at most 24, so the wallet tops out at 68 before wave 2 ends, and the cheapest "second tower + first upgrade" pair costs 70. The `SessionSimulator` runs (Play Mode, real services, fixed step) show the result:
  - One tower only: falls in wave 3, 67.8 s.
  - A second tower or an upgrade, and nothing after: falls in wave 3.
  - Second tower first, then upgrades: perfect, 78.1 s.
  - Upgrade first, then the second tower: perfect, 77.1 s.
  - Puzzle Pulse as the second tower: perfect, 77.7 s.

  Stage 1, for comparison: a clean run took 105.6 s, and the player could build two towers at the start.
- **Placement rules (Task 2).** A plain C# validator with no scene lookups checks that a point is inside the arena, 0.8 from the path, 0.9 from other towers and 0.6 from board decoration. The decoration rule was added beyond the brief, so a tower can never stand inside a tree. All distances are in `GameConfig`.
- **Rebuild safety.** All Stage 2 numbers live in `MainSceneBuilder.ApplyShortRunPass`, which runs last on every scene rebuild. The older Stage 1 passes would otherwise grow the run back to 5 waves.
- **Audio rules (Task 7).** The samples load only after the first gesture and play through the same Web Audio context, so mute, a hidden page and landscape still silence everything. If a file can't be decoded, the synth plays instead; this was tested by blocking the files.

## 5. AI use

Claude Code was the pair programmer. It drove the open Unity editor through the Unity CLI and the Pipeline package: compiles, scene rebuilds, simulator runs and WebGL builds. It also wrote headless Chrome checks that read one console line per game outcome: `[Drag]` for drag results, `[Unlock]` for unlocks and `[Intro]` for the start tap.

Corrections found by the checks and fixed:
- **Stuck finger.** A touch that ended while the page was hidden left the input thinking that finger was still down, so every later touch was ignored. Found by `drag.js`; the router now forgets the active finger whenever the page pauses.
- **Hidden padlocks.** The locked-card padlocks were drawn behind the 3D pet faces and didn't show. Found in a screenshot; the padlock was moved in front of the face.
- **Build folder locked.** A build failed because the local test server was running from inside the build folder, which Windows then locked. The server is now started with `--directory` from outside it.

Rejected AI output:
- **First Stage 2 balance.** Lions at 18 HP spawning every 1.6 s made every plan lose, including the intended winning ones. It was rejected and retuned (14 HP, a slower first wave) until one tower alone loses and both choices win.
- **Mouse test failures.** These looked like a game bug at first. They were a test artifact: headless frames are slow, so the cursor had already left the card when Unity sampled the button. The test now holds still briefly after pressing.

## 6. Verification

All checks ran in headless Chrome (SwiftShader) against a static `python -m http.server`, with touch emulated by CDP and the page-hidden state faked. Scripts are in `tools/browser-test`.

| Script | What it covers | Last result |
|---|---|---|
| `test.js` (`FULL=1`) | Network, audio rules, landscape prompt, layouts, full session, CTA | Only same-origin requests, no errors. Rotate prompt and pause work. A full run ends on "CTA clicked — demo only" with the URL unchanged; the full run was played from the final ZIP unpacked into an empty folder (21:24), with `audio.js` as well |
| `drag.js` | Drag at 390×844 and 320×568, touch and mouse: path drop, valid drop, hidden tab, release off the canvas, Restart mid-drag | Coins are spent only on a build; the ghost is hidden after every outcome; Restart resets to 50 coins and 0 towers |
| `tutorial.js` | Hand loop frames, start tap, first tower by drag at both sizes | The tap starts the tutorial (1.0 s and 1.2 s); the tower is built at the suggested spot |
| `unlock.js` | Locked card, both unlocks, Restart and hidden tab during the celebration | A locked card is refused; both unlocks happen; Restart relocks the cards; a hidden tab freezes the celebration at the same game time |
| `audio.js` | First gesture, samples, music level, mute, hidden tab, restart, files blocked | Locked until the tap; 10 of 10 samples decode; the music level follows the wave and resets on restart; mute and a hidden tab suspend audio; the synth plays when files are blocked |
| `hud.js` | Top HUD idle and mid-wave at both sizes, Mute and Restart | No overlap; mute, unmute and restart work |

**Not tested:**
- A real phone, or a browser other than Chrome.
- Hiding the page with a real tab switch (the tests fake it).
- The sound by ear: the samples were picked by name, length and decode success.

## 7. Size

| Item | Stage 1 | Stage 2 |
|---|---|---|
| Build folder | 2,779,524 bytes | 2,886,406 bytes |
| Production ZIP (build, source, docs) | 3,326,131 bytes | 3,554,410 bytes (71% of 5,000,000) |

Measured with `tools/measure-zip.ps1 -Out Builds/ScramblyFoxDefense-stage2.zip`. Editing the docs shifts the ZIP by a few hundred bytes.

## 8. Known issues and limits

- At 320×568 the HUD scales down, so the 44 px buttons render about 30 CSS px tall.
- The fox health was not moved into the top HUD.
- The padlock on a locked card covers part of the card name.
- In the tutorial demo, the ghost turns red while it crosses the path.
- "Tap to start" uses the small phase line.
- The roar growl is silent on the first visit, because audio unlocks with the tap.
- Sessions (about 65–80 s) are still longer than the brief's 45–60 s.
