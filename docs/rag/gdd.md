# Scrambly Fox Defense — GDD

Oct 1, 2026 · @Cayo

> English translation of the original design document (written in Portuguese before development). This is the plan as designed; the shipped game diverges where playtesting changed it (5 waves, fox health instead of coin theft, mandatory tutorial). Those changes are logged in `decisions.md`.

A lean tower defense in Unity/C#, 45 to 60 seconds long and played with one hand: the fox leads Cube Pets against wild predators (lion, tiger and polar bear) that steal coins, following Scrambly's Discover → Play → Redeem flow, with Kenney assets recolored to the orange–purple palette.

## 1. Overview

**Pitch:** Scrambly's fox leads a squad of Cube Pets that protects the **Reward Vault** from wild predators who want to steal the coins. The player picks "game cards" (each one with a pet, which becomes the tower), places and upgrades them over 3 short waves, collects demonstration coins, unlocks 3 rewards and ends with a clear invitation: **Explore Scrambly**.

**Design pillars**

1. **Understandable in 3 seconds.** A single action (tap), one hand, no long text. A guide hand teaches the first two taps.
2. **Visible progress.** The tower grows piece by piece, the demo balance goes up and 3 reward locks open, one per wave.
3. **The product is the structure.** Each phase of the session matches a step of Scrambly (Discover, Play, Redeem); it is not decoration on top of a generic TD.
4. **Honest promise.** Every balance is "demo". No money amounts, no real brands.

**Audience and goal (from the brief):** adults who enjoy casual mobile games and are curious about rewards for discovering and playing. The goal is to make the link between playing, progressing and Scrambly easy to understand.

**Scope in one line:** 1 map, 1 path, 4 slots, 3 towers (Cube Pets), 3 predators (lion, tiger, polar bear), 3 waves, 3 rewards to unlock, 1 ending, 1 CTA, 1 restart. No audio, no defeat screen, no menus. **Assets:** only the 4 chosen Kenney packs (Tower Defense Kit, Cube Pets, UI Pack, Game Icons, all CC0); details in section 6.

**Language:** this GDD was written in Portuguese; all in-game text is in English (Scrambly operates in the US, UK and Canada).

## 2. Connection to Scrambly

The brief defines the product in three steps. The whole session is organized around those same three steps, and each has its own mechanic and a screen the player recognizes.

| Scrambly step | Session phase | What the player does | What they see |
| --- | --- | --- | --- |
| **Discover** | Card choice (0–10 s) | Picks among 3 "game cards" (each with a pet and a genre icon) and places the first one | Cards with pet faces; "Discover" label on the phase strip |
| **Play and progress** | Waves 1 to 3 (10–45 s) | Builds and upgrades towers, defeats the predators, collects demo coins | 3 locks at the top; the tower gains pieces and the pet celebrates |
| **Redeem** | Reward Vault (45–55 s) | Watches and taps to speed up | Coins fly to the Vault, the locks open and 3 generic reward cards appear (trophy, medal, basket) |
| **Invitation** | End screen | Taps **Explore Scrambly** | Big button; on click, local message "CTA clicked — demo only" |

**Why a tower defense fits.** In a TD, "playing well" already means growing (more towers, better towers), so progress is the game itself. The unlocking locks are the tangible reward and the fox gives the brand a face. Predators trying to steal coins give a reason to protect the demo balance.

**Honest-promise rules (from the brief)**

- Every balance appears as **Demo coins** and the end screen carries the line "Demo only — not real earnings".
- No money amounts, no guaranteed-earnings claims, no payout deadlines.
- Reward cards are generic icons (trophy, medal, basket), with no retailer or payment-method logos.
- The "games" on the cards are fictional. No real game is named.
- Do not use marketing numbers from the website (bonus, daily average, number of users), because the brief does not provide them and they could read as a promise.

## 3. Loop and session flow

The session lasts about 55 seconds and always reaches the end, even if the player builds nothing. This avoids a stuck game during review.

