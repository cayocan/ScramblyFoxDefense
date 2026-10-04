# Scrambly Fox Defense

Web playable (Unity 6000.3.19f1, C#, WebGL) for the Simula take-home, advertiser Scrambly. 6-hour work limit.

## Language rule (mandatory)
Everything in the project MUST be written in English: code, comments, commit messages, branch names, UI text, documentation, variable names, logs, error messages — absolutely everything. No exceptions.
Lean 45–60 s tower defense: the fox leads Cube Pets against predators (lion, tiger, polar bear) that steal demo coins. Flow Discover → Play → Redeem, ending with the "Explore Scrambly" CTA (simulated).

## Workflow (mandatory)
- Each feature lives on `feature/<name>`. Create it with `.\tools\start-feature.ps1 <name>`; this records the start time.
- Every feature ends in a **merge commit** on `main` that states its duration (start to end, no pauses): `.\tools\finish-feature.ps1 "summary"` (`--no-ff` merge with "Duracao da feature (inicio ate o fim): Xh YYmin").
- Never merge a feature without that commit. Never commit directly to `main`, except the initial setup.
- Also record it in `TIME_LOG.md` (the submission asks for time spent and AI decisions).
- The scripts use ASCII-only messages, to avoid garbled text in PowerShell 5.
- Unity generates `.meta` files when importing assets; version them together with the asset.

## Brief constraints (non-negotiable)
- Production ZIP ≤ 5,000,000 bytes, with `index.html` at the root, build + readable code + assets + instructions.
- Runs on a static HTTP server, with no external requests, login or backend. No Decompression headers: use Decompression Fallback.
- Touch and mouse. Test 320×568 and 390×844 (portrait) with a rotate prompt in landscape. No page scroll.
- CTA with a clear label; on click it shows "CTA clicked — demo only", logs to the console and does not navigate.
- Pause the game and clocks when the page is hidden and resume with no time jump (use scaled time only).
- Restart resets everything with no duplicated timers/listeners/effects.
- The balance is demo: never promise real earnings or payment.
- Audio: Web Audio synth in the WebGL template (`window.scramblySfx`) via `Assets/Plugins/WebGL/Sfx.jslib`; no audio files, no Unity audio module. Starts after the first gesture, mute button in the HUD, suspended while the page is hidden or in landscape.
- Palette: Orange #F58324, Purple #7845D8, Deep ink #201338, Warm white #FFF6E8.

## Size budget
Build target ≤ 2.8 MB (hard limit 3.5), code ≤ 0.4 MB, raw assets ≤ ~1 MB, margin ≥ 0.5 MB. Phase 0: measure an empty WebGL build in the first 45 min. If it goes over 3.8 MB, plan B: TypeScript + Three.js.

## Assets (Kenney, CC0): Tower Defense Kit, Cube Pets, UI Pack, Game Icons
- Already imported into `Assets/Art/`: Cube Pets (fox, dog, cat, lion, tiger, polar bear) and Tower Defense Kit (tile, tile-straight, tile-corner-round, tile-spawn, tile-end, tower-round-base, tower-square-bottom-a, selection-a). UI Pack and Game Icons not yet.
- The files are `.glb`. Unity 6 does not import glTF/GLB on its own: an importer is needed (for example the glTFast package, editor only) or a conversion to FBX. Decide and measure the size impact in Phase 0.
- Each GLB uses the single `colormap.png` texture; swapping the texture recolors the kit.
- Defenders: `animal-fox`, `animal-dog`, `animal-cat`, each **on top of a tower base** (animal visible on the tower).
- Enemies: `animal-lion` (default), `animal-tiger` (fast), `animal-polar` (tank). Do not use the kit's UFOs.
- Size cuts, in order: cat, scenery details.
- The original packs stay outside the repository; only the files in use go into `Assets/`.

## Unity editor automation (unattended / remote sessions)
- Drive the open editor with the Unity CLI + Pipeline: `unity --no-banner command <name> ...` (`recompile`, `recompile_status`, `menu --path "Scrambly/Build Main Scene"`, `build --target WebGL --outputPath Builds/WebGL --confirm true` then poll `build_status`, `eval`/`eval_file`, `editor_play`/`editor_stop`, `capture_game_view`).
- Compile check: trust `recompile_status` ("completed" with no errors) plus a type lookup via `eval`; `console_status.compilationFailed` can lag.
- If commands time out ("Main thread operation timed out"), the editor is either throttled in the background or blocked by a modal dialog. Use `tools/unity-window.ps1` (Win32, works when the Pipeline is stuck): `status` (lists dialogs and buttons), `click "<button>"`, `focus`, `shot <png>`.
- During long or unattended sessions run `tools/unity-window.ps1 watch keepfocus` in the background: it auto-answers known-safe dialogs (scene modified externally → Reload), logs any other dialog to `Logs/unity-window-watch.log` as NEEDS DECISION, keeps Unity in front every 30 s and keeps the PC awake. Never auto-click unknown dialogs: read them with `status`/`shot` and decide.
- Play Mode does not advance frames while Unity is in the background; for logic checks use `Assets/Editor/SessionSimulator.cs` (ticks the real services) and `tools/browser-test` (headless Chrome against `python -m http.server 8080` in `Builds/WebGL`).

## Additional context
Put context documents (GDD, notes, references) in `docs/rag/`. Read `docs/rag/README.md` before deciding scope.

# context-mode — MANDATORY routing rules

You have context-mode MCP tools available. These rules are NOT optional — they protect your context window from flooding. A single unrouted command can dump 56 KB into context and waste the entire session.

## BLOCKED commands — do NOT attempt these

### curl / wget — BLOCKED
Any Bash command containing `curl` or `wget` is intercepted and replaced with an error message. Do NOT retry.
Instead use:
- `ctx_fetch_and_index(url, source)` to fetch and index web pages
- `ctx_execute(language: "javascript", code: "const r = await fetch(...)")` to run HTTP calls in sandbox

### Inline HTTP — BLOCKED
Any Bash command containing `fetch('http`, `requests.get(`, `requests.post(`, `http.get(`, or `http.request(` is intercepted and replaced with an error message. Do NOT retry with Bash.
Instead use:
- `ctx_execute(language, code)` to run HTTP calls in sandbox — only stdout enters context

### WebFetch — BLOCKED
WebFetch calls are denied entirely. The URL is extracted and you are told to use `ctx_fetch_and_index` instead.
Instead use:
- `ctx_fetch_and_index(url, source)` then `ctx_search(queries)` to query the indexed content

## REDIRECTED tools — use sandbox equivalents

### Bash (>20 lines output)
Bash is ONLY for: `git`, `mkdir`, `rm`, `mv`, `cd`, `ls`, `npm install`, `pip install`, and other short-output commands.
For everything else, use:
- `ctx_batch_execute(commands, queries)` — run multiple commands + search in ONE call
- `ctx_execute(language: "shell", code: "...")` — run in sandbox, only stdout enters context

### Read (for analysis)
If you are reading a file to **Edit** it → Read is correct (Edit needs content in context).
If you are reading to **analyze, explore, or summarize** → use `ctx_execute_file(path, language, code)` instead. Only your printed summary enters context. The raw file content stays in the sandbox.

### Grep (large results)
Grep results can flood context. Use `ctx_execute(language: "shell", code: "grep ...")` to run searches in sandbox. Only your printed summary enters context.

## Tool selection hierarchy

1. **GATHER**: `ctx_batch_execute(commands, queries)` — Primary tool. Runs all commands, auto-indexes output, returns search results. ONE call replaces 30+ individual calls.
2. **FOLLOW-UP**: `ctx_search(queries: ["q1", "q2", ...])` — Query indexed content. Pass ALL questions as array in ONE call.
3. **PROCESSING**: `ctx_execute(language, code)` | `ctx_execute_file(path, language, code)` — Sandbox execution. Only stdout enters context.
4. **WEB**: `ctx_fetch_and_index(url, source)` then `ctx_search(queries)` — Fetch, chunk, index, query. Raw HTML never enters context.
5. **INDEX**: `ctx_index(content, source)` — Store content in FTS5 knowledge base for later search.

## Subagent routing

When spawning subagents (Agent/Task tool), the routing block is automatically injected into their prompt. Bash-type subagents are upgraded to general-purpose so they have access to MCP tools. You do NOT need to manually instruct subagents about context-mode.

## Output constraints

- Keep responses under 500 words.
- Write artifacts (code, configs, PRDs) to FILES — never return them as inline text. Return only: file path + 1-line description.
- When indexing content, use descriptive source labels so others can `ctx_search(source: "label")` later.

## ctx commands

| Command | Action |
|---------|--------|
| `ctx stats` | Call the `ctx_stats` MCP tool and display the full output verbatim |
| `ctx doctor` | Call the `ctx_doctor` MCP tool, run the returned shell command, display as checklist |
| `ctx upgrade` | Call the `ctx_upgrade` MCP tool, run the returned shell command, display as checklist |
