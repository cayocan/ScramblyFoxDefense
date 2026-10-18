# Stage 2 plan (1-2 h, due 3 calendar days after the Stage 2 email of 2026-10-18)

Rules: keep Stage 1 untouched (tag `stage1-final`), deliver the revision as tag `stage2`. Do not add new towers, enemies or systems. Tune waves, economy and slots. Keep at least one real placement or upgrade decision.

## Reviewer feedback -> work items
| # | Feedback | Work item | Budget |
|---|---|---|---|
| 1 | Run is about 3 min, too long | Cut to about 75-90 s: 3 waves instead of 5, faster spawns, retuned economy so a real choice remains (which tower first, where to place, when to upgrade) | 20 min |
| 2 | Drag towers anywhere, grabbing-hand tutorial | Drag a ghost tower from the card onto the map; valid/invalid ghost tint; snap off the path; replace the tapping hand with a grabbing hand that drags a ghost into place | 30 min |
| 3 | Exciting start / hook | Opening beat before wave 1: fox intro, "Protect the vault" call to action, first tower already selected | 10 min |
| 4 | Rewarding milestones | Start with the fox tower only. Winning a wave unlocks the dog, then the cat, with a short celebration (card flies in, flash, jingle). No new towers are created, existing ones are gated | 20 min |
| 5 | UI at the top looks better | Redesign the top HUD (coins, wave, fox health, mute) with the Kenney UI Pack panels, readable at 320x568 | 15 min |
| 6 | Sound and music more satisfying | Pick from `docs/stage2-audio-candidates` (CC0, about 150 KB total). Decide: keep Web Audio synth and add decoded OGG one-shots, or improve the synth only | 15 min |
| 7 | Deliver | Production ZIP (index.html at root, < 5,000,000 bytes) via Google Drive "Anyone with the link can view"; change log; repeated checks; known issues; recording of the changed behavior | 10 min |

## Candidate audio (all CC0, Kenney)
- SFX UI: click_003, select_002, confirmation_001 (place), confirmation_003 (unlock), maximize_006 (unlock whoosh), drop_002 (ghost drop), error_004 (invalid spot), pluck_001 (coin)
- SFX game: impactSoft_medium_001 (hit), impactMetal_light_002 (shot), impactBell_heavy_001 (wave clear), impactPlate_light_001 (upgrade)
- Jingles: STEEL00, STEEL05, PIZZI00, PIZZI05 (audition, keep 1-2)
- These were selected by file name and length, not by listening. Audition before using.

## Order of work
1 (length and economy) -> 2 (drag) -> 4 (unlocks) -> 3 (hook) -> 5 (HUD) -> 6 (audio) -> 7 (ZIP, tag).
If time runs out, cut 3 first, then 5.
