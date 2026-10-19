# Stage 2 changelog

Revision of the Stage 1 playable (tag `stage1-final`, untouched) driven by reviewer feedback, within a two-hour limit. Tag: `stage2`. Details and numbers: `docs/STAGE2_NOTES.md`.

## Changed

- **Shorter run with a real decision.** 3 waves instead of 5, 2.6 s breathers, retuned predators and rewards. A clean run is about 77–78 s of game time (was 105.6 s), about 80 s with the tutorial. You start with coins for one tower; before wave 2 ends you can afford a second tower or the first upgrade, not both (at most 68 coins against a cheapest pair of 70). One tower alone falls in wave 3.
- **Drag towers anywhere.** Fixed slots are gone. Press a card and drag a ghost tower with its range ring onto the map: green where it can stand, red on the path, next to a tower or a tree, or off the arena. Release on green to build; anywhere else cancels and no coins are spent. Up to 3 towers. On touch the ghost floats above the finger. Tapping a tower still upgrades it.
- **Opening hook.** No new screen: "Defend the Vault!" over the board, a distant roar (camera shake and a growl once audio is on) and a pulsing "Tap to start". The tap also unlocks the audio; the tutorial starts on that tap, or by itself after 8 s.
- **Grabbing-hand tutorial.** A hand opens over the first card, grabs, carries a ghost tower to a suggested spot near the path and lets go, in a loop, until the player drags the first tower. It hides as soon as the player starts dragging.
- **Richer sound.** Ten Kenney CC0 one-shots (UI click, place, upgrade, hit, wave clear bell, coin, CTA, error, an unlock jingle and an end-card jingle; 94 KB) are fetched from the build folder and decoded after the first gesture; any that fail fall back to the Stage 1 synth. The music speeds up and adds layers each wave and ducks under the jingles. Shots stay synthesized (they fire too often for samples).
- **Milestone unlocks.** Only the first tower card is open at the start; the others show a silhouette and a padlock. Clearing wave 1 unlocks Puzzle Pulse, clearing wave 2 unlocks Racer Zap, each with a celebration: "New friend: <name>!" banner, the card slides in with an overshoot, sparks, a screen flash and a cue. The new card pulses until first used.

## Fixed

- A touch that ended while the page was hidden left the input thinking that finger was still down, so every later touch was ignored. Found by the drag browser test.

## Not done (cut for the two-hour limit, in the documented cut order)

- Top HUD redesign (coin pill, wave pips, round buttons).

## Known issues

- "Tap to start" reuses the small phase line under the locks; a bigger call to action would read better.
- The roar growl is silent on the very first visit (audio is locked until the first tap); only the shake shows.
- The samples were chosen by name, length and level, not by ear (no listening was possible in the automated session); give them a listen before relying on the mix.
- The padlock on a locked card sits a little low and covers part of the card name.
- In the tutorial demo the ghost briefly turns red while it crosses the path on its way to the spot (it shows the placement rule, but it is not a deliberate beat).
- Tested in headless Chrome only (emulated touch, faked `document.hidden`), not on a real phone or a second browser.
- Sessions (about 65–80 s) are still longer than the brief's 45–60 s target.
