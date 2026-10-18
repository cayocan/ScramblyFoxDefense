# Stage 2 prompts for Claude Code (VS Code)

How to use: paste the **Master prompt** once at the start of the session. Then paste **one task prompt at a time**, in order. Do not start the next task before the previous one is merged. Total budget: 2 hours (hard limit). Each task has its own time budget; if a task runs over, ship the smaller version and move on.

---

## Master prompt (paste once)

You are working on Scrambly Fox Defense (Unity 6000.3.19f1, C#, WebGL). Stage 1 is delivered and tagged `stage1-final`; it must stay untouched. We are doing Stage 2, a revision driven by reviewer feedback, with a hard limit of 2 hours.

Read first, in this order, and summarize what you learned in 10 lines before changing anything: `CLAUDE.md`, `README.md`, `docs/rag/architecture.md`, `docs/STAGE2_PLAN.md`, `Assets/Scripts/Config/GameConfig.cs`, `Assets/Scripts/States/SessionStates.cs`, `Assets/Scripts/Core/GameInstaller.cs`.

Reviewer feedback (verbatim intent):
- Make the start more exciting. Give players a hook that makes them want to begin.
- Let players drag towers anywhere on the map instead of picking fixed spots. Replace the tapping-hand tutorial with a grabbing hand that drags a ghost tower into place.
- Make the sound and music more satisfying.
- Make milestones more rewarding: winning each level should feel like an unlock. Getting a new tower should be an exciting moment.
- Keep at least one real placement or upgrade decision. Tune waves, the economy or slots. Do NOT add new towers, enemies or systems.
- Make the UI at the top of the game look better.
- A full run takes about 3 minutes; tighten it while keeping a real decision.
- Deliver a production ZIP (`index.html` at the root, under 5,000,000 bytes).

Non-negotiable rules:
1. Everything in English: code, comments, commit messages, branch names, UI text, docs.
2. Workflow from `CLAUDE.md`: one `feature/<name>` branch per task, started with `.\tools\start-feature.ps1 <name>`, finished with `.\tools\finish-feature.ps1 "summary"`. Never commit to `main` directly. Add the duration to `TIME_LOG.md`.
3. Keep all brief constraints: static hosting, touch and mouse, portrait 320x568 and 390x844 with the rotate prompt in landscape, no page scroll, CTA shows "CTA clicked — demo only" and logs to console without navigating, pause on hidden page with no time jump (scaled time only), restart resets cleanly with no duplicated timers, listeners or effects, demo-balance wording only.
4. Size: the production ZIP must stay under 5,000,000 bytes. Measure with `tools/measure-zip.ps1` after any task that adds assets or code. Stage 1 build folder was 2,779,524 bytes.
5. Tuning goes in `GameConfig` / `GameConfig.asset`. If a value is edited by `MainSceneBuilder`, change it there too so a rebuild does not undo it. Use `SessionSimulator` to check the numbers.
6. Before writing code for a task, state your plan in at most 8 bullets and list the files you will touch. After the work, run the verification listed in the task and report evidence (what you ran, what you saw), not just "done".
7. Do not refactor unrelated code. Do not add libraries. If something in the task is ambiguous, pick the simplest option, say which, and continue.
8. If you find a bug in Stage 1 behavior while working, fix it in the same task only if it blocks the task; otherwise note it in `docs/STAGE2_NOTES.md`.

Acknowledge with the 10-line summary and wait for the first task.

---

## Task 1: shorter run and economy (budget 20 min, branch `stage2-short-run`)

Goal: a full successful run takes about 75-90 seconds, and the player still faces a real decision.

Do:
1. Measure the current run length and economy with `SessionSimulator` (or by reading waves in `GameConfig.asset`): total seconds per wave, coins earned, coins needed. Write the baseline numbers down.
2. Reduce from 5 waves to 3. Keep the locks bar consistent (3 locks, one per wave) and update everything that assumes 5: redeem sequence, end card text, README text, tutorial upgrade hint ("from wave 3" must still make sense, move it to wave 2 if needed).
3. Retune: shorter breather (about 2 s), tighter `spawnInterval`, wave groups so wave 1 is easy, wave 2 needs a second tower, wave 3 needs an upgrade or a good position. Keep the fox health rules.
4. Economy decision: `startingCoins` allows exactly one tower at the start. The second tower plus one upgrade are not both affordable before wave 2 ends. The player must choose between a second tower and upgrading the first. Prove it with numbers in `docs/STAGE2_NOTES.md`.
5. Keep a failure path ("So close!" card) possible but not likely.

Verify: simulator or manual run shows 75-90 s for a clean run; a run that only builds one tower and never upgrades loses or barely survives; a run that makes either reasonable choice wins. Report the before/after table.

---

## Task 2: free placement by dragging (budget 30 min, branch `stage2-drag-placement`)

Goal: the player drags a tower from its card onto any valid point of the map. Fixed slots are gone as the player-facing mechanic.

Do:
1. Read `SlotManager`, `PlayerActions`, `InputRouter`, `CardBarView`, `SlotHighlighter`, `TowerSystem`, `PathRoute`, `TutorialGate`.
2. Extend `InputRouter` with a drag primitive: press on an affordable card starts a drag, the first pointer only, move updates the ghost, release places or cancels. On touch, offset the ghost above the finger so it stays visible. Keep the existing tap primitive for HUD buttons and tower upgrade taps. Cancel any drag on focus loss, page hide, pointer cancel, restart and state change.
3. Add a placement validator (plain C# class, no scene lookups): a point is valid if it is inside the arena bounds, at least a configured distance from the path polyline (`PathRoute`), and at least a configured distance from every existing tower. Distances go in `GameConfig`.
4. Ghost tower: a translucent copy of the tower prefab with a range ring; green tint when valid, red when invalid (reuse `Tint`). Releasing on an invalid point cancels with a short shake and an error cue, and the coins are not spent. Releasing on a valid point builds, spends coins, plays the existing build feedback.
5. Replace `SlotManager` usage by a simple tower registry with a max tower count taken from config (keep it small, it is part of the economy decision). Remove slot highlighting from the gameplay view. Delete dead code only if nothing references it.
6. Keep upgrade by tapping an existing tower.

Verify: in a browser at 390x844 and 320x568 with mouse and with touch emulation, drag a ghost to valid and invalid points, release outside the canvas, change tab mid-drag, press Restart mid-drag. Report that no ghost is left behind and no coins are lost. Run `Scrambly > Build Main Scene` if the scene changed.

---

## Task 3: grabbing-hand tutorial (budget 10 min, branch `stage2-grab-tutorial`)

Goal: replace the tapping hand with a grabbing hand that drags a ghost tower into place.

Do:
1. Read `TutorialHand`, `TutorialFocus`, `TutorialGate`.
2. Redraw the hand as a clear open-hand to closed-grab silhouette (the Stage 1 hand once read as a rude gesture, so check the silhouette at 100% and at small size).
3. Animation loop: hand opens over the Pop Blaster card, closes (grab), moves with a ghost tower along an arc to a recommended spot near the path, opens (release), then fades. Repeat until the first tower is built. If the player starts dragging, hide the hand immediately.
4. The gate stays: until the first tower is built other cards and Restart are greyed out.
5. Recommended spot comes from config, not hard-coded in the view, and must satisfy the Task 2 validator.

Verify: screenshot sequence of the four hand frames; run the tutorial to completion with a real drag at both portrait sizes.

---

## Task 4: milestones unlock towers (budget 20 min, branch `stage2-tower-unlocks`)

Goal: winning each wave feels like a reward by unlocking a tower the player already knows exists. No new towers are created.

Do:
1. Start with only the first tower card available. The other two cards are visible but locked (silhouette plus lock icon).
2. Clearing wave 1 unlocks card 2. Clearing wave 2 unlocks card 3. Wave 3 is the final.
3. Celebration (runs inside the breather, uses scaled time, no new timers that survive restart): the lock bar pops, a banner "New friend: <name>!" for about 1.2 s, the card slides in with a scale overshoot, spark burst (`SparkFx`), screen flash (`FeedbackFx`), a sound cue (placeholder cue now, final sound in Task 7). The new card pulses until first used.
4. Make sure the unlock does not break the economy decision from Task 1: re-run the simulator and update numbers if needed.
5. Restart returns to the locked state with no leftover effects.

Verify: play to each unlock, restart during the celebration, hide the tab during the celebration, confirm no stuck banner or card state.

---

## Task 5: stronger opening hook (budget 10 min, branch `stage2-opening-hook`)

Goal: the first 3 seconds make the player want to start, without adding steps or time.

Do:
1. Read the Intro state in `SessionStates.cs`.
2. Opening beat: fox idle animation, a title "Defend the Vault!", a short telegraph of the threat (a predator silhouette or roar shake in the distance), and a pulsing "Tap to start" that also satisfies the first-gesture requirement for audio.
3. Keep the intro under `introMaxSeconds` and skippable by a tap. The tutorial begins right after.
4. No new screens or menus.

Verify: record a short capture of the first 5 seconds at 390x844; confirm audio can start from that tap.

---

## Task 6: top HUD redesign (budget 15 min, branch `stage2-top-hud`)

Goal: the top of the screen looks polished and is readable at 320x568.

Do:
1. Read `HudLayout`, `HudView`, `HudButton`, `FoxHealthBar`, the Kenney UI Pack panels already imported.
2. New layout: a coin pill with a coin icon and animated count, wave progress as 3 pips with the current wave highlighted, fox health bar integrated with a small fox icon, Restart and Mute as round buttons with a consistent style. Use palette orange `#F58324`, purple `#7845D8`, deep ink `#201338`, warm white `#FFF6E8`.
3. Respect safe areas and small screens: no overlap at 320x568 or 390x844, touch targets at least 44 CSS px, text readable.
4. Keep HUD rendering lightweight and cheap in size.

Verify: screenshots at 320x568 and 390x844 in two states (idle, mid-wave), and a check that Restart and Mute still work.

---

## Task 7: satisfying sound and music (budget 15 min, branch `stage2-audio-pass`)

Goal: the audio feels richer, in budget, and still obeys the audio rules.

Do:
1. Read `Audio/*`, `Assets/Plugins/WebGL/Sfx.jslib` and the template `index.html`. Today audio is a Web Audio synth with no files.
2. Audition the CC0 candidates in `docs/stage2-audio-candidates/`. Choose a small set: UI click, place, invalid, unlock, upgrade, wave clear, hit, shot, coin, plus one or two jingles for unlock and end card.
3. Add a tiny loader in the template that fetches the chosen OGG files from the build folder and decodes them after the first gesture, with the existing synth as fallback if loading fails. No external requests.
4. Music: keep or improve the current synth loop; make it react to the game (slightly faster or fuller each wave, duck under jingles). Do not add a large music file unless the size check allows it.
5. Rules: starts after the first interaction, mute works, everything silent while the page is hidden or in landscape. Update `CREDITS.md` with the Kenney packs (CC0).
6. Size: audio additions under about 200 KB. Re-measure the ZIP.

Verify: play a full run with sound, mute and unmute, hide the tab and return, restart, and report that no sound overlaps or sticks.

---

## Task 8: ship (budget 10 min, branch `stage2-release`)

Do:
1. Run `Scrambly > Build WebGL`. Run `tools/measure-zip.ps1`. The ZIP must have `index.html` at the root and be under 5,000,000 bytes. Name it with `stage2` in the file name.
2. Repeat the Stage 1 checks and record real versus emulated: portrait 320x568 and 390x844, touch and mouse, CTA text and console log, tab hide and resume with no time jump, restart (including mid-drag and mid-celebration), resize, interrupted input.
3. Update `README.md` (3 waves, drag placement, unlocks, run length, new sizes), `CREDITS.md`, `PROJECT_NOTE.md` (Stage 2 section: decisions, one correction, one rejected AI output), `TIME_LOG.md`, and add `CHANGELOG_STAGE2.md` with what changed and known issues.
4. Merge, then tag `stage2` on `main`. Do not move or delete `stage1-final`.
5. Print the exact path of the ZIP and its size. (Uploading to Google Drive with "Anyone with the link can view" is done by the human.)

Verify: unzip to a clean folder, serve it with `python -m http.server`, and play one full run from that copy.

---

## Cut order if time runs out
Task 5 first, then Task 6 (keep only the coin pill and wave pips), then reduce Task 7 to jingles plus a few SFX. Never cut Task 1, 2, 3, 4 or 8.