| Approx. time | State | What happens | Player input |
| --- | --- | --- | --- |
| 0–8 s | **Intro / Discover** | The fox enters the Reward Vault, 70 demo coins, 3 game cards (each with a pet) at the bottom, the guide hand points at the first card | Tap a card, tap a glowing slot |
| ~8–20 s | **Wave 1** | 6 Snatchers (lions) follow the path; the pets shoot; coins pop out of the predators | Build more towers |
| ~20–23 s | Breather | Lock 1 opens, "Wave 2" banner; the guide hand points at the first upgrade if there are coins | Upgrade a tower (tap it) |
| ~23–37 s | **Wave 2** | 5 Snatchers + 4 Darts (tigers) | Build/upgrade |
| ~37–40 s | Breather | Lock 2 opens, "Final wave" banner | Upgrade |
| ~40–54 s | **Wave 3** | 6 Snatchers + 4 Darts + 2 Haulers (polar bears) | Build/upgrade |
| ~54–60 s | **Redeem** | Lock 3 opens, the coins fly to the Vault, which glows, and 3 generic reward cards appear | Optional: tap to speed up |
| end | **End card** | "Discover games. Play and progress. Redeem rewards." + **Explore Scrambly** button + **Play again** | Tap the CTA or restart |

**Pacing rules**

- **Wave 1 starts** when the first slot is filled or after 8 s of game time in the intro, whichever comes first. This way the session never waits for the player indefinitely.
- **Each wave ends** when every spawned enemy has died or reached the Vault. There is no countdown, so there is no "time's up" case.
- **Inactivity:** if the player goes 5 s without tapping during a breather, the guide hand reappears pointing at the best action.
- **Game time = scaled time** (`Time.deltaTime` with `timeScale`). This allows pausing everything at once when the page is hidden.

## 4. Mechanics and controls

A single input primitive: **tap (or click) on a point**. No dragging, holding, hover or gestures, so it works the same on touch and mouse.

**Build (Discover)**

1. Tapping a game card at the bottom selects the tower. The free slots pulse in purple.
2. Tapping a pulsing slot builds the tower ("pop" animation) and deducts the coins.
3. Tapping the card again, an empty area, or losing the pointer cancels the selection.
4. Not enough coins: the card shakes and the price flashes. Nothing is built.

**Upgrade (Play and progress)**

- Tapping a tower raises it one level (levels 1 to 3), if there are coins. The cost appears on a badge with a wrench icon above the tower; without enough balance, the badge turns grey.
- The tower gains a new (taller) modular piece and a ring, and the pet celebrates with a short confetti burst. This is the main progress feedback.
- Selling and moving towers are **out of scope**.

**Guide hand (text-free tutorial)**

1. Step 1: points at the first card.
2. Step 2: points at the slot closest to the start of the path.
3. Step 3: the first time the balance covers an upgrade, points at the tower. After that it never appears again, except for the inactivity hint.

**No defeat screen.** A predator that reaches the Vault takes 5 demo coins (never below zero) and runs off the map. The ending is always the same path to Redeem; only the opening line of the end screen changes ("Perfect defense!" with no leaks, "Nice defense!" with leaks). This simplifies states, reduces edge cases and keeps the warm tone of the brief.

**Response feedback (clarity and feel)**

- Accepted tap: card and slot react within 1 frame (scale + glow).
- Hit: short white flash on the predator. Death: particle burst and a coin that jumps to the counter.
- Wave: animated banner at the top. Milestone: the lock opens with a pulse and a glow.
- Touch targets of at least 48×48 CSS px; slot selection by a 44 px radius around the slot.

**Interrupted input:** only the first pointer counts. Pointer cancel, focus loss or leaving the window discard the current selection without building anything.

## 5. Content and initial balance

Starting values, all in a `GameConfig` (ScriptableObject) so they can be tuned without touching code. Reserve about 1 h of playtesting to tune.

**Towers ("Pet Posts": each one is a fictional game card, with a Cube Pet as the gunner)**

| Tower / card | Pet | Role | Cost | Damage | Fire rate (shots/s) | Range (u) | Level 2 / Level 3 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| **Pop Blaster** | Dog (named in the pack tags) | Fast single shot | 30 | 4 | 2.0 | 3.2 | cost 40: damage 6, 2.4/s · cost 60: damage 9, 2.8/s |
| **Puzzle Pulse** | Cat (named in the pack tags) | Area damage | 45 | 5 (radius 1.4) | 0.9 | 3.0 | cost 50: damage 8 · cost 70: damage 12, radius 1.8 |
| **Racer Zap** | Any other pet from the pack | Long, strong shot | 55 | 14 | 0.6 | 4.5 | cost 60: damage 20 · cost 80: damage 28, 0.7/s |

