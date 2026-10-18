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