**Enemies (Cube Pets predators: lion, tiger and polar bear, recolored purple)**

| Enemy | Health | Speed (u/s) | Coins on death | Role | Visual (kit variant to define when opening the pack) |
| --- | --- | --- | --- | --- | --- |
| Snatcher | 10 | 1.6 | 6 | Basic | Lion, animal-lion (default) |
| Dart | 6 | 2.8 | 7 | Fast, tests range and fire rate | Tiger, animal-tiger (fast and fragile) |
| Hauler | 40 | 1.0 | 15 | Heavy, rewards upgrades | Polar bear, animal-polar (slow tank, larger scale) |

**Waves**

| Wave | Composition | Interval between enemies | Total health |
| --- | --- | --- | --- |
| 1 | 6 Snatchers | 1.2 s | 60 |
| 2 | 5 Snatchers + 4 Darts | 0.9 s | 74 |
| 3 | 6 Snatchers + 4 Darts + 2 Haulers | 0.8 s | 164 |

**Economy**

- Starting balance: **70 demo coins** (covers one tower with almost enough left for a second).
- Four slots on the map. Building and upgrading everything does not fit the budget, so there are real choices.
- Two counters: **Wallet** (spendable, in the HUD) and **Collected** (sum of everything that dropped, used for the coins flying to the Vault and on the end screen).
- Leak: −5 from the Wallet per predator that reaches the Vault, minimum 0.
- Intended curve: one level-1 tower almost clears wave 1; wave 3 needs two or three towers, or upgrades.
- Path of about 20 u, so a Snatcher takes about 12 s to cross.

## 6. Theme, art direction and UI

The brief asks for a **warm, expressive and welcoming** treatment, with rounded shapes and orange–purple contrast. The provided fox and colors are a starting point, not a mandatory layout. I have not seen the reference kit yet (`Scrambly-Assessment-Reference-Kit-v3.zip`, 20.1 KiB), so the fox details depend on it.

**Theme chosen to match the 4 packs.** Scrambly's fox leads a squad of **Cube Pets** that protects the **Reward Vault** from **wild predators**, who invade the Pet Corner to steal the coins and run off with them. Nothing aggressive: **clumsy, cute predators (lion, tiger and polar bear from Cube Pets), no blood or biting. Orange = your side (fox, pets, Vault), purple = the predators**. It is the contrast the brief suggests, readable without text.

**Asset map (the 4 chosen packs, all CC0)**

| Pack | Use in the game | What the pages confirm | What to check when opening the ZIP |
| --- | --- | --- | --- |
| [Tower Defense Kit](https://kenney.nl/assets/tower-defense-kit) | Board and path (tiles), tower bases (modular pieces), weapon as the shot origin, **Cube Pets predators as enemies (the kit has no animal enemies)**, castle pieces for the Vault | 160 objects; tiles, modular tower pieces and weapons (the kit's UFOs are not used); FBX, OBJ and glTF | Verified: the kit has 4 UFOs (not used), a beam, selection markers and crystals; all models use a shared palette texture (colormap.png). There is no gate or chest piece |
| [Cube Pets](https://kenney.nl/assets/cube-pets) | 3 **gunners** (one per tower) and the face of each game card | Version 2.0, 24 files, **animated** 3D models; the tags name dog and cat | Full pet list (is there a fox? if so, it can be the game's 3D fox), clip names (idle, attack or dance), format, weight |
| [UI Pack](https://kenney.nl/assets/ui-pack) | Cards, panels, progress bar, **Explore Scrambly** and **Play again** buttons | 430 assets: buttons, panels, sliders | Whether the style is rounded and tintable to the palette, whether there are stars or coins |
| [Game Icons](https://kenney.nl/assets/game-icons) | Locks for the 3 rewards, trophy and medal on the reward cards, wrench for upgrade, checkmark, play | Monochrome icons. The list below comes from a third-party copy of the pack ([Korge](https://store.korge.org/gfx/kenney_game_icons/)): lock and unlock, checkmark, trophy, medal, basket, wrench, arrows, play, pause, target, gear | The count differs (105 on Kenney's page, "125+" on the copy): check the official ZIP. I did not see a coin, gift or star: **draw them as simple shapes** |

**Recolor with a single texture.** Kenney kits usually share a small palette texture. If that is the case, replacing that one texture with one in the brief's colors recolors the whole kit without retexturing anything. If not, override the materials with flat colors. Confirm in Phase 0.

**Palette (from the brief) and usage**

| Color | Hex | Use in the game |
| --- | --- | --- |
| Orange | `#F58324` | Fox, towers, Vault, path (lighter version), CTA button, coins |
| Purple | `#7845D8` | Predators, slot rings, secondary UI highlights |
| Deep ink | `#201338` | Background outside the board, text on light, tower details |
| Warm white | `#FFF6E8` | Board ground, panels, predators' eyes and details, text on dark |

**Game pieces**

- **Pet Post (tower):** level 1 = kit modular base; level 2 = one more piece; level 3 = one more piece and a bigger weapon. The pet stands on the top platform, idling, and plays its attack (or dance) clip when it shoots. If the clips have no useful names, the pet gets squash and stretch in code.
- **Predator:** purple tint on the Cube Pet (walk and run animations). Dying = gesture-negative, particle burst and a jumping coin. Leaking = the predator runs off with the coin over its head and the Wallet drops by 5.
- **Reward Vault:** built from kit tower or castle pieces in orange and cream, with the fox on top. The kit may have no chest, so the reward **does not depend on animating a lid**: the Vault glows, the locks open and the reward cards appear.
- **Coins and gems:** Unity primitives (flattened cylinder), no size cost.
- **Fox:** the mascot and the face of the ending. Use the one from the reference kit: if it is 2D art, it appears as a sprite or billboard on the Vault, in the HUD and on the end screen, animated by tween; if it is a model, a simple low-poly version. Do not invent another mascot.

**Font:** Fredoka (SIL OFL 1.1, [license in the Google Fonts repository](https://github.com/google/fonts/blob/main/ofl/fredoka/OFL.txt)), bundled in the project, no external request, only the needed characters.

**Camera and layout (portrait, 390×844 reference):**

- Fixed camera in light perspective (about 55° tilt), board filling the width.
- **Top band** (~90 px): **3 reward locks** that open one per wave, **Demo coins** balance, small restart button.
- **Middle:** board with an S-shaped path, top to bottom. Predators spawn at the top, the Reward Vault with the fox at the bottom.
- **Bottom** (~150 px, respecting the safe area): 3 game cards, each with a pet face and a genre icon.
- Phone in landscape: "Rotate your phone" notice and the game paused. In a wide desktop window: the game sits in a centered portrait column, with Deep ink on the sides.

**On-screen text (in English)**

| Where | Text |
| --- | --- |
| Phase strip | Discover · Play · Redeem |
| Balance | Demo coins |
| Wave banners | Wave 1 · Wave 2 · Final wave! |
| Cards | Pop Blaster · Puzzle Pulse · Racer Zap |
| Vault opening | Demo rewards unlocked! |
| End title | Perfect defense! / Nice defense! |
| End line | Discover games. Play and progress. Redeem rewards. |
| CTA | Explore Scrambly |
| Secondary | Play again |
| Footer | Demo only — not real earnings |
| CTA confirmation | CTA clicked — demo only |

Basic accessibility: Deep ink text on Warm white (high contrast), meaning never by color alone (each card has its own pet, icon and shape; each reward has a lock).

## 7. Technical architecture (Unity/C#, WebGL)

One scene (`Main`), few scripts, no package dependency that is not needed. Each extra package costs megabytes of the budget (section 8).

**Modules (scripts)**

| Script | Responsibility |
| --- | --- |
| `GameConfig` (ScriptableObject) | All the numbers from sections 3 and 5, nothing hard-coded |
| `GameFlow` | State machine: Intro → Wave → Breather → Redeem → EndCard |
| `WaveSpawner` | Spawns enemies per wave, reports when the wave ends |
| `EnemyAgent` | Follows the path points, health, death, leak |
| `Tower` | Picks a target, shoots, levels 1–3 (each level adds a modular piece) and drives the pet that animates the shot |
| `Projectile` | Simple projectile, from a pool |
| `Economy` | Wallet and Collected, costs, change events |
| `SlotManager` | Slots, free/occupied state, selection by screen distance |
| `InputRouter` | One pointer at a time, cancellation, double-tap filter |
| `HudView` / `TutorialHand` | Progress bar, balance, cards, guide hand |
| `RedeemSequence` / `EndCard` | Flying coins, Vault, end screen, CTA and restart |
| `PageVisibility` (+ `.jslib`) | Receives "page hidden/visible" from the browser and pauses the game |
| `ObjectPool` | Enemies, projectiles, coins, particles: no `Instantiate` during a wave |

**Input.** UI buttons use the `EventSystem`, which already handles mouse and touch. Slot and tower picking uses **screen distance** (project the slot position and compare it with the pointer), without Physics, which removes the physics module from the build. Only the first pointer is accepted, and `pointer cancel` discards the selection.

**Time and pause.** All movement, spawning and animation use scaled time (`Time.deltaTime`), and tweens are our own coroutines, with no tween library. The HTML template registers `visibilitychange` **once** and notifies the game; the game sets `Time.timeScale = 0` when hidden and back to 1 when shown. In addition, `Time.maximumDeltaTime = 0.1` so there is never a time jump on return.

**Restart.** Reload the scene (`SceneManager.LoadScene`) with the button disabled during the reload. Rules so nothing is duplicated:

- no `static` fields holding state (or reset them in `Awake`);
- every event subscribed in `OnEnable` is unsubscribed in `OnDisable`;
- `timeScale` goes back to 1 at the start of the scene;
- the JavaScript listeners live in `index.html`, which does not reload, so they do not stack.

**Resize and layout.** Canvas Scaler in "Scale With Screen Size" (390×844 reference). The camera recomputes the framing when the aspect ratio changes. The template places the canvas in a centered portrait container (width = `min(100vw, 100dvh × 0.5625)`), with a Deep ink background.

**WebGL template (own `index.html`)**

- `html, body { overflow: hidden; overscroll-behavior: none; touch-action: none; }` and a `viewport` with `user-scalable=no, viewport-fit=cover`.
- `touchmove` with `preventDefault` on the canvas and the context menu disabled, so the page does not scroll during play.
- "Rotate your phone" notice when the pointer is coarse (phone) and the screen is in landscape.
- No logos, fonts or scripts from a CDN: **no external requests**.

**CTA.** Button on the `EndCard`. On click: on-screen message "CTA clicked — demo only" and `Debug.Log` (which Unity WebGL sends to the browser console). No `Application.OpenURL` call and no navigation.

**Audio.** There will be none. Per the brief, with no audio there is no need for a mute button, and the audio module can be disabled to save size.

**Build settings for size (verify each one in the section 8 test)**

- Simple pipeline (Built-in RP with simple Unlit/Standard shaders; URP tends to grow the build, so compare).
- IL2CPP, high Managed Stripping Level, Engine Code Stripping on, exceptions off.
- Compression with **Decompression Fallback** on, because a simple static server does not send the compression headers.
- Native modules the game does not use (physics, audio, video, etc.) disabled in the Package Manager.
- Textures ≤ 512 px, low-poly meshes, no mipmaps where not needed.

## 8. 5 MB budget and size test

The brief caps the **whole ZIP** at 5,000,000 bytes, and that ZIP must contain the build, readable source code, libraries, fonts, assets, instructions and credits. The real risk of Unity WebGL is size, not gameplay. The numbers below are my working targets, **not measurements**; the test decides.

| ZIP part | Target | Limit |
| --- | --- | --- |
| WebGL build (files already compressed) | ≤ 2.8 MB | 3.5 MB |
| Source code (scripts, scene, prefabs, template, essential settings) | ≤ 0.3 MB | 0.4 MB |
| Original assets included as source (meshes, textures, font, fox) | ≤ 0.5 MB | 0.6 MB |
| README, CREDITS, project note, test log | ≤ 0.05 MB | 0.1 MB |
| **Margin** | ≥ 0.5 MB |  |

Two cautions: the build's `.br`/`.gz` files do not shrink again in the ZIP, and the assets appear **twice** (packed in the build and raw in the source folder), so every 100 KB of assets costs about 200 KB.

**Cube Pets weigh more than the rest.** They are animated (skeletal mesh and clips), unlike the static kit pieces. Import only 3 pets and only the clips used (idle and attack or dance), and measure Phase 0 with an animated pet inside, not just a cube.

**Phase 0 — size test (first 30 minutes, before any art)**

1. Project with one scene: camera, 1 kit tower, 1 animated Cube Pet, 1 sprite (the fox), 1 UI button, 1 text, all with the section 7 settings.
2. WebGL build, zip it together with the source folder and measure it in bytes.
3. Serve it with `python3 -m http.server` (the brief's simple server, no special headers) and open it in the browser. Check in the Network tab that **there are only same-origin requests**.
4. Also measure the time to first interaction on a phone, if one is available.

**Decision rule (at 0:45 at the latest)**

| Test result | Decision |
| --- | --- |
| ZIP ≤ 3.0 MB | Continue in Unity, with a comfortable budget |
| 3.0 to 3.8 MB | Continue in Unity with cuts: no TextMesh Pro (simpler own font), no URP, fewer meshes |
| > 3.8 MB, or does not open on a static server | **Plan B:** the same GDD in TypeScript + Three.js (or Canvas 2D), one bundle with Vite. The section 7 module table maps almost one to one onto TS modules |

Unity's playable ads sample only counts if it passes this same test and produces `index.html` at the root. Any reused code must be listed in `CREDITS.md`.

## 9. Brief requirements and how each one is met

Each row comes from the brief text (Technical requirements and Stage 1). The right column is what you must be able to show in the recording.

| Brief requirement | How we meet it | How to verify |
| --- | --- | --- |
| **Production ZIP** with `index.html` at the root, full build, readable code, libraries, fonts, assets, instructions | Section 12 structure; own WebGL template produces `index.html` at the root | Open the ZIP in another folder and run it from scratch |
| **Size** ≤ 5,000,000 bytes | Budget and Phase 0 (section 8) | Measure the final ZIP in bytes, note it in the README |
| **Runtime**: simple static server, no external requests, login, backend or keys | Decompression Fallback; no font, script or image from a CDN | `python3 -m http.server`; Network tab with same-origin only |
| **Layout**: touch and mouse; portrait 320×568 and 390×844; no competing scroll | Canvas Scaler 390×844, no-scroll CSS, `preventDefault` on touch; rotate notice on a phone in landscape | Emulate both sizes and test with mouse and touch |
| **CTA**: local confirmation and console log, without leaving the page | "CTA clicked — demo only" message + `Debug.Log`; no `OpenURL` | Click and open the console; confirm the URL does not change |
| **Visibility**: pause the game and clocks when the page is hidden; resume with no jump | `visibilitychange` → `timeScale = 0`; scaled time everywhere; `maximumDeltaTime` 0.1 | Hide the tab in the middle of a wave, wait 10 s, come back |
| **Reliability**: resize, interrupted input, design outcomes, clean restart | Sections 4 and 7; scene reload with no static state | Test matrix (section 11) |
| **Handoff**: instructions, browsers and devices tested (real or emulated), limitations, what was not tested | `README.md` with a test table | Fill it in during testing, not at the end |
| **Understandable, responsive interaction** | One tap primitive, guide hand, feedback within 1 frame | Someone who has never seen it plays without explanation |
| **Purposeful progression and a clear end** | Three waves, three locks, Redeem, end card | Play from start to end without touching anything: it still reaches the end |
| **Meaningful connection to Scrambly** | Section 2 | Say in one sentence how each phase maps to a product step |
| **Restart easy to review** | Small button always visible at the top, plus "Play again" on the end screen | Restart 10 times in a row, including in the middle of a wave |

**Conditional requirements (the brief says they only apply if the design uses them):** no audio, so **no mute button**; no countdown, so **no time's-up case**. The spawn and wave clocks are still paused by the visibility rule.

## 10. 6-hour plan, decision points and cuts

The limit is 6 hours of work. Times below count from the start of the project, with screen recording on from minute 0 (the brief wants to see everything from the interpretation of the brief onward).

| Window | Deliverable | Decision point |
| --- | --- | --- |
| 0:00–0:45 | Phase 0: size test (section 8) with fox, 1 tower, 1 animated pet, UI and a build on a static server | **G0:** ZIP within target? If not, Plan B (TS + Three.js) right away |
| 0:45–1:45 | Greybox core: board, path, walking lion, slots, 1 shooting tower, coins | Tapping works with mouse and emulated touch |
| 1:45–2:45 | 3 towers, upgrades, 3 waves, economy, progress bar, leak | **G1:** playable start to end in greybox |
| 2:45–3:45 | Art: palette materials, fox, final UI, particles, hit and upgrade feedback | Readable at 320×568? |
| 3:45–4:30 | Redeem, end card, CTA, restart, guide hand | Full flow in one playthrough |
| 4:30–5:30 | Hardening: visibility, resize, interrupted input, 320×568 and 390×844 tests, final ZIP measured | Test matrix green |
| 5:30–6:00 | README, `CREDITS.md`, project note, test log, check the "Before you submit" list | Deliver what exists and note what is missing |

**G1 in detail.** If the loop is not fun at the 2:45 mark, tune numbers for up to 30 minutes. If it still does not work, **simplify** (2 towers, 2 waves) instead of changing games: with the ZIP, recording and testing requirements, a switch after G1 is not realistic. I had suggested the runner as a game plan B; it only applies before G1.

**Cut order if running late (top to bottom)**

1. Third tower (Racer Zap): two remain.
2. Tower level 3: two levels remain.
3. Extra particles and screen shake.
4. Elaborate Vault animation: replaced by a simple glow with fade.
5. Polar bear (Hauler): wave 3 uses only lions and tigers.
6. Step 3 of the guide hand.

**Never cut:** restart, CTA with confirmation and log, visibility pause, ZIP size test, README with the test table, `CREDITS.md`.

**Time log:** keep a `TIME_LOG.md` with the start and end time of each block. The 5-minute script asks for "time spent", and the brief warns that when the 6 hours are reached, you should deliver the current version and list what was left unfinished or untested.

## 11. Tests

The "Technical execution" criterion (25%) mentions credible tests. Each row below becomes a README row, with the environment and whether it was **real or emulated**. Anything not tested must be stated as not tested.

| Area | Case | Expected |
| --- | --- | --- |
| Screen | Portrait 320×568 and 390×844 | Everything visible, cards tappable, nothing cut |
| Screen | Wide desktop window | Centered portrait column, Deep ink background |
| Screen | Phone in landscape | "Rotate your phone" notice, game paused, resumes when rotated back |
| Screen | Resize the window mid-wave | Framing and UI adjust, the game continues |
| Input | Mouse and touch | Same behavior on both |
| Input | Two fingers at once; canceled touch | Only the first pointer counts; selection discarded |
| Input | Tap a card without coins | Card shakes, nothing is built |
| Input | Page scroll during play | Page does not scroll |
| Flow | Build nothing | Waves pass, the game reaches the end |
| Flow | Build and upgrade everything the balance allows | No errors, balance never negative |
| Restart | Mid-wave, 10 times in a row, on the end screen | Clean state, no duplicated enemies or events, same pacing as the first run |
| Visibility | Hide the tab mid-wave for 10 s and come back | Enemies frozen while hidden, resume with no time jump |
| CTA | Click **Explore Scrambly** | Message "CTA clicked — demo only", console log, URL unchanged |
| Network | Network tab with the simple static server | Only same-origin requests, no errors |
| Package | ZIP in a clean folder | Opens, `index.html` at the root, ≤ 5,000,000 bytes |

**Edge case to show in the script:** hide the tab in the middle of a wave and come back. It is the case the brief describes most specifically and it is easy to prove in the recording.

**Environments to record (fill in what you actually use):** Chrome on desktop; device emulation at 320×568 and 390×844; a real Android phone or iPhone, if possible; a second browser (Firefox or Safari). For each one, note real or emulated and the test date.

## 12. Delivery and process

There are **three deliverables** (Stage 1): the production ZIP, the recording of the entire process and the 5-minute walkthrough. The links must be public.

**ZIP structure**

```text
index.html            <- root, mandatory
Build/ TemplateData/  <- WebGL build
README.md             <- how to run, environments tested (real/emulated), limitations, what was not tested, size
CREDITS.md            <- assets, font, tools, reused code, AI use
PROJECT_NOTE.md       <- what I used, what I contributed, one decision/correction/rejected output
TIME_LOG.md           <- time per block
source/               <- Assets/Scripts, scene, prefabs, WebGL template, essential settings (no Library/Temp)
```

**Project note (required):** say what was used (Unity, asset pack, AI), what you contributed and **one decision, correction or rejected output that improved the result**. You need to understand the implementation and be able to modify it. Two real candidates from this session, if you want to use them in your own words:

- An initial AI suggestion used the egg as the mascot (from the public website); the brief provides a **fox**, and the art direction was corrected before starting.
- The 5 MB constraint made Unity conditional, and that became the Phase 0 size test with a decision rule.

**Recording of the full process.** Turn on screen recording at the start and leave it running. The brief wants to see, from the interpretation of the brief to the final build: tools, prompts, iterations, accepted and rejected outputs, corrections and where you applied your own judgment. Show how you evaluated and improved what the AI delivered, not just that you used it.

**5-minute walkthrough (script)**

| Time | Content |
| --- | --- |
| 0:00–0:30 | Concept and how Discover, Play and Redeem become mechanics |
| 0:30–1:45 | Live play: build, upgrade, waves |
| 1:45–2:30 | Progression and ending: locks, Vault, end screen |
| 2:30–2:50 | CTA, with the browser console visible |
| 2:50–3:10 | Restart |
| 3:10–3:55 | Edge case: hide the tab mid-wave and come back |
| 3:55–5:00 | Decisions, time spent, tools and reuse, tests, limitations and the most important change |

**Stage 2 (if invited).** Mark Stage 1 with a Git tag (for example `stage1-final`) and keep the numbers in the `GameConfig`, because the adjustment request is usually about balance, clarity or polish. The revision goes in a new version or tag, without changing Stage 1, and the scope is the same game: a new game or an engine switch is out. Extras are not rewarded.

**Brief points to check before submitting**

- [ ] The ZIP opens, has `index.html` at the root and is within 5 MB.
- [ ] The ZIP includes readable code, instructions and credits for assets and tools.
- [ ] Input, layout, CTA, restart, visibility and edge cases were tested.
- [ ] The recording of the full process is accessible and shows the use of AI.
- [ ] The 5-minute walkthrough covers decisions, time, tools, tests, limitations and changes.

## 13. Risks and questions for the Simula contact

| Risk | Mitigation |
| --- | --- |
| Unity WebGL build blows the 5 MB (build + source + assets) | Phase 0, section 8 decision rule and Plan B in TS + Three.js |
| A simple static server does not serve Unity's compressed build | Decompression Fallback and a test with `python3 -m http.server` in Phase 0 |
| The TD scope grows beyond the time | Closed scope from section 1, G1 at 2:45, section 10 cut list |
| Bad balance makes the game too easy or unfair | Everything in the `GameConfig`; about 1 h of playtesting; with no defeat, the worst case is just leaking coins |
| Animated Cube Pets weigh more and the clip names are unknown | Import only 3 pets and only the clips used; measure Phase 0 with an animated pet; fallback: squash and stretch in code |
| The 3 predators add up to 444 KB (lion 173, tiger 172, bear 99), and raw assets reach ~1.04 MB | Cut the cat (165 KB), then the scenery details; reuse the same model recolored and scaled; measure in Phase 0 |
| Game Icons has no coin, gift or star, and the count differs (105 on Kenney's page, "125+" on a third-party copy) | Draw those shapes as simple sprites; check the official ZIP |
| The reference kit's fox clashes with the cubic pets, or does not work in 3D | Fox as a 2D mascot on the Vault, in the HUD and on the end screen, with the cubic pets as the team she leads (kit not seen yet) |
| WebGL memory on iPhone | Low initial memory, small textures, test on a real device if available |
| Uncertain asset license | The 4 chosen packs are CC0 (checked on Kenney's pages) and the Fredoka font is OFL 1.1; even so, everything is listed in `CREDITS.md` |
| Looking like a "generic ad" (30% criterion) | Three phases labeled Discover/Play/Redeem, reward locks and an end screen with the product line |
| Running out of time to document | Fixed 30-minute block at the end and `TIME_LOG.md` throughout the work |

**Questions for the Simula contact (the brief says to ask when something is unclear)**

1. Does recording the 5-minute walkthrough count within the 6 hours of work?
2. Can the "readable source code" be just `Assets/Scripts`, the scene and the template, instead of the full Unity project, given the 5 MB limit for everything?
3. Does the reference kit have usage or format restrictions on the fox that I should respect?

## Sources

- [Simula Playable Game Developer Take-Home (brief)](https://simula-ad.notion.site/Simula-Playable-Game-Developer-Take-Home-321af70f6f0d80f4b560ef057587f77b)
- [Scrambly](https://scrambly.io/)
